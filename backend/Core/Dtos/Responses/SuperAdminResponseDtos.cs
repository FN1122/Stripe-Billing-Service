namespace Core.Dtos.Responses
{
    public class ImpersonationResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public int ExpiresInMinutes { get; set; }
    }

    public class SystemDashboardDto
    {
        public int TotalTenants { get; set; }
        public int ActiveTenants { get; set; }
        public int TotalCustomers { get; set; }
        public int ActiveSubscriptions { get; set; }
        public decimal TotalRevenue { get; set; }
        public int FailedPaymentsLast30Days { get; set; }
        public List<TenantSummaryDto> RecentTenants { get; set; } = new();
    }

    public class TenantSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TenantRevenueBreakdownDto
    {
        public Guid TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int TotalCustomers { get; set; }
    }

    public class PlatformSettingsDto
    {
        public string? PlatformName { get; set; }
        public string? DefaultCurrency { get; set; }
        public bool MaintenanceMode { get; set; }
        public string? DefaultFeatures { get; set; }
        public int MaxTenantsAllowed { get; set; }
    }
}
