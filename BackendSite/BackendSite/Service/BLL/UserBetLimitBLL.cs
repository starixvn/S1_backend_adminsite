using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.ExceptionHandling;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
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
            
            return tpCategories.Where(a=> a.CategoryId == (int)TpCategoryEnum.Sports || a.CategoryId == (int)TpCategoryEnum.LiveCasino)
                .Select(category => new TpCategoryBetLimit() {CategoryId = category.CategoryId, CategoryName = category.CategoryName}).ToList();
        }

        public async Task<int> SetUserBetLimit(int custId, int siteId, List<TpBetSetting> betSetting, string updateOperator)
        {
            return await _userBetLimitService.InsertOrUpdate(custId, siteId, betSetting, updateOperator);
        }

        public IEnumerable<UserBetLimit> GetUserBetLimit(IEnumerable<int> siteIds, IEnumerable<int> currencyIds, string userName, DateTimeOffset startDate, DateTimeOffset endDate, int pageSize, int pageNumber, out int totalPages, out int rowCount)
        {
            return _userBetLimitService.GetUserBetLimit(siteIds,currencyIds,  userName, startDate, endDate, pageSize, pageNumber, out totalPages, out rowCount);
        }

        public async Task<int> Delete(int custId)
        {
            return await _userBetLimitService.Delete(custId);
        }

        public async Task<int> UpdateStatus(int custId, int status, string updateOperator)
        {
            return await _userBetLimitService.UpdateStatus(custId, status, updateOperator);
        }

        public async Task<IEnumerable<GameTypeBetLimit>> GetTpGameTypeListbyCategoryAsync(int categoryId)
        {
            var tpGameType = await _userBetLimitService.GetTpGameTypeListbyCategoryAsync(categoryId);
            var gameTypeWithBetLimit = tpGameType.Where(t => t.TpId == 4)
                .Select(a => new GameTypeBetLimit()
                {
                    TypeId = a.Type,
                    TypeName = a.TypeName
                });
            return gameTypeWithBetLimit;
        }

        public async Task<CustBetLimitByCustId> GetByCustId(int custId)
        {
            var custBetSetting = await _userBetLimitService.UserBetLimitGetByCustId(custId);
            
            
            var tpCategories = await _userBetLimitService.GetTpCategoryList();
            var listCategoryBetLimit = new List<TpCategoryBetLimit>();
            foreach (var category in tpCategories)
            {
                var categoryBetLimit = new TpCategoryBetLimit()
                {
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName
                };
                listCategoryBetLimit.Add(categoryBetLimit);
            }
            var resp = new CustBetLimitByCustId()
            {
                UserName = custBetSetting.UserName,
                SiteId = custBetSetting.SiteId,
                BetSetting = listCategoryBetLimit
            };
            return resp;
        }

        public async Task<IEnumerable<TpBetLimit>> GetTpBetLimit(int tpId, int currencyId)
        {
            return await _userBetLimitService.GetTpBetLimit(tpId, currencyId);
        }
    }
}