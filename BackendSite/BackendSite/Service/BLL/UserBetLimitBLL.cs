using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.ExceptionHandling;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class UserBetLimitBLL
    {
        private readonly UserBetLimitService _userBetLimitService;
        private readonly TpBLL _tpBll;
        
        public UserBetLimitBLL(UserBetLimitService userBetLimitService, TpBLL tpBll)
        {
            _userBetLimitService = userBetLimitService;
            _tpBll = tpBll;
        }

        public async Task<IEnumerable<TpCategoryBetLimit>> GetTpCategoryList()
        {
            var tpCategories = await _userBetLimitService.GetTpCategoryList();

            return tpCategories.Select(category => new TpCategoryBetLimit() {CategoryId = category.CategoryId, CategoryName = category.CategoryName}).ToList();
        }

        public async Task<int> SetUserBetLimit(int custId, int siteId, List<BetSetting> betSetting, string updateOperator)
        {
            var sabaBetLimit = betSetting.SingleOrDefault(a => a.TpCategory == 1);
            if (sabaBetLimit != null)
            {
                var apiResult = await _tpBll.SetBetLimitAsync(1, custId, sabaBetLimit.MinBet, sabaBetLimit.MaxBet);
            }

            return await _userBetLimitService.InsertOrUpdate(custId, siteId, betSetting, updateOperator);
        }

        public IEnumerable<UserBetLimit> GetUserBetLimit(IEnumerable<int> siteIds, string userName, long startDate, long endDate, int pageSize, int pageNumber, out int totalPages, out int rowCount)
        {
            return _userBetLimitService.GetUserBetLimit(siteIds, userName, startDate, endDate, pageSize, pageNumber, out totalPages, out rowCount);
        }

        public async Task<int> Delete(int custId)
        {
            var custBetSetting = await _userBetLimitService.UserBetLimitGetByCustId(custId);
            var betSetting = JsonConvert.DeserializeObject<List<BetSetting>>(custBetSetting.BetSetting);
            if (betSetting.Any(a => a.TpCategory == 1))
            {
                var sabaBetLimit = await _tpBll.GetBetLimitAsync(1, custId);
                var apiResult = await _tpBll.SetBetLimitAsync(1, custId, sabaBetLimit.Data);
                if (apiResult.ErrorCode != 0)
                {
                    throw new AdminException(ErrorCodeEnum.Fail, "update third party bet limit failed");
                }
            }
            
            return await _userBetLimitService.Delete(custId);
        }

        public async Task<int> UpdateStatus(int custId, int status, string updateOperator)
        {
            return await _userBetLimitService.UpdateStatus(custId, status, updateOperator);
        }
    }
}