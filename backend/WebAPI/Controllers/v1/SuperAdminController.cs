using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.ServiceContracts;
using Core.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/super-admin")]
    [Authorize(Policy = "SuperAdminOnly")]
    public class SuperAdminController : GatewayControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly IRevenueAnalyticsService _analyticsService;
        private readonly IAuditService _auditService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUserRepository _userRepo;
        private readonly ITenantRepository _tenantRepo;
        private readonly BillingDbContext _dbContext;

        public SuperAdminController(
            ITenantService tenantService,
            IRevenueAnalyticsService analyticsService,
            IAuditService auditService,
            IJwtTokenService jwtTokenService,
            IUserRepository userRepo,
            ITenantRepository tenantRepo,
            BillingDbContext dbContext)
        {
            _tenantService = tenantService;
            _analyticsService = analyticsService;
            _auditService = auditService;
            _jwtTokenService = jwtTokenService;
            _userRepo = userRepo;
            _tenantRepo = tenantRepo;
            _dbContext = dbContext;
        }

        // ────── Tenant CRUD ──────

        [HttpGet("tenants")]
        public async Task<IActionResult> ListTenants([FromQuery] TenantFilterDto filter)
        {
            return ToResponse(await _tenantService.ListAsync(filter));
        }

        [HttpPost("tenants")]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantDto request)
        {
            return ToResponse(await _tenantService.CreateAsync(request));
        }

        [HttpGet("tenants/{id}")]
        public async Task<IActionResult> GetTenant(Guid id)
        {
            return ToResponse(await _tenantService.GetAsync(id));
        }

        [HttpPut("tenants/{id}")]
        public async Task<IActionResult> UpdateTenant(Guid id, [FromBody] UpdateTenantDto request)
        {
            return ToResponse(await _tenantService.UpdateAsync(id, request));
        }

        [HttpPost("tenants/{id}/suspend")]
        public async Task<IActionResult> SuspendTenant(Guid id, [FromQuery] string reason = "Suspended by admin")
        {
            return ToResponse(await _tenantService.SuspendAsync(id, reason));
        }

        [HttpPost("tenants/{id}/activate")]
        public async Task<IActionResult> ActivateTenant(Guid id)
        {
            return ToResponse(await _tenantService.ActivateAsync(id));
        }

        [HttpPost("tenants/{id}/rotate-keys")]
        public async Task<IActionResult> RotateKeys(Guid id)
        {
            return ToResponse(await _tenantService.RotateKeysAsync(id));
        }

        [HttpGet("tenants/{id}/health")]
        public async Task<IActionResult> GetTenantHealth(Guid id)
        {
            return ToResponse(await _tenantService.GetHealthCheckAsync(id));
        }

        [HttpGet("tenants/{id}/verify-stripe")]
        public async Task<IActionResult> VerifyStripe(Guid id)
        {
            return ToResponse(await _tenantService.VerifyStripeConfigurationAsync(id));
        }

        // ────── Create First Admin for Tenant ──────

        [HttpPost("tenants/{tenantId}/admin-user")]
        public async Task<IActionResult> CreateTenantAdmin(Guid tenantId, [FromBody] CreateTenantAdminDto request)
        {
            var response = new GatewayResponseWrapper<UserResponseDto>();

            var tenant = await _tenantRepo.GetByIdAsync(tenantId);
            if (tenant == null) { response.SetError("Tenant not found.", 404); return ToResponse(response); }

            // Check if an admin already exists for this tenant
            var existingAdmin = await _userRepo.Query(tenantId).FirstOrDefaultAsync(u => u.Role == "Admin");
            if (existingAdmin != null)
            {
                response.SetError("This tenant already has an admin user. Tenant admins manage their own users.", 400);
                return ToResponse(response);
            }

            var existingEmail = await _userRepo.GetByEmailGlobalAsync(request.Email);
            if (existingEmail != null) { response.SetError("A user with this email already exists.", 400); return ToResponse(response); }

            var user = new User
            {
                TenantId = tenantId,
                Email = request.Email,
                FirstName = request.FirstName ?? "",
                LastName = request.LastName ?? "",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12),
                Role = "Admin",
                Permissions = "[\"*\"]",
                IsActive = true
            };

            await _userRepo.CreateAsync(user);

            response.SetSuccess(new UserResponseDto
            {
                Id = user.Id, Email = user.Email, FirstName = user.FirstName,
                LastName = user.LastName, FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Role = user.Role, IsActive = user.IsActive, CreatedAt = user.CreatedAt
            });
            return ToResponse(response);
        }

        // ────── Impersonation ──────

        [HttpPost("tenants/{tenantId}/impersonate")]
        public async Task<IActionResult> ImpersonateTenant(Guid tenantId)
        {
            var response = new GatewayResponseWrapper<ImpersonationResponseDto>();

            var tenant = await _tenantRepo.GetByIdAsync(tenantId);
            if (tenant == null) { response.SetError("Tenant not found.", 404); return ToResponse(response); }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) { response.SetError("Invalid user context.", 401); return ToResponse(response); }

            var superAdminUser = await _userRepo.GetByIdAsync(Guid.Parse(userIdClaim));
            if (superAdminUser == null) { response.SetError("SuperAdmin user not found.", 404); return ToResponse(response); }

            var impersonationToken = _jwtTokenService.GenerateImpersonationToken(superAdminUser, tenantId, tenant.Name);

            response.SetSuccess(new ImpersonationResponseDto
            {
                AccessToken = impersonationToken,
                TenantId = tenantId.ToString(),
                TenantName = tenant.Name,
                ExpiresInMinutes = 30
            });
            return ToResponse(response);
        }

        // ────── System Dashboard ──────

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetSystemDashboard()
        {
            var response = new GatewayResponseWrapper<SystemDashboardDto>();

            var totalTenants = await _dbContext.Tenants.CountAsync();
            var activeTenants = await _dbContext.Tenants.CountAsync(t => t.IsActive);
            var totalCustomers = await _dbContext.Customers.CountAsync();
            var activeSubscriptions = await _dbContext.Subscriptions.CountAsync(s => s.Status == "active" || s.Status == "trialing");
            var totalRevenue = await _dbContext.PaymentTransactions.Where(p => p.Status == "succeeded").SumAsync(p => p.Amount);
            var failedPayments30d = await _dbContext.PaymentTransactions
                .CountAsync(p => p.Status == "failed" && p.CreatedAt >= DateTime.UtcNow.AddDays(-30));

            var recentTenants = await _dbContext.Tenants
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .Select(t => new TenantSummaryDto { Id = t.Id, Name = t.Name, IsActive = t.IsActive, CreatedAt = t.CreatedAt })
                .ToListAsync();

            response.SetSuccess(new SystemDashboardDto
            {
                TotalTenants = totalTenants,
                ActiveTenants = activeTenants,
                TotalCustomers = totalCustomers,
                ActiveSubscriptions = activeSubscriptions,
                TotalRevenue = Math.Round(totalRevenue, 2),
                FailedPaymentsLast30Days = failedPayments30d,
                RecentTenants = recentTenants
            });
            return ToResponse(response);
        }

        // ────── Global Analytics ──────

        [HttpGet("analytics/mrr")]
        public async Task<IActionResult> GetMrr()
        {
            return ToResponse(await _analyticsService.GetMrrAsync());
        }

        [HttpGet("analytics/tenant-breakdown")]
        public async Task<IActionResult> GetTenantBreakdown()
        {
            var response = new GatewayResponseWrapper<List<TenantRevenueBreakdownDto>>();

            var breakdown = await _dbContext.Tenants
                .Select(t => new TenantRevenueBreakdownDto
                {
                    TenantId = t.Id,
                    TenantName = t.Name,
                    TotalRevenue = t.PaymentTransactions.Where(p => p.Status == "succeeded").Sum(p => p.Amount),
                    ActiveSubscriptions = t.Subscriptions.Count(s => s.Status == "active" || s.Status == "trialing"),
                    TotalCustomers = t.Customers.Count
                })
                .OrderByDescending(t => t.TotalRevenue)
                .ToListAsync();

            response.SetSuccess(breakdown);
            return ToResponse(response);
        }

        // ────── Global Audit Log ──────

        [HttpGet("audit-log")]
        public async Task<IActionResult> GetAuditLog([FromQuery] AuditLogFilterDto filter)
        {
            return ToResponse(await _auditService.ListAsync(filter));
        }

        // ────── Global Email Templates ──────

        [HttpGet("email-templates")]
        public async Task<IActionResult> GetGlobalEmailTemplates()
        {
            var response = new GatewayResponseWrapper<List<EmailTemplateResponseDto>>();
            var templates = await _dbContext.EmailTemplates
                .Where(t => t.TenantId == Guid.Empty)
                .OrderBy(t => t.TemplateKey)
                .ToListAsync();
            var dtos = templates.Select(t => new EmailTemplateResponseDto
            {
                Id = t.Id,
                TemplateKey = t.TemplateKey,
                Subject = t.Subject,
                HtmlBody = t.HtmlBody,
                Variables = ParseVariables(t.Variables),
                CreatedAt = t.CreatedAt
            }).ToList();
            response.SetSuccess(dtos);
            return ToResponse(response);
        }

        [HttpPost("email-templates")]
        public async Task<IActionResult> CreateGlobalEmailTemplate([FromBody] CreateEmailTemplateDto request)
        {
            var response = new GatewayResponseWrapper<EmailTemplateResponseDto>();
            var template = new EmailTemplate
            {
                TenantId = Guid.Empty,
                TemplateKey = request.TemplateKey,
                Subject = request.Subject,
                HtmlBody = request.HtmlBody,
                Variables = System.Text.Json.JsonSerializer.Serialize(request.Variables)
            };
            _dbContext.EmailTemplates.Add(template);
            await _dbContext.SaveChangesAsync();
            response.SetSuccess(new EmailTemplateResponseDto
            {
                Id = template.Id, TemplateKey = template.TemplateKey, Subject = template.Subject,
                HtmlBody = template.HtmlBody, Variables = ParseVariables(template.Variables), CreatedAt = template.CreatedAt
            });
            return ToResponse(response);
        }

        [HttpPut("email-templates/{id}")]
        public async Task<IActionResult> UpdateGlobalEmailTemplate(Guid id, [FromBody] CreateEmailTemplateDto request)
        {
            var response = new GatewayResponseWrapper<EmailTemplateResponseDto>();
            var template = await _dbContext.EmailTemplates.FirstOrDefaultAsync(t => t.Id == id && t.TenantId == Guid.Empty);
            if (template == null) { response.SetError("Template not found.", 404); return ToResponse(response); }

            template.TemplateKey = request.TemplateKey;
            template.Subject = request.Subject;
            template.HtmlBody = request.HtmlBody;
            template.Variables = System.Text.Json.JsonSerializer.Serialize(request.Variables);
            template.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            response.SetSuccess(new EmailTemplateResponseDto
            {
                Id = template.Id, TemplateKey = template.TemplateKey, Subject = template.Subject,
                HtmlBody = template.HtmlBody, Variables = ParseVariables(template.Variables), CreatedAt = template.CreatedAt
            });
            return ToResponse(response);
        }

        [HttpDelete("email-templates/{id}")]
        public async Task<IActionResult> DeleteGlobalEmailTemplate(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var template = await _dbContext.EmailTemplates.FirstOrDefaultAsync(t => t.Id == id && t.TenantId == Guid.Empty);
            if (template == null) { response.SetError("Template not found.", 404); return ToResponse(response); }
            _dbContext.EmailTemplates.Remove(template);
            await _dbContext.SaveChangesAsync();
            response.SetSuccess(true);
            return ToResponse(response);
        }

        // ────── Helper ──────

        private static List<string> ParseVariables(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<string>();
            try { return System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>(); }
            catch { return new List<string>(); }
        }

        // ────── Platform Settings ──────

        [HttpGet("settings")]
        public async Task<IActionResult> GetPlatformSettings()
        {
            var response = new GatewayResponseWrapper<PlatformSettingsDto>();
            var settings = await _dbContext.Settings.Where(s => s.TenantId == Guid.Empty).ToListAsync();

            response.SetSuccess(new PlatformSettingsDto
            {
                PlatformName = settings.FirstOrDefault(s => s.Key == "platform_name")?.Value ?? "Stripe Billing Service",
                DefaultCurrency = settings.FirstOrDefault(s => s.Key == "default_currency")?.Value ?? "usd",
                MaintenanceMode = settings.FirstOrDefault(s => s.Key == "maintenance_mode")?.Value == "true",
                DefaultFeatures = settings.FirstOrDefault(s => s.Key == "default_features")?.Value ?? "[]",
                MaxTenantsAllowed = int.Parse(settings.FirstOrDefault(s => s.Key == "max_tenants")?.Value ?? "100"),
            });
            return ToResponse(response);
        }

        [HttpPut("settings")]
        public async Task<IActionResult> UpdatePlatformSettings([FromBody] PlatformSettingsDto request)
        {
            var response = new GatewayResponseWrapper<PlatformSettingsDto>();
            var updates = new Dictionary<string, string>
            {
                ["platform_name"] = request.PlatformName ?? "Stripe Billing Service",
                ["default_currency"] = request.DefaultCurrency ?? "usd",
                ["maintenance_mode"] = request.MaintenanceMode.ToString().ToLower(),
                ["default_features"] = request.DefaultFeatures ?? "[]",
                ["max_tenants"] = request.MaxTenantsAllowed.ToString()
            };

            foreach (var kv in updates)
            {
                var setting = await _dbContext.Settings.FirstOrDefaultAsync(s => s.TenantId == Guid.Empty && s.Key == kv.Key);
                if (setting != null) { setting.Value = kv.Value; setting.UpdatedAt = DateTime.UtcNow; }
                else { _dbContext.Settings.Add(new Setting { TenantId = Guid.Empty, Key = kv.Key, Value = kv.Value, ValueType = "string" }); }
            }
            await _dbContext.SaveChangesAsync();
            response.SetSuccess(request);
            return ToResponse(response);
        }
    }
}
