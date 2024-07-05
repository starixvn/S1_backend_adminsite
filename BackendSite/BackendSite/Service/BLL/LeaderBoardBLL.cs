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
        private readonly ILogger<UserBetLimitBLL> _logger;
        private readonly ICacheHelper _cacheHelper;

        public LeaderBoardBLL(IServiceProvider provider)
        {
            _leaderBoardService = provider.GetRequiredService<LeaderBoardService>();
            _adjustmentLogReportService = provider.GetRequiredService<AdjustmentLogReportService>();
            _logger = provider.GetRequiredService<ILogger<UserBetLimitBLL>>();
            _cacheHelper = provider.GetRequiredService<ICacheHelper>();
        }
        
        public IEnumerable<LeaderboardReportResponse> GetLeaderboardReportBySiteIds(GetLeaderboardReportRequest request)
        {
            var rankingBonus = _leaderBoardService.GetLeaderboardRankingBonus(JsonConvert.SerializeObject(request.SiteIds))
                .OrderByDescending(x=>x.Bonus).ToList();

            var response = new List<LeaderboardReportResponse>();
            
            foreach (var siteId in request.SiteIds)
            {
                var siteData = DealLeaderboardReport(siteId, request.CurrencyId, request.CategoryId, request.StartDate);
                
                var result = new LeaderboardReportResponse
                {
                    detail = GetLeaderboardReport(siteId, request.CategoryId, siteData, rankingBonus),
                    SiteId = siteId
                };
                
                response.Add(result);
            }
            
            return response;
        }

        public IEnumerable<GetLeaderboardReportResponse> ExportLeaderboardReport(GetLeaderboardReportRequest request)
        {
            var siteId = request.SiteIds.FirstOrDefault();
            var siteData = DealLeaderboardReport(siteId, request.CurrencyId, request.CategoryId, request.StartDate);
            
            var rankingBonus = _leaderBoardService.GetLeaderboardRankingBonus(JsonConvert.SerializeObject(request.SiteIds))
                .OrderByDescending(x=>x.Bonus).ToList();
            
            var result = GetLeaderboardReport(siteId, request.CategoryId, siteData, rankingBonus);
            
            return result;
        }
        
        private List<GetLeaderboardReportResponse> GetLeaderboardReport(int siteId , int categoryId , IEnumerable<Leaderboard> data , IEnumerable<LeaderboardRankingBonusData> rankingBonus)
        {
            var takeCount = siteId == 1 ? 30 : 100;
            var result = data
                .GroupBy(y => new {y.CustId, y.SiteId, y.SiteName, y.UserName, y.CurrencyId, y.IsoCode})
                .OrderByDescending(z => z.Sum(i => i.WTurnover))
                .Select((r, index) =>
                {
                    var bonusModel = rankingBonus.FirstOrDefault(rb =>
                        rb.SiteId == r.Key.SiteId &&
                        rb.CurrencyId == r.Key.CurrencyId &&
                        rb.UserCount > 0 &&
                        r.Sum(i => i.WTurnover) >= rb.MinPoint);

                    if (bonusModel is not null)
                    {
                        bonusModel.UserCount -= 1;
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
                        Reward = categoryId != 0 || bonusModel is null ? 0 : bonusModel.Bonus
                    };
                }).Take(takeCount).ToList();
            
            return result;
        }
        
        private IEnumerable<Leaderboard> DealLeaderboardReport( int siteId , int currencyId , int categoryId , DateTimeOffset startDate)
        {
            var date = startDate.ToString("yyyyMM");
            var data = GetLeaderboardListMonthWithCache(categoryId, siteId, currencyId, date).ToList();
            
            return data;
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
