using BackendSite.Service.DAL;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.Report;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;


namespace BackendSite.Service.BLL
{
    public class LeaderBoardBLL
    {
        private readonly LeaderBoardService _leaderBoardService;
        private readonly ServerInfoBLL _serverInfoBll;

        public LeaderBoardBLL(IServiceProvider provider)
        {
            _leaderBoardService = provider.GetRequiredService<LeaderBoardService>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
        }

        public IEnumerable<GetLeaderboardReportResponse> GetLeaderboardReport(GetLeaderboardReportRequest request)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            var multiSiteRequest = ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);

            var data = _leaderBoardService.GetLeaderboardReport(
                JsonConvert.SerializeObject(multiSiteRequest),
                request.CategoryId,
                request.StartDate.DateTime,
                request.EndDate.DateTime );
            
            var groupData = data.GroupBy(x => x.CustId)
                .OrderByDescending(x => x.Sum(i => i.Points))
                .Select( (r, index) => new GetLeaderboardReportResponse
                {
                    Ranking = $"TOP {index + 1}",
                    SiteId = (int)(r.FirstOrDefault()?.SiteId),
                    SiteName = r.FirstOrDefault()?.SiteName,
                    UserName = r.FirstOrDefault()?.UserName,
                    CurrencyId = (int)(r.FirstOrDefault()?.CurrencyId),
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
