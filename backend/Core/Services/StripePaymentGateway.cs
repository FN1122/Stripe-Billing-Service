using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.ServiceContracts;
using Core.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Stripe;
using Stripe.Checkout;

namespace Core.Services
{
    public class StripePaymentGateway : BaseService, IPaymentGateway
    {
        private readonly IPaymentTransactionRepository _transactionRepo;
        private readonly ICustomerRepository _customerRepo;
        private readonly ITenantRepository _tenantRepo;
        private readonly IEncryptionService _encryption;
        private readonly IWebhookDispatchService _webhookDispatch;

        public StripePaymentGateway(ITenantContextProvider tenantContextProvider, IPaymentTransactionRepository transactionRepo, ICustomerRepository customerRepo, ITenantRepository tenantRepo, IEncryptionService encryption, IWebhookDispatchService webhookDispatch) : base(tenantContextProvider)
        {
            _transactionRepo = transactionRepo;
            _customerRepo = customerRepo;
            _tenantRepo = tenantRepo;
            _encryption = encryption;
            _webhookDispatch = webhookDispatch;
        }

        private async Task<StripeClient> GetStripeClientAsync()
        {
            var tenant = await _tenantRepo.GetByIdAsync(CurrentTenantContext.TenantId);
            if (tenant == null || string.IsNullOrEmpty(tenant.StripeSecretKeyEnc)) return new StripeClient("sk_test_placeholder");
            return new StripeClient(_encryption.Decrypt(tenant.StripeSecretKeyEnc));
        }

        public async Task<GatewayResponseWrapper<CheckoutResponseDto>> CreateCheckoutSessionAsync(CreateCheckoutDto request)
        {
            var response = new GatewayResponseWrapper<CheckoutResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            try
            {
                var client = await GetStripeClientAsync();
                var lineItems = request.LineItems.Select(item => new SessionLineItemOptions
                {
                    PriceData = string.IsNullOrEmpty(item.StripePriceId) ? new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Amount * 100), Currency = item.Currency ?? "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions { Name = item.Name, Description = item.Description }
                    } : null,
                    Price = !string.IsNullOrEmpty(item.StripePriceId) ? item.StripePriceId : null, Quantity = item.Quantity
                }).ToList();
                var options = new SessionCreateOptions
                {
                    LineItems = lineItems, Mode = request.Mode, SuccessUrl = request.SuccessUrl,
                    CancelUrl = request.CancelUrl, Metadata = request.Metadata ?? new Dictionary<string, string>()
                };
                if (!string.IsNullOrEmpty(request.CustomerEmail)) options.CustomerEmail = request.CustomerEmail;
                var service = new SessionService(client);
                var session = await service.CreateAsync(options);
                var transaction = new PaymentTransaction
                {
                    TenantId = tenantId, StripeCheckoutSessionId = session.Id,
                    Amount = request.LineItems.Sum(l => l.Amount * l.Quantity),
                    Currency = request.LineItems.FirstOrDefault()?.Currency ?? "usd", Status = "pending",
                    Type = request.Mode == "subscription" ? "subscription" : "one_time",
                    Description = $"Checkout: {string.Join(", ", request.LineItems.Select(l => l.Name))}",
                    Metadata = request.Metadata != null ? JsonConvert.SerializeObject(request.Metadata) : null
                };
                if (request.CustomerId.HasValue) transaction.CustomerId = request.CustomerId;
                await _transactionRepo.CreateAsync(transaction);
                response.SetSuccess(new CheckoutResponseDto { SessionId = session.Id, Url = session.Url, TransactionId = transaction.Id });
            }
            catch (StripeException ex) { response.SetError($"Stripe error: {ex.Message}", 400); }
            return response;
        }

        public async Task<GatewayResponseWrapper<PaymentIntentResponseDto>> CreatePaymentIntentAsync(CreatePaymentIntentDto request)
        {
            var response = new GatewayResponseWrapper<PaymentIntentResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            try
            {
                var client = await GetStripeClientAsync();
                var customer = await _customerRepo.GetByIdWithDetailsAsync(tenantId, request.CustomerId);
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(request.Amount * 100), Currency = request.Currency ?? "usd",
                    Description = request.Description, Metadata = request.Metadata ?? new Dictionary<string, string>()
                };
                if (customer?.StripeCustomerId != null) options.Customer = customer.StripeCustomerId;
                if (!string.IsNullOrEmpty(request.PaymentMethodId)) options.PaymentMethod = request.PaymentMethodId;
                var service = new PaymentIntentService(client);
                var intent = await service.CreateAsync(options);
                var transaction = new PaymentTransaction
                {
                    TenantId = tenantId, CustomerId = request.CustomerId, StripePaymentIntentId = intent.Id,
                    Amount = request.Amount, Currency = request.Currency ?? "usd",
                    Status = intent.Status == "succeeded" ? "succeeded" : "pending", Type = "one_time",
                    Description = request.Description,
                    Metadata = request.Metadata != null ? JsonConvert.SerializeObject(request.Metadata) : null
                };
                await _transactionRepo.CreateAsync(transaction);
                response.SetSuccess(new PaymentIntentResponseDto
                {
                    PaymentIntentId = intent.Id, ClientSecret = intent.ClientSecret, Status = intent.Status,
                    Amount = request.Amount, Currency = request.Currency ?? "usd", TransactionId = transaction.Id
                });
            }
            catch (StripeException ex) { response.SetError($"Stripe error: {ex.Message}", 400); }
            return response;
        }

        public async Task<GatewayResponseWrapper<PaymentResponseDto>> GetPaymentAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<PaymentResponseDto>();
            var tx = await _transactionRepo.GetByIdWithCustomerAsync(CurrentTenantContext.TenantId, id);
            if (tx == null) { response.SetError("Payment not found.", 404); return response; }
            response.SetSuccess(MapPayment(tx));
            return response;
        }

        public async Task<GatewayPaginatedListResponseWrapper<PaymentResponseDto>> ListPaymentsAsync(PaymentFilterDto filter)
        {
            var response = new GatewayPaginatedListResponseWrapper<PaymentResponseDto>();
            var query = _transactionRepo.Query(CurrentTenantContext.TenantId);
            if (!string.IsNullOrEmpty(filter.Status)) query = query.Where(t => t.Status == filter.Status);
            if (filter.MinAmount.HasValue) query = query.Where(t => t.Amount >= filter.MinAmount);
            if (filter.MaxAmount.HasValue) query = query.Where(t => t.Amount <= filter.MaxAmount);
            if (filter.DateFrom.HasValue) query = query.Where(t => t.CreatedAt >= filter.DateFrom);
            if (filter.DateTo.HasValue) query = query.Where(t => t.CreatedAt <= filter.DateTo);
            if (!string.IsNullOrEmpty(filter.Search))
                query = query.Where(t => t.Customer != null && (t.Customer.Name.Contains(filter.Search) || t.Customer.Email.Contains(filter.Search)));
            query = filter.SortOrder == "asc" ? query.OrderBy(t => t.CreatedAt) : query.OrderByDescending(t => t.CreatedAt);
            var totalCount = await query.CountAsync();
            var items = await query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
            response.SetSuccessWithPagination(items.Select(MapPayment).ToList(), filter.Page, filter.PageSize, totalCount);
            return response;
        }

        public async Task<GatewayResponseWrapper<PaymentAnalyticsDto>> GetPaymentAnalyticsAsync(string period)
        {
            var response = new GatewayResponseWrapper<PaymentAnalyticsDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var days = period switch { "7d" => 7, "90d" => 90, "12m" => 365, _ => 30 };
            var since = DateTime.UtcNow.AddDays(-days);
            var transactions = await _transactionRepo.GetByTenantIdSinceAsync(tenantId, since);
            var succeeded = transactions.Where(t => t.Status == "succeeded").ToList();
            var failed = transactions.Where(t => t.Status == "failed").ToList();
            var analytics = new PaymentAnalyticsDto
            {
                TotalRevenue = succeeded.Sum(t => t.Amount), NetRevenue = succeeded.Sum(t => t.Amount - t.AmountRefunded),
                TransactionCount = transactions.Count, SuccessCount = succeeded.Count, FailedCount = failed.Count,
                SuccessRate = transactions.Count > 0 ? Math.Round((decimal)succeeded.Count / transactions.Count * 100, 1) : 0,
                AverageTransactionValue = succeeded.Count > 0 ? Math.Round(succeeded.Average(t => t.Amount), 2) : 0,
                RevenueByDay = succeeded.GroupBy(t => t.CreatedAt.Date).OrderBy(g => g.Key)
                    .Select(g => new RevenueDataPoint { Date = g.Key.ToString("yyyy-MM-dd"), Amount = g.Sum(t => t.Amount), Count = g.Count() }).ToList()
            };
            response.SetSuccess(analytics);
            return response;
        }

        private static PaymentResponseDto MapPayment(PaymentTransaction tx) => new()
        {
            Id = tx.Id, CustomerId = tx.CustomerId, CustomerName = tx.Customer?.Name, CustomerEmail = tx.Customer?.Email,
            StripePaymentIntentId = tx.StripePaymentIntentId, Amount = tx.Amount, AmountRefunded = tx.AmountRefunded,
            Currency = tx.Currency, Status = tx.Status, Type = tx.Type, PaymentMethod = tx.PaymentMethod,
            PaymentMethodLast4 = tx.PaymentMethodLast4, PaymentMethodBrand = tx.PaymentMethodBrand,
            Description = tx.Description, FailureReason = tx.FailureReason, ReceiptUrl = tx.ReceiptUrl, CreatedAt = tx.CreatedAt
        };
    }
}
