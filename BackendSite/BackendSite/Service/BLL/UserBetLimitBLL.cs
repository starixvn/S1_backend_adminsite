using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public class UserBetLimitBLL
    {
        private readonly UserBetLimitService _userBetLimitService;

        public UserBetLimitBLL(UserBetLimitService userBetLimitService)
        {
            _userBetLimitService = userBetLimitService;
        }

        public async Task<IEnumerable<TpCategoryBetLimit>> GetTpCategoryList()
        {
            var tpCategories = await _userBetLimitService.GetTpCategoryList();

            return tpCategories.Select(category => new TpCategoryBetLimit() {CategoryId = category.CategoryId, CategoryName = category.CategoryName}).ToList();
        }

        public async Task<int> InsertOrUpdate(int custId, int siteId, List<BetSetting> betSetting, string updateOperator)
        {
            return await _userBetLimitService.InsertOrUpdate(custId, siteId, betSetting, updateOperator);
        }

        public IEnumerable<UserBetLimit> GetUserBetLimit(IEnumerable<int> siteIds, string userName, long startDate, long endDate, int pageSize, int pageNumber, out int totalPages, out int rowCount)
        {
            return _userBetLimitService.GetUserBetLimit(siteIds, userName, startDate, endDate, pageSize, pageNumber, out totalPages, out rowCount);
        }

        public async Task<int> Delete(int custId)
        {
            return await _userBetLimitService.Delete(custId);
        }
    }
}