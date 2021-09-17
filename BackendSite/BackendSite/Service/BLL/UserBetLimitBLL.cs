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
    }
}