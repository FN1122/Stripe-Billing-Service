using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Infrastructure;
using Core.ServiceContracts;
using Core.Utils;
using Microsoft.EntityFrameworkCore;

namespace Core.Services
{
    public class ConnectService : BaseService, IConnectService
    {
        private readonly BillingDbContext _dbContext;

        public ConnectService(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
        }

        public async Task<GatewayResponseWrapper<ConnectedAccountResponseDto>> CreateAccountAsync(CreateConnectedAccountDto request)
        {
            var response = new GatewayResponseWrapper<ConnectedAccountResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var account = new ConnectedAccount
            {
                TenantId = tenantId,
                Email = request.Email,
                BusinessName = request.BusinessName,
                Country = request.Country,
                Type = request.Type,
                PlatformFeePercent = request.PlatformFeePercent,
                PlatformFeeFixed = request.PlatformFeeFixed
            };

            _dbContext.ConnectedAccounts.Add(account);
            await _dbContext.SaveChangesAsync();
            response.SetSuccess(MapAccount(account));
            return response;
        }

        public async Task<GatewayResponseWrapper<List<ConnectedAccountResponseDto>>> GetAccountsAsync()
        {
            var response = new GatewayResponseWrapper<List<ConnectedAccountResponseDto>>();
            var tenantId = CurrentTenantContext.TenantId;
            var accounts = await _dbContext.ConnectedAccounts.Where(a => a.TenantId == tenantId).OrderByDescending(a => a.CreatedAt).ToListAsync();
            response.SetSuccess(accounts.Select(MapAccount).ToList());
            return response;
        }

        public async Task<GatewayResponseWrapper<ConnectedAccountResponseDto>> GetAccountAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<ConnectedAccountResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var account = await _dbContext.ConnectedAccounts.FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == id);
            if (account == null) { response.SetError("Connected account not found.", 404); return response; }
            response.SetSuccess(MapAccount(account));
            return response;
        }

        public async Task<GatewayResponseWrapper<string>> GetOnboardingLinkAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<string>();
            response.SetSuccess("https://connect.stripe.com/onboarding/placeholder");
            return response;
        }

        public async Task<GatewayResponseWrapper<string>> GetDashboardLinkAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<string>();
            response.SetSuccess("https://connect.stripe.com/dashboard/placeholder");
            return response;
        }

        public async Task<GatewayResponseWrapper<TransferResponseDto>> CreateTransferAsync(CreateTransferDto request)
        {
            var response = new GatewayResponseWrapper<TransferResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var transfer = new TransferRecord
            {
                TenantId = tenantId,
                ConnectedAccountId = request.ConnectedAccountId,
                Amount = request.Amount,
                Currency = request.Currency,
                Description = request.Description,
                SourcePaymentId = request.SourcePaymentId,
                Status = "pending"
            };

            _dbContext.TransferRecords.Add(transfer);
            await _dbContext.SaveChangesAsync();

            response.SetSuccess(new TransferResponseDto
            {
                Id = transfer.Id, ConnectedAccountId = transfer.ConnectedAccountId,
                StripeTransferId = transfer.StripeTransferId, Amount = transfer.Amount,
                Currency = transfer.Currency, Description = transfer.Description,
                Status = transfer.Status, CreatedAt = transfer.CreatedAt
            });
            return response;
        }

        public async Task<GatewayResponseWrapper<List<TransferResponseDto>>> GetTransfersAsync()
        {
            var response = new GatewayResponseWrapper<List<TransferResponseDto>>();
            var tenantId = CurrentTenantContext.TenantId;
            var transfers = await _dbContext.TransferRecords.Where(t => t.TenantId == tenantId).OrderByDescending(t => t.CreatedAt).ToListAsync();
            response.SetSuccess(transfers.Select(t => new TransferResponseDto
            {
                Id = t.Id, ConnectedAccountId = t.ConnectedAccountId, StripeTransferId = t.StripeTransferId,
                Amount = t.Amount, Currency = t.Currency, Description = t.Description,
                Status = t.Status, CreatedAt = t.CreatedAt
            }).ToList());
            return response;
        }

        public async Task<GatewayResponseWrapper<PlatformBalanceDto>> GetBalanceAsync()
        {
            var response = new GatewayResponseWrapper<PlatformBalanceDto>();
            response.SetSuccess(new PlatformBalanceDto { Available = 0, Pending = 0 });
            return response;
        }

        private ConnectedAccountResponseDto MapAccount(ConnectedAccount a) => new()
        {
            Id = a.Id, StripeAccountId = a.StripeAccountId, BusinessName = a.BusinessName,
            Email = a.Email, Country = a.Country, Type = a.Type,
            ChargesEnabled = a.ChargesEnabled, PayoutsEnabled = a.PayoutsEnabled,
            OnboardingComplete = a.OnboardingComplete, PlatformFeePercent = a.PlatformFeePercent,
            PlatformFeeFixed = a.PlatformFeeFixed, CreatedAt = a.CreatedAt
        };
    }
}
