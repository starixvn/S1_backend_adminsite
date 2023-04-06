using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class AutoWithdrawalRuleBLL : IAutoWithdrawalRuleBLL
    {
        private readonly IAutoWithdrawalRuleService _autoWithdrawalRuleService;

        public AutoWithdrawalRuleBLL(IAutoWithdrawalRuleService autoWithdrawalRuleService)
        {
            _autoWithdrawalRuleService = autoWithdrawalRuleService;
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
        }

        public async Task SetAutoWithdrawalRuleAsync(AutoWithdrawalRuleSetRequest request, string userName)
        {
            await _autoWithdrawalRuleService.SetAutoWithdrawalRuleAsync(request, userName);
        }

        public async Task DeleteAutoWithdrawalRuleAsync(AutoWithdrawalRuleDeleteRequest request)
        {
            await _autoWithdrawalRuleService.DeleteAutoWithdrawalRuleAsync(request);
        }
    }
}
