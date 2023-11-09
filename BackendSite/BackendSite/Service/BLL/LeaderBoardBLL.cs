using BackendSite.Service.DAL;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Report;

using Newtonsoft.Json;


namespace BackendSite.Service.BLL
{
    public class LeaderBoardBLL
    {
        private readonly LeaderBoardService _leaderBoardService;

        public LeaderBoardBLL(IServiceProvider provider)
        {
            _leaderBoardService = provider.GetRequiredService<LeaderBoardService>();
        }

        public IEnumerable<GetLeaderboardReportResponse> GetLeaderboardReport(GetLeaderboardReportRequest request , bool newCalculation)
        {
            if (newCalculation)
            {
                var data = DealLeaderboardReport(request);

                var rankingBonus = _leaderBoardService.GetLeaderboardRankingBonus(JsonConvert.SerializeObject(request.SiteIds));
            
                var groupData = data.GroupBy(x => new {x.CustId ,x.SiteId, x.SiteName, x.UserName ,x.CurrencyId ,x.IsoCode})
                    .OrderByDescending(x => x.Sum(i => i.Points))
                    .Select((r, index) =>
                    {
                        var bonusModel = rankingBonus.FirstOrDefault(rb =>
                            rb.SiteId == r.Key.SiteId &&
                            rb.CurrencyId == r.Key.CurrencyId &&
                            (rb.Ranking == (index + 1).ToString() || rb.Ranking == "9-30"));
                    
                        return new GetLeaderboardReportResponse
                        {
                            Ranking = $"TOP {index + 1}",
                            SiteId = r.Key.SiteId,
                            SiteName = r.Key.SiteName,
                            UserName = r.Key.UserName,
                            CurrencyId = r.Key.CurrencyId,
                            IsoCode = r.Key.IsoCode,
                            CustId = r.Key.CustId,
                            TurnOver = r.Sum(i => i.TurnOver),
                            Points = r.Sum(i => i.Points),
                            Reward = request.CategoryId != 0 || r.Sum(i => i.Points) < bonusModel.MinPoint ? 0 : bonusModel.Bonus
                        };
                    }).Take(30);
                
                return groupData;
            }
            else
            {
                var data = _leaderBoardService.GetLeaderboardReportOld(
                    JsonConvert.SerializeObject(request.SiteIds),
                    request.CurrencyId,
                    request.CategoryId,
                    request.StartDate.DateTime,
                    request.EndDate.DateTime);

                var groupData = data.GroupBy(x => x.CustId)
                    .OrderByDescending(x => x.Sum(i => i.Points))
                    .Select((r, index) => new GetLeaderboardReportResponse
                    {
                        Ranking = $"TOP {index + 1}",
                        SiteId = request.SiteId,
                        UserName = r.FirstOrDefault()?.UserName,
                        CurrencyId = request.CurrencyId,
                        IsoCode = r.FirstOrDefault()?.IsoCode,
                        CustId = r.Key,
                        TurnOver = r.Sum(i => i.TurnOver),
                        Points = r.Sum(i => i.Points)
                    }).Take(30);

                return groupData;
            }
        }
        
        public IEnumerable<LeaderboardReportResponse> GetLeaderboardReportBySiteIds(GetLeaderboardReportRequest request)
        {
            var data = DealLeaderboardReport(request);
            
            var rankingBonus = _leaderBoardService.GetLeaderboardRankingBonus(JsonConvert.SerializeObject(request.SiteIds));
            
            var groupData = data.GroupBy(x => x.SiteId)
                .Select(x => new LeaderboardReportResponse
                {
                    SiteId = x.Key,
                    detail = x.GroupBy(y => new { y.CustId, y.SiteId, y.SiteName, y.UserName, y.CurrencyId, y.IsoCode })
                        .OrderByDescending(z => z.Sum(i => i.Points))
                        .Select((r, index) =>
                        {
                            var bonusModel = rankingBonus.FirstOrDefault(rb =>
                                rb.SiteId == r.Key.SiteId &&
                                rb.CurrencyId == r.Key.CurrencyId &&
                                (rb.Ranking == (index + 1).ToString() || rb.Ranking == "9-30"));
                            
                            return new GetLeaderboardReportResponse
                            {
                                Ranking = $"TOP {index + 1}",
                                SiteId = r.Key.SiteId,
                                SiteName = r.Key.SiteName,
                                UserName = r.Key.UserName,
                                CurrencyId = r.Key.CurrencyId,
                                IsoCode = r.Key.IsoCode,
                                CustId = r.Key.CustId,
                                TurnOver = r.Sum(i => i.TurnOver),
                                Points = r.Sum(i => i.Points),
                                Reward = request.CategoryId != 0 || r.Sum(i => i.Points) < bonusModel.MinPoint ? 0 : bonusModel.Bonus
                            };
                        }).Take(30).ToList()
                });
            
            return groupData;
        }

        private IEnumerable<LeaderboardReportModel> DealLeaderboardReport(GetLeaderboardReportRequest request)
        {
            var data = _leaderBoardService.GetLeaderboardReport(
                JsonConvert.SerializeObject(request.SiteIds),
                request.CurrencyId,
                request.CategoryId,
                request.StartDate.DateTime,
                request.EndDate.DateTime).ToList();
            
            var date = request.StartDate.ToString("yyyyMM");
            var pointCalculation = _leaderBoardService.GetLeaderboardPointCalculation(JsonConvert.SerializeObject(request.SiteIds) ,date);

            //GroupBy CustId,CategoryId 
            var groupCategoryIdData = data.GroupBy(x => new
                {
                    x.SiteId, x.UserName, x.CurrencyId, x.IsoCode, x.CategoryId, x.CustId, x.SiteName
                })
                .Select(x => new LeaderboardReportModel
                {
                    SiteId = x.Key.SiteId,
                    UserName = x.Key.UserName,
                    CurrencyId = x.Key.CurrencyId,
                    IsoCode = x.Key.IsoCode,
                    CategoryId = x.Key.CategoryId,
                    CustId = x.Key.CustId,
                    TurnOver = x.Sum(y=>y.TurnOver),
                    SiteName = x.Key.SiteName
                }).ToList();
            
            // 計算Points with SlotTurnOver,SportTurnOver,LiveCasinoTurnOver
            foreach (var detail in groupCategoryIdData)
            {
                var cal = pointCalculation.FirstOrDefault(x =>
                    x.SiteId == detail.SiteId && x.CurrencyId == detail.CurrencyId && x.Status == 1);
                
                if (cal != null)
                {
                    detail.Points = detail.CategoryId switch
                    {
                        (int)TpCategoryEnum.Sports => (int)(detail.TurnOver / cal.SportTurnOver),
                        (int)TpCategoryEnum.Casino => (int)(detail.TurnOver / cal.SlotTurnOver),
                        (int)TpCategoryEnum.LiveCasino => (int)( detail.TurnOver / cal.LiveCasinoTurnOver),
                        _ => 0
                    };
                }
            }
            
            return groupCategoryIdData;
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
        
        public bool SetLeaderboardRankingBonus(SetLeaderboardRankingBonusReq req , string updater)
        {
            return _leaderBoardService.SetLeaderboardRankingBonus(req, updater);
        }
        
        public IEnumerable<LeaderboardPointCalculation> GetLeaderboardPointCalculation(string siteIds ,string date)
        {
            return _leaderBoardService.GetLeaderboardPointCalculation(siteIds ,date);
        }
        
        public bool SetLeaderboardPointCalculation(SetLeaderboardPointCalculationReq req ,string date, string updater)
        {
            return _leaderBoardService.SetLeaderboardPointCalculation(req, date, updater);
        }
    }
}
