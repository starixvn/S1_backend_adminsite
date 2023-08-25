using BackendSite.Service.DAL;
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

        public IEnumerable<GetLeaderboardReportResponse> GetLeaderboardReport(GetLeaderboardReportRequest request)
        {
            var data = _leaderBoardService.GetLeaderboardReport(
                request.SiteId,
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

    }
}
