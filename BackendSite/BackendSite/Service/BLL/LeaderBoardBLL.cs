using BackendSite.Extensions;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Report;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class LeaderBoardBLL
    {
        private readonly LeaderBoardService _leaderBoardService;
        private readonly AdjustmentLogReportService _adjustmentLogReportService;
        private readonly CustInfoService _custInfoService;
        private readonly ILogger<UserBetLimitBLL> _logger;
        private readonly ICacheHelper _cacheHelper;

        public LeaderBoardBLL(IServiceProvider provider)
        {
            _leaderBoardService = provider.GetRequiredService<LeaderBoardService>();
            _adjustmentLogReportService = provider.GetRequiredService<AdjustmentLogReportService>();
            _custInfoService = provider.GetRequiredService<CustInfoService>();
            _logger = provider.GetRequiredService<ILogger<UserBetLimitBLL>>();
            _cacheHelper = provider.GetRequiredService<ICacheHelper>();
        }

        public IEnumerable<GetLeaderboardReportResponse> GetLeaderboardReport(GetLeaderboardReportRequest request)
        {
            var data = DealLeaderboardReport(request);

            var rankingBonus = _leaderBoardService.GetLeaderboardRankingBonus(JsonConvert.SerializeObject(request.SiteIds))
                .OrderByDescending(x=>x.Bonus).ToList();
            
            var groupData = data.GroupBy(x => new {x.CustId ,x.SiteId, x.SiteName, x.UserName ,x.CurrencyId ,x.IsoCode})
                .OrderByDescending(x => x.Sum(i => i.WTurnover))
                .Select((r, index) =>
                {
                    var bonusModel = rankingBonus.FirstOrDefault(rb =>
                        rb.SiteId == r.Key.SiteId &&
                        rb.CurrencyId == r.Key.CurrencyId &&
                        r.Sum(i => i.WTurnover) >= rb.MinPoint);

                    if (bonusModel is not null && bonusModel.Ranking != "9-30")
                    {
                        rankingBonus.Remove(bonusModel);
                    }
                    
                    return new GetLeaderboardReportResponse
                    {
                        Ranking = $"TOP {index + 1}",
                        SiteId = r.Key.SiteId,
                        SiteName = r.Key.SiteName,
                        UserName = r.Key.UserName,
                        CurrencyId = r.Key.CurrencyId,
                        IsoCode = r.Key.IsoCode,
                        CustId = r.Key.CustId,
                        Points = r.Sum(i => i.WTurnover),
                        Reward = bonusModel is null ? 0 :
                            request.CategoryId != 0 || (r.Sum(i => i.WTurnover) < bonusModel.MinPoint) ? 0 : bonusModel.Bonus
                    };
                }).Take(30);
                
            return groupData;
        }
        
        public IEnumerable<LeaderboardReportResponse> GetLeaderboardReportBySiteIds(GetLeaderboardReportRequest request)
        {
            var data = DealLeaderboardReport(request);
            
            var rankingBonus = _leaderBoardService.GetLeaderboardRankingBonus(JsonConvert.SerializeObject(request.SiteIds))
                .OrderByDescending(x=>x.Bonus).ToList();
            
            var groupData = data.GroupBy(x => x.SiteId)
                .Select(x => new LeaderboardReportResponse
                {
                    SiteId = x.Key,
                    detail = x.GroupBy(y => new { y.CustId, y.SiteId, y.SiteName, y.UserName, y.CurrencyId, y.IsoCode })
                        .OrderByDescending(z => z.Sum(i => i.WTurnover))
                        .Select((r, index) =>
                        {
                            var bonusModel = rankingBonus.FirstOrDefault(rb =>
                                rb.SiteId == r.Key.SiteId &&
                                rb.CurrencyId == r.Key.CurrencyId &&
                                r.Sum(i => i.WTurnover) >= rb.MinPoint);

                                if (bonusModel is not null && bonusModel.Ranking != "9-30")
                                {
                                    rankingBonus.Remove(bonusModel);
                                }
                                
                            return new GetLeaderboardReportResponse
                            {
                                Ranking = $"TOP {index + 1}",
                                SiteId = r.Key.SiteId,
                                SiteName = r.Key.SiteName,
                                UserName = r.Key.UserName,
                                CurrencyId = r.Key.CurrencyId,
                                IsoCode = r.Key.IsoCode,
                                CustId = r.Key.CustId,
                                Points = r.Sum(i => i.WTurnover),
                                Reward = request.CategoryId != 0 || bonusModel is null ? 0 : bonusModel.Bonus
                            };
                        }).Take(30).ToList()
                });
            
            return groupData;
        }

        private IEnumerable<Leaderboard> DealLeaderboardReport(GetLeaderboardReportRequest request)
        {
            var dataList = new List<Leaderboard>();
            var date = request.StartDate.DateTime.ToString("yyyyMM");

            foreach (var siteId in request.SiteIds)
            {
                var dataOrigin = GetLeaderboardListMonthWithCache(request.CategoryId, siteId, request.CurrencyId, date);
                
                if (dataOrigin is not null)
                {
                    List<LeaderBoardCustInfo> filteredList;
                    var getCount = 50;
                    
                    do
                    {
                        filteredList = new List<LeaderBoardCustInfo>();
                    
                        //先排名
                        dataOrigin = dataOrigin.OrderByDescending(x => x.WTurnover).ToList();
                    
                        //拉 {getCount} 個CustId 出來
                        var custIds = dataOrigin.Select(x => x.CustId).Take(getCount);
                    
                        //找 {getCount} 個custInfo 出來
                        var custInfoList = _custInfoService.GetCustInfoByCustIds(request.SiteIds, custIds).ToList();
                
                        // 要排除掉的Tag清單
                        var excludeTagBySite = _leaderBoardService.GetLeaderboardTagExcludeList(siteId).TagsExclude;
                    
                        // 排除掉Tag清單後的 custInfo清單
                        var filtered = custInfoList.Where(x => x.Tags is null || x.Tags.All(tag => !excludeTagBySite.Contains(tag))).ToList();

                        if (filtered.Any())
                        {
                            filteredList.AddRange(filtered);
                        }
                        
                        // 數量夠了 break
                        if (filteredList.Count <= 30)
                        {
                            break;
                        }
                
                        //篩選後的筆數不夠 重拉
                        getCount += 50;
                    } 
                    while (filteredList.Count < 30);

                    var filterOrigin = dataOrigin.Where(x => filteredList.Select(y => y.CustId).Contains(x.CustId)).ToList();
                    
                    if (filterOrigin.Any())
                    {
                        dataList.AddRange(filterOrigin);
                    }
                    
                }
            }
            
            return dataList;
        }
        
        private IEnumerable<Leaderboard> GetLeaderboardListMonthWithCache(int lBCategoryId, int siteId, int currencyId, string yyyyMM)
        {
            var key = $"LeaderboardListMonth:{lBCategoryId}:{siteId}:{currencyId}:{yyyyMM}";
            var json = _cacheHelper.StringGetAsync(key, RedisDataBase.Member).Result;
            return JsonConvert.DeserializeObject<IEnumerable<Leaderboard>>(json);
        }
        
        public IEnumerable<LeaderboardExtraPointList> GetLeaderboardExtraPointList(
            int siteId,
            IEnumerable<int> categoryIds,
            IEnumerable<int> currencyIds,
            DateTime startDate,
            DateTime endDate,
            int pageNumber,
            int pageSize, out int totalPages, out int rowCount)
        {
            return _leaderBoardService.GetLeaderboardExtraPointList(
                siteId,
                JsonConvert.SerializeObject(categoryIds),
                JsonConvert.SerializeObject(currencyIds),
                startDate,
                endDate,
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize), out totalPages, out rowCount);
        }

        public IEnumerable<LeaderboardExtraPointList> GetLeaderboardExtraPointListExport(int siteId, IEnumerable<int> categoryIds, IEnumerable<int> currencyIds, DateTime startDate, DateTime endDate)
        {
            return _leaderBoardService.GetLeaderboardExtraPointListExport(siteId, JsonConvert.SerializeObject(categoryIds), JsonConvert.SerializeObject(currencyIds), startDate, endDate);
        }

        public int SetLeaderboardExtraPoint(
            DateTime activeDate,
            int siteId,
            int categoryId,
            string userName,
            decimal extraPoint,
            string lastUpdateUser)
        {
            return _leaderBoardService.SetLeaderboardExtraPoint(activeDate, siteId, categoryId, userName, extraPoint, lastUpdateUser);
        }

        public bool SetLeaderboardExtraPoint(IEnumerable<SetLeaderboardExtraPointModel> input)
        {
            return _leaderBoardService.SetBatchLeaderboardExtraPoint(input);
        }

        public IEnumerable<LeaderboardRankingBonusData> GetLeaderboardRankingBonus(string siteIds)
        {
            return _leaderBoardService.GetLeaderboardRankingBonus(siteIds);
        }
        
        public bool SetLeaderboardRankingBonus(ParamsWithLog<SetLeaderboardRankingBonusReq> input)
        {
            var result = _leaderBoardService.SetLeaderboardRankingBonus(
                input.SiteId,
                input.CurrencyId,
                input.After.Ranking,
                input.After.MinPoint,
                input.After.Bonus, 
                input.Operator);

            if (result)
            {
                var logParam = new CreateAdminAdjustmentInfoLogInput
                {
                    SiteId = input.SiteId,
                    CurrencyId = input.CurrencyId,
                    MenuEnum = AdminAdjustmentLogMenuEnum.LeaderboardRankingBonus,
                    ActionType = ActionType.Update,
                    BeforeDetailInfo = input.Before.ToLog(),
                    AfterDetailInfo = input.After.ToLog(),
                    Operator = input.Operator
                };

                if (!_adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParam))
                {
                    _logger.LogError("CreateAdminAdjustmentInfoLog failed. params: " + JsonConvert.SerializeObject(logParam));
                }
            }
            
            return result;
        }
        
        public IEnumerable<LeaderboardPointCalculation> GetLeaderboardPointCalculation(string siteIds ,string date)
        {
            return _leaderBoardService.GetLeaderboardPointCalculation(siteIds ,date);
        }
        
        public bool SetLeaderboardPointCalculation(ParamsWithLog<SetLeaderboardPointCalculationReq> input ,string date)
        {
            var result = _leaderBoardService.SetLeaderboardPointCalculation(
                input.SiteId,
                input.CurrencyId,
                input.After.SlotTurnOver,
                input.After.SportTurnOver,
                input.After.LiveCasinoTurnOver,
                input.After.Status, 
                date, 
                input.Operator);

            if (result)
            {
                var logParam = new CreateAdminAdjustmentInfoLogInput
                {
                    SiteId = input.SiteId,
                    CurrencyId = input.CurrencyId,
                    MenuEnum = AdminAdjustmentLogMenuEnum.LeaderboardPointCalculation,
                    ActionType = ActionType.Update,
                    BeforeDetailInfo = input.Before.ToLog(),
                    AfterDetailInfo = input.After.ToLog(),
                    Operator = input.Operator
                };

                if (!_adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParam))
                {
                    _logger.LogError("CreateAdminAdjustmentInfoLog failed. params: " + JsonConvert.SerializeObject(logParam));
                }
            }
           
            return result;
        }
        
        public LeaderboardTagExcludeList GetLeaderboardTagExcludeList(int siteId)
        {
            return _leaderBoardService.GetLeaderboardTagExcludeList(siteId);
        }
        
        public bool SetLeaderboardTagExcludeList(ParamsWithLog<SetLeaderboardTagExcludeReq> req )
        {
            var result = _leaderBoardService.SetLeaderboardTagExcludeList(req.SiteId ,req.After.TagExcludeList  ,req.Operator);
            
            if (result)
            {
                var logParam = new CreateAdminAdjustmentInfoLogInput
                {
                    SiteId = req.SiteId,
                    CurrencyId = 0,
                    MenuEnum = AdminAdjustmentLogMenuEnum.LeaderboardRankingBonus,
                    ActionType = ActionType.Update,
                    BeforeDetailInfo = req.Before.ToLog(),
                    AfterDetailInfo = req.After.ToLog(),
                    Operator = req.Operator
                };

                if (!_adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParam))
                {
                    _logger.LogError("SetLeaderboardTagExcludeList failed. params: " + JsonConvert.SerializeObject(logParam));
                }
            }
            
            return result;
        }
    }
}
