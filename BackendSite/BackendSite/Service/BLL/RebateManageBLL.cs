using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class RebateManageBLL
    {
        private readonly RebateManageService _rebateManageService;
        private readonly UserBetLimitBLL _userBetLimitBLL;
        private readonly VipLevelBLL _vipLevelBLL;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly MySqlClient _mainDbClient;
        private readonly ILogger<RebateManageBLL> _logger;

        public RebateManageBLL(IServiceProvider provider)
        {
            _rebateManageService = provider.GetRequiredService<RebateManageService>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _logger = provider.GetRequiredService<ILogger<RebateManageBLL>>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _userBetLimitBLL = provider.GetRequiredService<UserBetLimitBLL>();
            _vipLevelBLL = provider.GetRequiredService<VipLevelBLL>();
        }

        public IEnumerable<TPCategory> GetTPCategoryList()
        {
            return _rebateManageService.GetTPCategoryList();
        }
        public IEnumerable<TpGameType> GetTpGameTypeListByCategory(int categoryId)
        {
            return _rebateManageService.GetTpGameTypeListByCategory(categoryId);
        }
        public IEnumerable<RebatePayoutLimitData> GetRebatePayoutLimit(int siteId, int currencyId, int? categoryId)
        {
            return _rebateManageService.GetRebatePayoutLimit(siteId, currencyId, categoryId);
        }
        public async Task<bool> SetRebatePayoutLimitAsync(int siteId, int currencyId, int categoryId, decimal maxPayout, decimal minPayout, string lastUpdateUser)
        {
            return await _rebateManageService.SetRebatePayoutLimitAsync(siteId, currencyId, categoryId, maxPayout, minPayout, lastUpdateUser);
        }
        public bool SetRebatePayoutLimitBatch(int siteId, int currencyId, string levelPayout, string lastUpdateUser)
        {
            return _rebateManageService.SetRebatePayoutLimitBatch(siteId, currencyId, levelPayout, lastUpdateUser);
        }
        public bool DelRebatePayoutLimit(int siteId, int currencyId, int categoryId, string lastUpdateUser)
        {
            return _rebateManageService.DelRebatePayoutLimit(siteId, currencyId, categoryId, lastUpdateUser);
        }
        public RebatePeriod GetRebatePeriod(int siteId)
        {
            return _rebateManageService.GetRebatePeriod(siteId).FirstOrDefault();
        }
        public async Task<bool> SetRebatePeriodAsync(int siteId, int periodType, int? periodDay, int redeemHours, string lastUpdateUser)
        {
            var siteData = _serverInfoBll.GetSiteList().Where(x => x.SiteId == siteId).FirstOrDefault();
            return await _rebateManageService.SetRebatePeriodAsync(siteId, siteData.ICurrencyList, periodType, periodDay, redeemHours, lastUpdateUser);
        }
        public async Task<IEnumerable<RebateGameTypeDetail>> GetRebateGameTypeList(int siteId, int currencyId, int rebateCategoryId)
        {
            var result = new List<RebateGameTypeDetail>();
            var tpGameType = await _userBetLimitBLL.GetTpGameTypeListbyCategoryAsync(rebateCategoryId);
            var vipLevelList = _vipLevelBLL.GetVipLevelSetting(siteId);
            foreach (var item in tpGameType)
            {
                var data = _rebateManageService.GetRebateDataByGameType(siteId, item.TypeId, currencyId);
                if (data.Any())
                {
                    var existLevelData = data.Select(x => x.Level);
                    foreach (var level in vipLevelList)
                    {
                        if (existLevelData.Contains(level.Level))
                        {
                            result.Add(new RebateGameTypeDetail()
                            {
                                GameType = item.TypeId,
                                GameTypeName = item.TypeName,
                                Level = level.Level,
                                Percentage = data.Where(x => x.Level == level.Level).Select(x => x.Percentage).FirstOrDefault()
                            });
                        }
                        else
                        {
                            result.Add(new RebateGameTypeDetail()
                            {
                                GameType = item.TypeId,
                                GameTypeName = item.TypeName,
                                Level = level.Level,
                                Percentage = null
                            });
                        }
                    }
                }
                else
                {
                    result.AddRange(vipLevelList.Select(x => new RebateGameTypeDetail()
                    {
                        GameType = item.TypeId,
                        GameTypeName = item.TypeName,
                        Level = x.Level,
                        Percentage = null
                    }));
                }
            }
            return result;
        }
        public async Task<bool> BatchSetRebateGameTypeAsync(int siteId, int currencyId, IEnumerable<RebateGameTypeDetail> details, string lastUpdateUser)
        {
            await using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            await using var trans = await conn.BeginTransactionAsync();

            foreach (var item in details)
            {
                if (!await _rebateManageService.SetRebateGameTypeAsync(siteId, currencyId, item.GameType, item.Level, (decimal)item.Percentage, lastUpdateUser, conn, trans))
                {
                    _logger.LogError($"SetRebateGameType Fail.SiteId={siteId}. CurrencyId={currencyId}. GameType={item.GameType}. Level={item.Level}. Percentage={item.Percentage}. lastUpdateUser={lastUpdateUser}");
                    trans.Rollback();
                    return false;
                }
            }

            trans.Commit();
            return true;
        }
        public bool DelRebatePeriod(int siteId, string lastUpdateUser)
        {
            return _rebateManageService.DelRebatePeriod(siteId, lastUpdateUser);
        }
        public IEnumerable<RebateLevelData> GetRebateDataByCategory(int siteId, int categoryId, int currencyId)
        {
            return _rebateManageService.GetRebateDataByCategory(siteId, categoryId, currencyId);
        }
        public RebateCurrencyData GeRebateInfo(int siteId, int currencyId)
        {
            var result = new RebateCurrencyData
            {
                CurrencyId = currencyId,
                PayoutLimitList = GetRebatePayoutLimit(siteId, currencyId, null)
            };
            var rList = GetTPCategoryList().Select(x => new RebateCategoryList() { CategoryId = x.CategoryId, CategoryName = x.CategoryName, CategoryLevelList = GetRebateDataByCategory(siteId, x.CategoryId, currencyId) }).ToList();
            result.CategoryDataList = rList;
            return result;
        }
        public IEnumerable<RebateLevelData> GetRebateDataByGameType(int siteId, int gameType, int currencyId)
        {
            return _rebateManageService.GetRebateDataByGameType(siteId, gameType, currencyId);
        }
        public RebateCategoryDetail GeRebateCategoryDetail(int siteId, int currencyId, int categoryId)
        {
            var reault = new RebateCategoryDetail
            {
                CategoryId = categoryId
            };

            var rList = new List<RebateGameTypeList>();
            foreach (var x in GetTpGameTypeListByCategory(categoryId))
            {
                rList.Add(new RebateGameTypeList() { Type = x.Type, TypeName = x.TypeName, GameTypeLevelList = GetRebateDataByGameType(siteId, x.Type, currencyId) });
            }
            reault.GameTypeList = rList;
            return reault;
        }
        public async Task<bool> SetRebateCategoryAsync(int siteId, int currencyId, int categoryId, int level, decimal percentage, string lastUpdateUser)
        {
            return await _rebateManageService.SetRebateCategoryAsync(siteId, currencyId, categoryId, level, percentage, lastUpdateUser);
        }
        public bool SetRebateCategoryBatch(int siteId, int currencyId, int categoryId, string levelPercentage, string lastUpdateUser)
        {
            return _rebateManageService.SetRebateCategoryBatch(siteId, currencyId, categoryId, levelPercentage, lastUpdateUser);
        }
        public bool DelRebateCategory(int siteId, int currencyId, int categoryId, int level, string lastUpdateUser)
        {
            return _rebateManageService.DelRebateCategory(siteId, currencyId, categoryId, level, lastUpdateUser);
        }
        public async Task<bool> SetRebateGameTypeAsync(int siteId, int currencyId, int gameType, int level, decimal percentage, string lastUpdateUser)
        {
            await using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            await using var trans = await conn.BeginTransactionAsync();
            if (!await _rebateManageService.SetRebateGameTypeAsync(siteId, currencyId, gameType, level, percentage, lastUpdateUser, conn, trans))
            {
                trans.Rollback();
                return false;
            }

            trans.Commit();
            return true;
        }
        public bool SetRebateGameTypeBatch(int siteId, int currencyId, int gameType, string levelPercentage, string lastUpdateUser)
        {
            return _rebateManageService.SetRebateGameTypeBatch(siteId, currencyId, gameType, levelPercentage, lastUpdateUser);
        }
        public bool DelRebateGameType(int siteId, int currencyId, int gameType, int level, string lastUpdateUser)
        {
            return _rebateManageService.DelRebateGameType(siteId, currencyId, gameType, level, lastUpdateUser);
        }
    }
}
