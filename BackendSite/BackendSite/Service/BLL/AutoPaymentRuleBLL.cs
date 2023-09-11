using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class AutoPaymentRuleBLL : IAutoPaymentRuleBLL
    {
        private readonly IAutoPaymentRuleService _autoPaymentRuleService;
        private readonly VipLevelBLL _vipLevelBll;
        private readonly ITagBLL _tagBll;

        public AutoPaymentRuleBLL(IAutoPaymentRuleService autoWithdrawalRuleService, VipLevelBLL vipLevelBll, ITagBLL tagBll)
        {
            _autoPaymentRuleService = autoWithdrawalRuleService;
            _vipLevelBll = vipLevelBll;
            _tagBll = tagBll;
        }

        public async Task<IEnumerable<AutoPaymentRuleDataListResponse>> GetDataListAsync(AutoPaymentRuleDataListRequest request)
        {
            var data = await _autoPaymentRuleService.GetDataListAsync(request);
            var defaultList = new List<int>();

            return data.Select(item => new AutoPaymentRuleDataListResponse
            {
                Id = item.Id,
                CurrencyId = item.CurrencyId,
                WithdrawalAmount = item.WithdrawalAmount,
                AccountWithdrawalCount = item.AccountWithdrawalCount,
                Status = item.Status,

                VipLevels = string.IsNullOrEmpty(item.VipLevelStr)
                    ? defaultList
                    : JsonConvert.DeserializeObject<List<int>>(item.VipLevelStr),

                Tags = string.IsNullOrEmpty(item.TagStr)
                    ? defaultList
                    : JsonConvert.DeserializeObject<List<int>>(item.TagStr),

                DeviceLevel = string.IsNullOrEmpty(item.DeviceLevelStr)
                    ? new List<string>()
                    : JsonConvert.DeserializeObject<List<string>>(item.DeviceLevelStr),

                DeviceScores = item.DeviceScores,
                WithdrawalType = item.WithdrawalType,
                CreateUser = item.CreateUser,
                CreateTime = item.CreateTime,
                LastUpdateUser = item.LastUpdateUser,
                LastUpdateTime = item.LastUpdateTime
            }).ToList();
        }

        #region Create
        
        public async Task CreateAutoPaymentRuleAsync(AutoPaymentRuleCreateRequest request, string userName)
        {
            await _autoPaymentRuleService.CreateAutoPaymentRuleAsync(request, userName);
            await CreateActivityLog(request, userName);
        }

        private async Task CreateActivityLog(AutoPaymentRuleCreateRequest request, string userName)
        {
            var afterLog = GetActivityLogObj(
                autoType: request.AutoType,
                siteId: request.SiteId,
                currencyId: request.CurrencyId,
                withdrawalAmount: request.WithdrawalAmount,
                accountWithdrawalCount: request.AccountWithdrawalCount,
                vipLevels: request.VipLevels,
                tags: request.Tags,
                deviceScores: request.DeviceScores,
                deviceLevel: request.DeviceLevel,
                withdrawalType: request.WithdrawalType,
                status: true);

            await _autoPaymentRuleService.InsertLogAsync(
                siteId: request.SiteId,
                custId: 0,
                currencyId: (int)request.CurrencyId,
                beforeData: null,
                afterData: afterLog,
                adminAdjustmentLogMenu: AdminAdjustmentLogMenuEnum.AutoPaymentRule,
                actionType: ActionType.Create,
                userName);
        }

        #endregion

        #region Set

        public async Task SetAutoPaymentRuleAsync(AutoPaymentRuleSetRequest request, string userName)
        {
            var originData = await _autoPaymentRuleService.GetDataByIdAsync(request.Id);
            await _autoPaymentRuleService.SetAutoPaymentRuleAsync(request, userName);
            await SetActivityLog(request, userName, originData);
        }

        private async Task SetActivityLog(AutoPaymentRuleSetRequest request, string userName,
            AutoPaymentRuleDataListDto originData)
        {
            var beforeLog = GetBeforeChangeLog(originData);

            var afterLog = GetActivityLogObj(
                autoType: originData.AutoType,
                siteId: originData.SiteId,
                currencyId: originData.CurrencyId,
                withdrawalAmount: request.WithdrawalAmount,
                accountWithdrawalCount: request.AccountWithdrawalCount,
                vipLevels: request.VipLevels,
                tags: request.Tags,
                deviceScores : request.DeviceScores,
                deviceLevel:request.DeviceLevel,
                withdrawalType: request.WithdrawalType,
                status: GetStatus(request.Status));

            await _autoPaymentRuleService.InsertLogAsync(siteId: originData.SiteId,
                custId: 0,
                currencyId: (int)originData.CurrencyId,
                beforeData: beforeLog,
                afterData: afterLog,
                adminAdjustmentLogMenu: AdminAdjustmentLogMenuEnum.AutoPaymentRule,
                actionType: ActionType.Update,
                userName);
        }

        #endregion

        #region Delete
        
        public async Task DeleteAutoPaymentRuleAsync(AutoPaymentRuleDeleteRequest request, string userName)
        {
            var originData = await _autoPaymentRuleService.GetDataByIdAsync(request.Id);
            await _autoPaymentRuleService.DeleteAutoPaymentRuleAsync(request);
            var beforeLog = GetBeforeChangeLog(originData);

            await _autoPaymentRuleService.InsertLogAsync(siteId: originData.SiteId,
                custId: 0,
                currencyId: (int)originData.CurrencyId,
                beforeData: beforeLog,
                afterData: null,
                adminAdjustmentLogMenu: AdminAdjustmentLogMenuEnum.AutoPaymentRule,
                actionType: ActionType.Delete,
                userName);
        }

        #endregion

        #region private common method

        private object GetBeforeChangeLog(AutoPaymentRuleDataListDto originData)
        {
            var defaultList = new List<int>();
            var vipLevels = string.IsNullOrEmpty(originData.VipLevelStr)
                ? defaultList
                : JsonConvert.DeserializeObject<List<int>>(originData.VipLevelStr);

            var tags = string.IsNullOrEmpty(originData.TagStr)
                ? defaultList
                : JsonConvert.DeserializeObject<List<int>>(originData.TagStr);

            var deviceLevel = string.IsNullOrEmpty(originData.DeviceLevelStr)
                ? new List<string>()
                : JsonConvert.DeserializeObject<List<string>>(originData.DeviceLevelStr);

            var beforeLog = GetActivityLogObj(
                autoType: originData.AutoType,
                siteId: originData.SiteId,
                currencyId: originData.CurrencyId,
                withdrawalAmount: originData.WithdrawalAmount,
                accountWithdrawalCount: originData.AccountWithdrawalCount,
                vipLevels: vipLevels,
                tags: tags,
                deviceScores: originData.DeviceScores,
                deviceLevel: deviceLevel,
                withdrawalType: originData.WithdrawalType,
                status: GetStatus(originData.Status));

            return beforeLog;
        }

        private object GetActivityLogObj(AutoType autoType, int siteId, CurrencyEnum currencyId, decimal withdrawalAmount,
            int accountWithdrawalCount,
            List<int> vipLevels,
            List<int> tags,
            int deviceScores,
            List<string> deviceLevel,
            WithdrawalMethodEnum withdrawalType,
            bool status)
        {
            return new
            {
                AutoType = autoType.ToString(),
                Currency = currencyId.ToString(),
                WithdrawalAmount = withdrawalAmount,
                AccountWithdrawalCount = accountWithdrawalCount,
                VIPLevel = GetVipLevelName(vipLevels, siteId),
                Tagging = GetTagNames(tags),
                DeviceScores = deviceScores,
                DeviceLevel = deviceLevel.Any() ? string.Join(",", deviceLevel) : null ,
                WithdrawalType = withdrawalType.ToString(),
                Status = status
            };
        }

        private string GetVipLevelName(List<int> vips, int siteId)
        {
            if (!vips.Any()) return string.Empty;

            var vipData = _vipLevelBll.GetVipLevelSetting(siteId).ToList();
            var vipLevelNames = vips.Select(vipLevel => vipData.Where(r => r.Level == vipLevel).Select(x => x.LevelName).FirstOrDefault() ?? string.Empty).ToList();
            var result = string.Join(",", vipLevelNames.ToArray());

            return result;
        }

        private string GetTagNames(List<int> tags)
        {
            if (!tags.Any()) return string.Empty;

            var tagData = _tagBll.GetAllTag().Result.ToList();
            var tagNames = tags.Select(tagId => tagData.Where(r => r.Id == tagId).Select(r => r.TagName).FirstOrDefault()).ToList();
            var result = string.Join(",", tagNames.ToArray());

            return result;
        }

        private bool GetStatus(int status)
        {
            if (status == 0) return false;
            return true;
        }
        
        public List<WithdrawalOptionDto> GetWithdrawalOption()
        {
            return new List<WithdrawalOptionDto>()
            {
                new()
                {
                    Name = WithdrawalMethodEnum.EWallet.ToString(),
                    Value = (int)WithdrawalMethodEnum.EWallet
                },
                new()
                {
                    Name = WithdrawalMethodEnum.BankCard.ToString(),
                    Value = (int)WithdrawalMethodEnum.BankCard
                },
                new()
                {
                    Name = WithdrawalMethodEnum.Crypto.ToString(),
                    Value = (int)WithdrawalMethodEnum.Crypto
                }
            };
        }

        #endregion

    }
}