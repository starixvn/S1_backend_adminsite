using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using static BackendSite.Controllers.ApiV2.Member.Setting.VipLevelController;

namespace BackendSite.Service.DAL.Interface
{
    public interface IVipLevelService
    {
        Task<VipLevelInfoDto> GetVipLevelInfoAsync(int custId);
        Task<IEnumerable<VipLevelLogDto>> GetVipLevelLogAsync(int custId);
        Task<IEnumerable<VipLevelSetting>> GetVipLevelSettingAsync(int siteId);
        Task<int> SetVipLevelAsync(int siteId, int custId, string userName, int vipLevel, string memo, string updator);
        Task<int> SetVipLevelManuallyAsync(int siteId, int custId, string userName, int manually, string memo, string updator);
        Task<IEnumerable<VipSystem>> GetVipSystem(int siteId);
        Task<int> SetUpgradeSetting(ParamsWithLog<VipUpgradeSetting> input);
    }
}