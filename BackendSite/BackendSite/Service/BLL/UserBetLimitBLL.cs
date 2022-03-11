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

            return tpCategories.Select(category => new TpCategoryBetLimit() {CategoryId = category.CategoryId, CategoryName = category.CategoryName}).ToList();
        }

        public async Task<int> SetUserBetLimit(int custId, int siteId, List<BetSetting> betSetting, string updateOperator)
        {
            var sportBetSetting = betSetting.SingleOrDefault(a => a.TpCategory == 1);
            if (sportBetSetting != null)
            {
                var sabaBetSetting = sportBetSetting.TpBetSetting?.SingleOrDefault(a => a.GameTypeId == 69);
                if (sabaBetSetting != null)
                {
                    var minBet = sabaBetSetting.MinBet;
                    var maxBet = sabaBetSetting.MaxBet;
                    var apiResult = await _tpBll.SetBetLimitAsync(41, custId, minBet, maxBet);
                    if (apiResult.ErrorCode != (int)ErrorCodeEnum.Success)
                        throw new AdminException(ErrorCodeEnum.SystemException, "Set third party bet limit failed");
                }

              
            }

            return await _userBetLimitService.InsertOrUpdate(custId, siteId, betSetting, updateOperator);
        }

        public IEnumerable<UserBetLimit> GetUserBetLimit(IEnumerable<int> siteIds, string userName, DateTimeOffset startDate, DateTimeOffset endDate, int pageSize, int pageNumber, out int totalPages, out int rowCount)
        {
            return _userBetLimitService.GetUserBetLimit(siteIds, userName, startDate, endDate, pageSize, pageNumber, out totalPages, out rowCount);
        }

        public async Task<int> Delete(int custId)
        {
            var custBetSetting = await _userBetLimitService.UserBetLimitGetByCustId(custId);
            var betSetting = JsonConvert.DeserializeObject<List<BetSetting>>(custBetSetting.BetSetting);
            if (betSetting.Any(a => a.TpCategory == 1))
            {
                var sabaBetLimit = await _tpBll.GetBetLimitAsync(41, custId);
                var apiResult = await _tpBll.SetBetLimitAsync(41, custId, sabaBetLimit.Data);
                if (apiResult.ErrorCode != 0)
                {
                    throw new AdminException(ErrorCodeEnum.Fail, "reset third party bet limit failed");
                }
            }
            
            return await _userBetLimitService.Delete(custId);
        }

        public async Task<int> UpdateStatus(int custId, int status, string updateOperator)
        {
            return await _userBetLimitService.UpdateStatus(custId, status, updateOperator);
        }

        public async Task<IEnumerable<GameTypeBetLimit>> GetTpGameTypeListbyCategoryAsync(int categoryId)
        {
            var tpGameType = await _userBetLimitService.GetTpGameTypeListbyCategoryAsync(categoryId);
            var gameTypeWithBetLimit = tpGameType.Select(a => new GameTypeBetLimit()
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
                if (!string.IsNullOrEmpty(custBetSetting.BetSetting) && custBetSetting.BetSetting != "null")
                {
                    var betSettings = JsonConvert.DeserializeObject<List<BetSetting>>(custBetSetting.BetSetting);
                    
                    var categorySetting = betSettings.SingleOrDefault(a => a.TpCategory == category.CategoryId);
                    if (categorySetting != null)
                    {
                        categoryBetLimit.MinBet = categorySetting.MinBet;
                        categoryBetLimit.MaxBet = categorySetting.MaxBet;
                        categoryBetLimit.Checked = true;
                        var typeBetLimit = await _userBetLimitService.GetTpGameTypeListbyCategoryAsync(category.CategoryId);

                        var listTpGametype = new List<GameTypeBetLimit>();
                        foreach (var gameType in typeBetLimit)
                        {
                            var gameTypeBet = new GameTypeBetLimit()
                            {
                                TypeId = gameType.Type,
                                TypeName = gameType.TypeName
                            };
                        
                            var custTpBet = categorySetting.TpBetSetting?.SingleOrDefault(a => a.GameTypeId == gameType.Type);
                            if (custTpBet != null)
                            {
                                gameTypeBet.Checked = true;
                                gameTypeBet.MinBet = custTpBet.MinBet;
                                gameTypeBet.MaxBet = custTpBet.MaxBet;
                            }
                            listTpGametype.Add(gameTypeBet);
                        }
                        categoryBetLimit.GameTypeBetLimit = listTpGametype;
                    }
                }
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
    }
}