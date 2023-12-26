using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public class HotEventBLL
    {
        private readonly ILogger<HotEventBLL> _logger;
        private readonly IHotEventService _hotEventService;
        public HotEventBLL(IHotEventService hotEventService, ILogger<HotEventBLL> logger)
        {
            this._hotEventService = hotEventService;
            this._logger = logger;
        }

        public async Task<IEnumerable<SportLeagueTitleOptionResponse>> GetSportLeagueTitleAsync()
        {
            return await _hotEventService.GetSportLeagueTitleOptionAsync();
        }

        public async Task<IEnumerable<SportLeaguesOptionResponse>> GetSportLeaguesAsync()
        {
            return await _hotEventService.GetSportLeaguesAsync();
        }

        public async Task<IEnumerable<SportTeamOptionResponse>> GetSportTeamOptionAsync(SportTeamsRequest request)
        {
            return await _hotEventService.GetSportTeamAsync(request.SportLeaguesId);
        }

        public async Task CreateHotEventAsync(CreateHotEventRequest request, string createUser)
        {
            await _hotEventService.CreateHotEventAsync(request, createUser);
        }

        public async Task<HotEventDetailResponse> GetHotEventDetail(HotEventDetailRequest request)
        {
            var data = await _hotEventService.GetHotEventDetailAsync(request.Id);
            if (data == null) throw new Exception("HotEvent Data Not Found");

            return new HotEventDetailResponse()
            {
                Id = data.Id,
                SportLeaguesTitleId = data.SportLeaguesTitleId,
                MatchId = data.MatchId,
                StartTime = DatetimeFormat(data.StartTime),
                EndTime = DatetimeFormat(data.EndTime),
                BattleTime = DatetimeFormat(data.BattleTime),
                HomeSportLeaguesId = data.HomeSportLeaguesId,
                AwaySportLeaguesId = data.AwaySportLeaguesId,
                HomeTeamNameId = data.HomeSportTeamId,
                AwayTeamNameId = data.AwaySportTeamId,
                ICurrencyList = data.ICurrencyList,
                IsLiveStreaming = data.IsLiveStreaming
            };
        }

        public async Task<HotEventDataListResponseWrapper> GetHotEventDataListAsync(HotEventDataListRequest request)
        {
            var queryResult = await _hotEventService.GetHotEventDataLisAsync(request);
            if (!queryResult.Data.Any()) return new HotEventDataListResponseWrapper() { Data = new List<HotEventDataListResponse>() };

            var response = new HotEventDataListResponseWrapper
            {
                Data = queryResult.Data.Select(r => new HotEventDataListResponse()
                {
                    Id = r.Id,
                    SportLeaguesTitle = r.SportLeaguesTitle,
                    HomeTeamName = r.HomeTeamName,
                    AwayTeamName = r.AwayTeamName,
                    Status = r.Status,
                    StartTime = DatetimeFormat(r.StartTime),
                    EndTime = DatetimeFormat(r.EndTime),
                    BattleTime = DatetimeFormat(r.BattleTime),
                    LastUpdateUser = r.LastUpdateUser,
                    LastUpdateTime = DatetimeFormat(r.LastUpdateTime)
                }),
                RowCount = queryResult.RowCount,
                TotalPages = queryResult.TotalPages
            };

            return response;
        }

        public async Task SetHotEventData(SetHotEventDataRequest request, string updateUser)
        {
            await _hotEventService.SetHotEventDataAsync(request, updateUser);
        }

        public async Task SetHotEventDataAsync(SetHotEventStatusRequest request, string updateUser)
        {
            await _hotEventService.SetHotEventStatusAsync(request, updateUser);
        }

        public async Task DeleteHotEventAsync(DeleteHotEventRequest request)
        {
            await _hotEventService.DeleteHotEventAsync(request.Id);
        }

        private string DatetimeFormat(DateTime? dateTime)
        {
            return dateTime.HasValue ? $"{dateTime.Value:yyyy-MM-ddTHH:mm:ss}+00:00" : null;
        }
    }
}




