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
using Customer = Core.Infrastructure.Customer;
using CustomerService_Stripe = Stripe.CustomerService;

namespace Core.Services
{
    public class CustomerService : BaseService, ICustomerService
    {
        private readonly ICustomerRepository _customerRepo;
        private readonly ITenantRepository _tenantRepo;
        private readonly IEncryptionService _encryption;

        public CustomerService(ITenantContextProvider tenantContextProvider, ICustomerRepository customerRepo, ITenantRepository tenantRepo, IEncryptionService encryption) : base(tenantContextProvider)
        {
            _customerRepo = customerRepo;
            _tenantRepo = tenantRepo;
            _encryption = encryption;
        }

        private async Task<StripeClient> GetStripeClientAsync()
        {
            var tenant = await _tenantRepo.GetByIdAsync(CurrentTenantContext.TenantId);
            if (tenant == null || string.IsNullOrEmpty(tenant.StripeSecretKeyEnc))
                return new StripeClient("sk_test_placeholder");
            return new StripeClient(_encryption.Decrypt(tenant.StripeSecretKeyEnc));
        }

        public async Task<GatewayResponseWrapper<CustomerResponseDto>> CreateAsync(CreateCustomerDto request)
        {
            var response = new GatewayResponseWrapper<CustomerResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var existing = await _customerRepo.GetByEmailAsync(tenantId, request.Email);
            if (existing != null)
            {
                response.SetError("A customer with this email already exists.");
                return response;
            }

            string stripeCustomerId = null;
            try
            {
                var client = await GetStripeClientAsync();
                var stripeService = new CustomerService_Stripe(client);
                var stripeCustomer = await stripeService.CreateAsync(new CustomerCreateOptions
                {
                    Email = request.Email,
                    Name = request.Name,
                    Phone = request.Phone,
                    Metadata = request.Metadata ?? new Dictionary<string, string>()
                });
                stripeCustomerId = stripeCustomer.Id;
            }
            catch { /* Stripe sync failure is non-blocking */ }

            var customer = new Customer
            {
                TenantId = tenantId,
                ExternalReferenceId = request.ExternalReferenceId,
                StripeCustomerId = stripeCustomerId,
                Email = request.Email,
                Name = request.Name,
                Phone = request.Phone,
                Currency = request.Currency ?? "usd",
                BillingAddress = request.BillingAddress,
                TaxId = request.TaxId,
                Metadata = request.Metadata != null ? JsonConvert.SerializeObject(request.Metadata) : null
            };

            await _customerRepo.CreateAsync(customer);
            response.SetSuccess(MapCustomer(customer));
            return response;
        }

        public async Task<GatewayResponseWrapper<CustomerDetailResponseDto>> GetAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<CustomerDetailResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var customer = await _customerRepo.GetByIdWithDetailsAsync(tenantId, id);
            if (customer == null)
            {
                response.SetError("Customer not found.", 404);
                return response;
            }

            var detail = new CustomerDetailResponseDto
            {
                Id = customer.Id,
                ExternalReferenceId = customer.ExternalReferenceId,
                StripeCustomerId = customer.StripeCustomerId,
                Email = customer.Email,
                Name = customer.Name,
                Phone = customer.Phone,
                Currency = customer.Currency,
                BillingAddress = customer.BillingAddress,
                TaxId = customer.TaxId,
                SubscriptionCount = customer.Subscriptions.Count(s => s.Status == "active" || s.Status == "trialing"),
                TotalSpent = customer.Transactions.Where(t => t.Status == "succeeded").Sum(t => t.Amount),
                CreatedAt = customer.CreatedAt,
                Subscriptions = customer.Subscriptions.Select(s => new SubscriptionResponseDto
                {
                    Id = s.Id, CustomerId = s.CustomerId, PlanId = s.PlanId, PlanName = s.Plan?.Name, PlanAmount = s.Plan?.Amount ?? 0,
                    StripeSubscriptionId = s.StripeSubscriptionId, Status = s.Status, Quantity = s.Quantity,
                    CurrentPeriodStart = s.CurrentPeriodStart, CurrentPeriodEnd = s.CurrentPeriodEnd,
                    TrialEnd = s.TrialEnd, CancelAtPeriodEnd = s.CancelAtPeriodEnd, CreatedAt = s.CreatedAt
                }).ToList(),
                RecentTransactions = customer.Transactions.Select(t => new PaymentResponseDto
                {
                    Id = t.Id, Amount = t.Amount, Currency = t.Currency, Status = t.Status, Type = t.Type,
                    PaymentMethodLast4 = t.PaymentMethodLast4, Description = t.Description, CreatedAt = t.CreatedAt
                }).ToList(),
                Invoices = customer.Invoices.Select(i => new InvoiceResponseDto
                {
                    Id = i.Id, InvoiceNumber = i.InvoiceNumber, Total = i.Total, Status = i.Status,
                    Currency = i.Currency, PaidAt = i.PaidAt, CreatedAt = i.CreatedAt
                }).ToList()
            };

            response.SetSuccess(detail);
            return response;
        }

        public async Task<GatewayResponseWrapper<CustomerResponseDto>> GetByExternalRefAsync(string externalRefId)
        {
            var response = new GatewayResponseWrapper<CustomerResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var customer = await _customerRepo.GetByExternalRefAsync(tenantId, externalRefId);
            if (customer == null) { response.SetError("Customer not found.", 404); return response; }
            response.SetSuccess(MapCustomer(customer));
            return response;
        }

        public async Task<GatewayResponseWrapper<CustomerResponseDto>> UpdateAsync(Guid id, UpdateCustomerDto request)
        {
            var response = new GatewayResponseWrapper<CustomerResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var customer = await _customerRepo.GetByIdWithDetailsAsync(tenantId, id);
            if (customer == null) { response.SetError("Customer not found.", 404); return response; }

            if (!string.IsNullOrEmpty(request.Email)) customer.Email = request.Email;
            if (!string.IsNullOrEmpty(request.Name)) customer.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Phone)) customer.Phone = request.Phone;
            if (!string.IsNullOrEmpty(request.Currency)) customer.Currency = request.Currency;
            if (!string.IsNullOrEmpty(request.BillingAddress)) customer.BillingAddress = request.BillingAddress;
            if (!string.IsNullOrEmpty(request.TaxId)) customer.TaxId = request.TaxId;
            customer.UpdatedAt = DateTime.UtcNow;

            await _customerRepo.UpdateAsync(customer);
            response.SetSuccess(MapCustomer(customer));
            return response;
        }

        public async Task<GatewayPaginatedListResponseWrapper<CustomerResponseDto>> ListAsync(CustomerFilterDto filter)
        {
            var response = new GatewayPaginatedListResponseWrapper<CustomerResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var query = _customerRepo.Query(tenantId);

            if (!string.IsNullOrEmpty(filter.Search))
                query = query.Where(c => c.Name.Contains(filter.Search) || c.Email.Contains(filter.Search));
            if (filter.HasSubscription == true)
                query = query.Where(c => c.Subscriptions.Any(s => s.Status == "active" || s.Status == "trialing"));
            if (filter.HasSubscription == false)
                query = query.Where(c => !c.Subscriptions.Any(s => s.Status == "active" || s.Status == "trialing"));

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(c => c.CreatedAt).Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();

            response.SetSuccessWithPagination(items.Select(MapCustomer).ToList(), filter.Page, filter.PageSize, totalCount);
            return response;
        }

        public async Task<GatewayResponseWrapper<string>> CreatePortalSessionAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<string>();
            var tenantId = CurrentTenantContext.TenantId;
            var customer = await _customerRepo.GetByIdWithDetailsAsync(tenantId, id);
            if (customer == null || string.IsNullOrEmpty(customer.StripeCustomerId))
            { response.SetError("Customer not found or Stripe not linked.", 404); return response; }

            try
            {
                var client = await GetStripeClientAsync();
                var service = new Stripe.BillingPortal.SessionService(client);
                var session = await service.CreateAsync(new Stripe.BillingPortal.SessionCreateOptions { Customer = customer.StripeCustomerId });
                response.SetSuccess(session.Url);
            }
            catch (StripeException ex) { response.SetError($"Stripe error: {ex.Message}", 400); }
            return response;
        }

        private CustomerResponseDto MapCustomer(Customer c) => new()
        {
            Id = c.Id, ExternalReferenceId = c.ExternalReferenceId, StripeCustomerId = c.StripeCustomerId,
            Email = c.Email, Name = c.Name, Phone = c.Phone, Currency = c.Currency,
            SubscriptionCount = c.Subscriptions?.Count(s => s.Status == "active" || s.Status == "trialing") ?? 0,
            TotalSpent = c.Transactions?.Where(t => t.Status == "succeeded").Sum(t => t.Amount) ?? 0,
            CreatedAt = c.CreatedAt
        };
    }
}
