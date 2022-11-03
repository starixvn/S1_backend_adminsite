using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface
{
    public interface IVipLevelBLL
    {
        Task<MemberVipLevelInfo> GetVipLevelInfoAsync(int custId);
        Task<IEnumerable<MemberVipLevelLog>> GetVipLevelLogAsync(int custId);
        Task<IEnumerable<VipLevelSetting>> GetVipLevelSettingAsync(int siteId);
        Task<int> SetVipLevelAsync(int siteId, int custId, string userName, int vipLevel, string updator);
        Task<int> SetVipLevelManuallyAsync(int siteId, int custId, string userName, int manually, string updator);
    }
}