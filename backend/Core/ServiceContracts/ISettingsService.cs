using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface ISettingsService
    {
        Task<GatewayResponseWrapper<SettingValueDto>> GetAsync(string key);
        Task<GatewayResponseWrapper<List<SettingValueDto>>> GetAllAsync();
        Task<GatewayResponseWrapper<SettingValueDto>> SetAsync(string key, object value, string description = null);
        Task<GatewayResponseWrapper<bool>> DeleteAsync(string key);
        Task<GatewayResponseWrapper<bool>> UpdateAsync(string key, UpdateSettingDto request);
        Task<GatewayResponseWrapper<BillingSettingsDto>> GetBillingSettingsAsync();
        Task<GatewayResponseWrapper<bool>> UpdateBillingSettingsAsync(UpdateBillingSettingsDto request);
        Task<GatewayResponseWrapper<SecuritySettingsDto>> GetSecuritySettingsAsync();
        Task<GatewayResponseWrapper<bool>> UpdateSecuritySettingsAsync(UpdateSecuritySettingsDto request);
        Task<GatewayResponseWrapper<NotificationSettingsDto>> GetNotificationSettingsAsync();
        Task<GatewayResponseWrapper<bool>> UpdateNotificationSettingsAsync(UpdateNotificationSettingsDto request);
    }
}
