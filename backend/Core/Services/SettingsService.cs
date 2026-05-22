using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.ServiceContracts;
using Core.Utils;
using Newtonsoft.Json;

namespace Core.Services
{
    public class SettingsService : BaseService, ISettingsService
    {
        private readonly ISettingRepository _settingRepo;
        private readonly ITenantRepository _tenantRepo;

        public SettingsService(ITenantContextProvider tcp, ISettingRepository settingRepo, ITenantRepository tenantRepo) : base(tcp)
        {
            _settingRepo = settingRepo;
            _tenantRepo = tenantRepo;
        }

        public async Task<GatewayResponseWrapper<SettingValueDto>> GetAsync(string key)
        {
            var response = new GatewayResponseWrapper<SettingValueDto>();
            var setting = await _settingRepo.GetByKeyAsync(CurrentTenantContext.TenantId, key);
            if (setting == null) { response.SetError("Setting not found."); return response; }
            response.SetSuccess(MapSetting(setting));
            return response;
        }

        public async Task<GatewayResponseWrapper<List<SettingValueDto>>> GetAllAsync()
        {
            var response = new GatewayResponseWrapper<List<SettingValueDto>>();
            var settings = await _settingRepo.GetByTenantIdAsync(CurrentTenantContext.TenantId);
            response.SetSuccess(settings.Select(MapSetting).ToList());
            return response;
        }

        public async Task<GatewayResponseWrapper<SettingValueDto>> SetAsync(string key, object value, string description = null)
        {
            var response = new GatewayResponseWrapper<SettingValueDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var setting = await _settingRepo.GetByKeyAsync(tenantId, key);
            if (setting == null)
            {
                setting = new Setting { TenantId = tenantId, Key = key, Value = JsonConvert.SerializeObject(value), Description = description, ValueType = GetValueType(value) };
                await _settingRepo.CreateAsync(setting);
            }
            else
            {
                setting.Value = JsonConvert.SerializeObject(value);
                if (description != null) setting.Description = description;
                setting.UpdatedAt = DateTime.UtcNow;
                await _settingRepo.UpdateAsync(setting);
            }
            response.SetSuccess(MapSetting(setting));
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> DeleteAsync(string key)
        {
            var response = new GatewayResponseWrapper<bool>();
            var setting = await _settingRepo.GetByKeyAsync(CurrentTenantContext.TenantId, key);
            if (setting == null) { response.SetError("Setting not found."); return response; }
            await _settingRepo.DeleteAsync(setting);
            response.SetSuccess(true, "Setting deleted.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> UpdateAsync(string key, UpdateSettingDto request)
        {
            var response = new GatewayResponseWrapper<bool>();
            var setting = await _settingRepo.GetByKeyAsync(CurrentTenantContext.TenantId, key);
            if (setting == null) { response.SetError("Setting not found."); return response; }
            if (request.Value != null) { setting.Value = JsonConvert.SerializeObject(request.Value); setting.ValueType = GetValueType(request.Value); }
            if (request.Description != null) setting.Description = request.Description;
            setting.UpdatedAt = DateTime.UtcNow;
            await _settingRepo.UpdateAsync(setting);
            response.SetSuccess(true, "Setting updated.");
            return response;
        }

        public async Task<GatewayResponseWrapper<BillingSettingsDto>> GetBillingSettingsAsync()
        {
            var response = new GatewayResponseWrapper<BillingSettingsDto>();
            var tenant = await _tenantRepo.GetByIdAsync(CurrentTenantContext.TenantId);
            if (tenant == null) { response.SetError("Tenant not found."); return response; }
            var settings = await _settingRepo.GetByTenantIdAsync(CurrentTenantContext.TenantId);
            response.SetSuccess(new BillingSettingsDto
            {
                DefaultCurrency = GetSettingValue(settings, "billing.default_currency", "usd"),
                TaxCalculationMethod = GetSettingValue(settings, "billing.tax_calculation", "automatic"),
                InvoicePrefix = GetSettingValue(settings, "billing.invoice_prefix", "INV"),
                EnableAutoRetry = GetSettingValue<bool>(settings, "billing.enable_auto_retry", true),
                MaxRetryAttempts = GetSettingValue<int>(settings, "billing.max_retry_attempts", 3),
                GracePeriodDays = GetSettingValue<int>(settings, "billing.grace_period_days", 3),
                SendInvoiceEmails = GetSettingValue<bool>(settings, "billing.send_invoice_emails", true),
                EnableProrations = GetSettingValue<bool>(settings, "billing.enable_prorations", true),
                TrialPeriodDays = GetSettingValue<int>(settings, "billing.trial_period_days", 14),
                VatId = GetSettingValue(settings, "billing.vat_id", null),
                CompanyName = GetSettingValue(settings, "billing.company_name", tenant.Name),
                CompanyAddress = GetSettingValue(settings, "billing.company_address", null)
            });
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> UpdateBillingSettingsAsync(UpdateBillingSettingsDto request)
        {
            var response = new GatewayResponseWrapper<bool>();
            if (request.DefaultCurrency != null) await SetAsync("billing.default_currency", request.DefaultCurrency);
            if (request.TaxCalculationMethod != null) await SetAsync("billing.tax_calculation", request.TaxCalculationMethod);
            if (request.InvoicePrefix != null) await SetAsync("billing.invoice_prefix", request.InvoicePrefix);
            if (request.EnableAutoRetry.HasValue) await SetAsync("billing.enable_auto_retry", request.EnableAutoRetry.Value);
            if (request.MaxRetryAttempts.HasValue) await SetAsync("billing.max_retry_attempts", request.MaxRetryAttempts.Value);
            if (request.GracePeriodDays.HasValue) await SetAsync("billing.grace_period_days", request.GracePeriodDays.Value);
            if (request.SendInvoiceEmails.HasValue) await SetAsync("billing.send_invoice_emails", request.SendInvoiceEmails.Value);
            if (request.EnableProrations.HasValue) await SetAsync("billing.enable_prorations", request.EnableProrations.Value);
            if (request.TrialPeriodDays.HasValue) await SetAsync("billing.trial_period_days", request.TrialPeriodDays.Value);
            if (request.VatId != null) await SetAsync("billing.vat_id", request.VatId);
            if (request.CompanyName != null) await SetAsync("billing.company_name", request.CompanyName);
            if (request.CompanyAddress != null) await SetAsync("billing.company_address", request.CompanyAddress);
            response.SetSuccess(true, "Billing settings updated.");
            return response;
        }

        public async Task<GatewayResponseWrapper<SecuritySettingsDto>> GetSecuritySettingsAsync()
        {
            var response = new GatewayResponseWrapper<SecuritySettingsDto>();
            var settings = await _settingRepo.GetByTenantIdAsync(CurrentTenantContext.TenantId);
            response.SetSuccess(new SecuritySettingsDto
            {
                RequireMfa = GetSettingValue<bool>(settings, "security.require_mfa", false),
                ApiKeyExpiration = GetSettingValue<int>(settings, "security.api_key_expiration_days", 365),
                SessionTimeout = GetSettingValue<int>(settings, "security.session_timeout_minutes", 60),
                IpWhitelistEnabled = GetSettingValue<bool>(settings, "security.ip_whitelist_enabled", false),
                IpWhitelist = GetSettingValue(settings, "security.ip_whitelist", ""),
                RequireHttps = GetSettingValue<bool>(settings, "security.require_https", true),
                AllowPublicApiKeys = GetSettingValue<bool>(settings, "security.allow_public_api_keys", false)
            });
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> UpdateSecuritySettingsAsync(UpdateSecuritySettingsDto request)
        {
            var response = new GatewayResponseWrapper<bool>();
            if (request.RequireMfa.HasValue) await SetAsync("security.require_mfa", request.RequireMfa.Value);
            if (request.ApiKeyExpiration.HasValue) await SetAsync("security.api_key_expiration_days", request.ApiKeyExpiration.Value);
            if (request.SessionTimeout.HasValue) await SetAsync("security.session_timeout_minutes", request.SessionTimeout.Value);
            if (request.IpWhitelistEnabled.HasValue) await SetAsync("security.ip_whitelist_enabled", request.IpWhitelistEnabled.Value);
            if (request.IpWhitelist != null) await SetAsync("security.ip_whitelist", request.IpWhitelist);
            if (request.RequireHttps.HasValue) await SetAsync("security.require_https", request.RequireHttps.Value);
            if (request.AllowPublicApiKeys.HasValue) await SetAsync("security.allow_public_api_keys", request.AllowPublicApiKeys.Value);
            response.SetSuccess(true, "Security settings updated.");
            return response;
        }

        public async Task<GatewayResponseWrapper<NotificationSettingsDto>> GetNotificationSettingsAsync()
        {
            var response = new GatewayResponseWrapper<NotificationSettingsDto>();
            var settings = await _settingRepo.GetByTenantIdAsync(CurrentTenantContext.TenantId);
            response.SetSuccess(new NotificationSettingsDto
            {
                SendPaymentConfirmations = GetSettingValue<bool>(settings, "notifications.payment_confirmations", true),
                SendSubscriptionNotifications = GetSettingValue<bool>(settings, "notifications.subscription_notifications", true),
                SendRefundNotifications = GetSettingValue<bool>(settings, "notifications.refund_notifications", true),
                SendInvoiceNotifications = GetSettingValue<bool>(settings, "notifications.invoice_notifications", true),
                NotificationEmail = GetSettingValue(settings, "notifications.email", null),
                EnableWebhookNotifications = GetSettingValue<bool>(settings, "notifications.enable_webhooks", true),
                NotificationLanguage = GetSettingValue(settings, "notifications.language", "en")
            });
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> UpdateNotificationSettingsAsync(UpdateNotificationSettingsDto request)
        {
            var response = new GatewayResponseWrapper<bool>();
            if (request.SendPaymentConfirmations.HasValue) await SetAsync("notifications.payment_confirmations", request.SendPaymentConfirmations.Value);
            if (request.SendSubscriptionNotifications.HasValue) await SetAsync("notifications.subscription_notifications", request.SendSubscriptionNotifications.Value);
            if (request.SendRefundNotifications.HasValue) await SetAsync("notifications.refund_notifications", request.SendRefundNotifications.Value);
            if (request.SendInvoiceNotifications.HasValue) await SetAsync("notifications.invoice_notifications", request.SendInvoiceNotifications.Value);
            if (request.NotificationEmail != null) await SetAsync("notifications.email", request.NotificationEmail);
            if (request.EnableWebhookNotifications.HasValue) await SetAsync("notifications.enable_webhooks", request.EnableWebhookNotifications.Value);
            if (request.NotificationLanguage != null) await SetAsync("notifications.language", request.NotificationLanguage);
            response.SetSuccess(true, "Notification settings updated.");
            return response;
        }

        private static SettingValueDto MapSetting(Setting s) => new() { Key = s.Key, Value = !string.IsNullOrEmpty(s.Value) ? JsonConvert.DeserializeObject<dynamic>(s.Value) : null, ValueType = s.ValueType, Description = s.Description, UpdatedAt = s.UpdatedAt };
        private static string GetValueType(object value) { if (value == null) return "null"; if (value is bool) return "boolean"; if (value is int || value is long) return "integer"; if (value is decimal || value is double || value is float) return "number"; if (value is DateTime) return "datetime"; if (value is List<string> or string[]) return "array"; return "string"; }
        private static T GetSettingValue<T>(List<Setting> settings, string key, T defaultValue) { var setting = settings.FirstOrDefault(s => s.Key == key); if (setting == null) return defaultValue; try { return JsonConvert.DeserializeObject<T>(setting.Value); } catch { return defaultValue; } }
        private static string GetSettingValue(List<Setting> settings, string key, string defaultValue) { var setting = settings.FirstOrDefault(s => s.Key == key); if (setting == null) return defaultValue; try { return JsonConvert.DeserializeObject<string>(setting.Value); } catch { return defaultValue; } }
    }
}
