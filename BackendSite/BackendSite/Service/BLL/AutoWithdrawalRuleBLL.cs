using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class AutoWithdrawalRuleBLL : IAutoWithdrawalRuleBLL
    {
        private readonly IAutoWithdrawalRuleService _autoWithdrawalRuleService;
        private readonly VipLevelBLL _vipLevelBll;
        private readonly ITagBLL _tagBLL;

        public AutoWithdrawalRuleBLL(IAutoWithdrawalRuleService autoWithdrawalRuleService, VipLevelBLL vipLevelBll, CustInfoBLL custInfoBll, ITagBLL tagBll)
        {
            _autoWithdrawalRuleService = autoWithdrawalRuleService;
            _vipLevelBll = vipLevelBll;
            _tagBLL = tagBll;
        }

        public async Task<IEnumerable<AutoWithdrawalRuleDataListResponse>> GetDataListAsync(AutoWithdrawalRuleDataListRequest request)
        {
            var data = await _autoWithdrawalRuleService.GetDataListAsync(request);
            var defaultList = new List<int>();

            return data.Select(item => new AutoWithdrawalRuleDataListResponse
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

                WithdrawalTypes = string.IsNullOrEmpty(item.WithdrawalTypeStr)
                    ? defaultList
                    : JsonConvert.DeserializeObject<List<int>>(item.WithdrawalTypeStr),

                CreateUser = item.CreateUser,
                CreateTime = item.CreateTime,
                LastUpdateUser = item.LastUpdateUser,
                LastUpdateTime = item.LastUpdateTime
            }).ToList();
        }

        public async Task CreateAutoWithdrawalRuleAsync(AutoWithdrawalRuleCreateRequest request, string userName)
        {
            await _autoWithdrawalRuleService.CreateAutoWithdrawalRuleAsync(request, userName);
            await CreateActivityLog(request, userName);
        }

        private async Task CreateActivityLog(AutoWithdrawalRuleCreateRequest request, string userName)
        {
            var beforeLog = GetActivityLogObj(
                siteId: request.SiteId,
                currencyId: request.CurrencyId,
                withdrawalAmount: request.WithdrawalAmount,
                accountWithdrawalCount: request.AccountWithdrawalCount,
                vipLevels: request.VipLevels,
                tags: request.Tags,
                withdrawalTypes: request.WithdrawalTypes,
                status: true);

            await _autoWithdrawalRuleService.InsertLogAsync(siteId: request.SiteId,
                custId: 0,
                currencyId: (int)request.CurrencyId,
                beforeData: beforeLog,
                afterData: null,
                adminAdjustmentLogMenu: AdminAdjustmentLogMenuEnum.AutoWithdrawalRule,
                actionType: ActionType.Create,
                userName);
        }

        public async Task SetAutoWithdrawalRuleAsync(AutoWithdrawalRuleSetRequest request, string userName)
        {
            var originData = await _autoWithdrawalRuleService.GetDataByIdAsync(request.Id);
            await _autoWithdrawalRuleService.SetAutoWithdrawalRuleAsync(request, userName);
            await SetActivityLog(request, userName, originData);
        }

        private async Task SetActivityLog(AutoWithdrawalRuleSetRequest request, string userName,
            AutoWithdrawalRuleDataListDto originData)
        {
            var defaultList = new List<int>();
            var vipLevels = string.IsNullOrEmpty(originData.VipLevelStr)
                ? defaultList
                : JsonConvert.DeserializeObject<List<int>>(originData.VipLevelStr);

            var tags = string.IsNullOrEmpty(originData.TagStr)
                ? defaultList
                : JsonConvert.DeserializeObject<List<int>>(originData.TagStr);

            var withdrawalTypes = string.IsNullOrEmpty(originData.WithdrawalTypeStr)
                ? defaultList
                : JsonConvert.DeserializeObject<List<int>>(originData.WithdrawalTypeStr);

            var beforeLog = GetActivityLogObj(
                siteId: originData.SiteId,
                currencyId: originData.CurrencyId,
                withdrawalAmount: originData.WithdrawalAmount,
                accountWithdrawalCount: originData.AccountWithdrawalCount,
                vipLevels: vipLevels,
                tags: tags,
                withdrawalTypes: withdrawalTypes,
                status: GetStatus(originData.Status));

            var afterLog = GetActivityLogObj(
                siteId: originData.SiteId,
                currencyId: originData.CurrencyId,
                withdrawalAmount: request.WithdrawalAmount,
                accountWithdrawalCount: request.AccountWithdrawalCount,
                vipLevels: request.VipLevels,
                tags: request.Tags,
                withdrawalTypes: request.WithdrawalTypes,
                status: GetStatus(request.Status));

            await _autoWithdrawalRuleService.InsertLogAsync(siteId: originData.SiteId,
                custId: 0,
                currencyId: (int)originData.CurrencyId,
                beforeData: beforeLog,
                afterData: afterLog,
                adminAdjustmentLogMenu: AdminAdjustmentLogMenuEnum.AutoWithdrawalRule,
                actionType: ActionType.Update,
                userName);
        }

        public async Task DeleteAutoWithdrawalRuleAsync(AutoWithdrawalRuleDeleteRequest request)
        {
            // todo log before:value after N/A
            await _autoWithdrawalRuleService.DeleteAutoWithdrawalRuleAsync(request);
        }

        #region private method

        private object GetActivityLogObj(int siteId, CurrencyEnum currencyId, decimal withdrawalAmount,
            int accountWithdrawalCount,
            List<int> vipLevels,
            List<int> tags,
            List<int> withdrawalTypes,
            bool status)
        {
            return new
            {
                Currency = currencyId.ToString(),
                WithdrawalAmount = withdrawalAmount,
                AccountWithdrawalCount = accountWithdrawalCount,
                VIPLevel = GetVipLevelName(vipLevels, siteId),
                Tagging = GetTagNames(tags),
                WithdrawalType = GetWithdrawalTypeName(withdrawalTypes),
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
            if (tags.Any()) return string.Empty;

            var tagData = _tagBLL.GetAllTag().Result.ToList();
            var tagNames = tags.Select(tagId => tagData.Where(r => r.Id == tagId).Select(r => r.TagName).FirstOrDefault()).ToList();
            var result = string.Join(",", tagNames.ToArray());

            return result;
        }

        private string GetWithdrawalTypeName(List<int> withdrawalTypes)
        {
            if (!withdrawalTypes.Any()) return string.Empty;

            var withdrawalTypeData = new List<WithdrawalOptionDto>()
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

            var withdrawalTypeNames = withdrawalTypes.Select(withdrawalTypeValue => withdrawalTypeData.Where(r => r.Value == withdrawalTypeValue).Select(x => x.Name).FirstOrDefault()).ToList();
            var result = string.Join(",", withdrawalTypeNames.ToArray());

            return result;
        }

        private bool GetStatus(int status)
        {
            if (status == 0) return false;
            return true;
        }

        #endregion

    }
}
