using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL.Interface
{
    public interface IVipLevelBLL
    {
        Task<MemberVipLevelInfo> GetVipLevelInfoAsync(int custId);
        Task<IEnumerable<MemberVipLevelLog>> GetVipLevelLogAsync(int custId);
        Task<IEnumerable<VipLevelSetting>> GetVipLevelSettingAsync(int siteId);
        Task<int> SetVipLevelAsync(int siteId, int custId, string userName, int vipLevel, string updator);
        Task<int> SetVipLevelManuallyAsync(int siteId, int custId, string userName, int manually, string updator);
        Task<int> SetVipLevelDefaultAsync(int siteId, int custId, string userName, int oldLevel, DefaultVIPLevel defaultLevel, string updator);
        Task<int> VipAutomationExclusion(int custId, bool enable, string updator);
        Task<IEnumerable<VipUpgradeSetting>> GetUpgradeSetting(int siteId, int CurrencyId);
        Task<int> SetUpgradeSetting(ParamsWithLog<VipUpgradeSetting> input);
        Task<IEnumerable<int>> GetVipUpgradeSupportCurrency(int siteId);
    }
}