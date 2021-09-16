using System.Collections.Generic;
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

        public async Task<IEnumerable<BetLimitTpCategory>> GetTpCategoryList()
        {
            return await _userBetLimitService.GetTpCategoryList();
        } 
    }
}