using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.ExceptionHandling;
using BackendSite.Service.DAL;
using BackendSite.Service.DAL.BetLimit;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

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
            
            return tpCategories.Where(a=> a.CategoryId == (int)TpCategoryEnum.LiveCasino)
                .Select(category => new TpCategoryBetLimit() {CategoryId = category.CategoryId, CategoryName = category.CategoryName}).ToList();
        }

        public async Task<int> SetUserBetLimit(int custId, int siteId, List<BetSetting> betSetting, string updateOperator)
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
            var gameTypeWithBetLimit = tpGameType
                .Select(a => new GameTypeBetLimit()
                {
                    TypeId = a.Type,
                    TypeName = a.TypeName
                });
            return gameTypeWithBetLimit;
        }

        public async Task<IEnumerable<TpBetLimitGroup>> GetTpBetLimitGroupAsync(int categoryId, int currencyId)
        {
            var tpGameType = await _userBetLimitService.GetTpGameTypeListbyCategoryAsync(categoryId);
            var listTpLimitGroup = new List<TpBetLimitGroup>();
            foreach (var type in tpGameType)
            {
                var betLimit = BetLimitFactory.Create(_userBetLimitService.ReportDbClient, (TpGameTypeEnum) type.Type);
                if (betLimit == null) continue;
                var group = await betLimit.GetLimitGroup(currencyId);
                var tpBetLimitGroup = new TpBetLimitGroup()
                {
                    TypeId = type.Type,
                    TypeName = type.TypeName,
                    SelectedValue = group.DefaultGroup,
                    LimitGroup = JsonConvert.DeserializeObject<List<LimitGroup>>(group.LimitGroup)
                };
                listTpLimitGroup.Add(tpBetLimitGroup);
            }
            return listTpLimitGroup;
        }

        public async Task<CustBetLimitByCustId> GetByCustId(int custId)
        {
            var custBetSetting = await _userBetLimitService.UserBetLimitGetByCustId(custId);
            var betSetting = JsonConvert.DeserializeObject<List<BetSetting>>(custBetSetting.BetSetting);
            
            var tpCategories = await _userBetLimitService.GetTpCategoryList();
            var listCategoryBetLimit = new List<TpCategoryBetLimit>();
            foreach (var category in tpCategories.Where(a=> a.CategoryId == (int)TpCategoryEnum.LiveCasino))
            {
                var tpGameType = await _userBetLimitService.GetTpGameTypeListbyCategoryAsync(category.CategoryId);
                var limitGroups = new List<TpBetLimitGroup>();
                var liveCasinoSetting = betSetting.FirstOrDefault(a=>a.TpCategory == category.CategoryId);
                foreach (var gameType in tpGameType)
                {
                    var betLimit = BetLimitFactory.Create(_userBetLimitService.ReportDbClient, (TpGameTypeEnum) gameType.Type);
                    if(liveCasinoSetting == null || betLimit == null) continue;
                    var group = await betLimit.GetLimitGroup(custBetSetting.CurrencyId, liveCasinoSetting.TpBetSetting);
                    var tpGroup = new TpBetLimitGroup()
                    {
                        TypeId = gameType.Type,
                        TypeName = gameType.TypeName,
                        Checked = true,
                        SelectedValue = group.LimitGroup,
                        LimitGroup = JsonConvert.DeserializeObject<List<LimitGroup>>(group.LimitGroup)
                    };
                    limitGroups.Add(tpGroup);
                }
                
                

                var categoryBetLimit = new TpCategoryBetLimit()
                {
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName,
                    Checked = true,
                    GameTypeBetLimit = limitGroups
                };
                listCategoryBetLimit.Add(categoryBetLimit);
            }
            var resp = new CustBetLimitByCustId()
            {
                SiteId = custBetSetting.SiteId,
                SiteName = custBetSetting.SiteName,
                custId = custBetSetting.CustId,
                UserName = custBetSetting.UserName,
                CurrencyId = custBetSetting.CurrencyId,
                BetSetting = listCategoryBetLimit
            };
            return resp;
        }
    }
}
