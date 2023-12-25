using System.Data;

using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;

namespace BackendSite.Service.DAL
{
    public class HotEventService : IHotEventService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public HotEventService(IEnumerable<MySqlClient> sqlClients)
        {
            _mainDbClient = sqlClients.First(p => p.DataBase == DataBase.Main);
            _reportDbClient = sqlClients.First(p => p.DataBase == DataBase.Report);
        }

        public Task<IEnumerable<SportLeagueTitleOptionResponse>> GetSportLeagueTitleOptionAsync()
        {
            return _reportDbClient.QueryAsync<SportLeagueTitleOptionResponse>("Admin_SportLeagueTitleOption_Get");
        }

        public Task<IEnumerable<SportLeaguesOptionResponse>> GetSportLeaguesAsync()
        {
            return _reportDbClient.QueryAsync<SportLeaguesOptionResponse>("Admin_SportLeaguesOption_Get");
        }

        public Task<IEnumerable<SportTeamOptionResponse>> GetSportTeamAsync(int sportLeaguesId)
        {
            var p = new DynamicParameters();
            p.Add("_SportLeaguesId", sportLeaguesId);
            return _reportDbClient.QueryAsync<SportTeamOptionResponse>("Admin_SportTeamsOption_Get", p);
        }

        public async Task<HotEventDataListDtoWrapper> GetHotEventDataLisAsync(HotEventDataListRequest request)
        {
            var result = new HotEventDataListDtoWrapper();
            var p = new DynamicParameters();
            p.Add("_SiteId", request.SiteId);
            p.Add("_Status", (byte)request.Status);
            p.Add("_SportLeaguesTitleId", request.SportLeaguesTitleId);
            p.Add("_PageSize", Math.Abs(request.PageSize));
            p.Add("_StartIndex", (Math.Abs(request.PageNumber) - 1) * Math.Abs(request.PageSize));
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            result.Data = await _reportDbClient.QueryAsync<HotEventDataListDto>("Admin_HotEventDataList_Get", p);
            result.TotalPages = p.Get<int?>("_TotalPages") ?? 0;
            result.RowCount = p.Get<int?>("_RowCount") ?? 0;
            return result;
        }

        public Task<HotEventDetailDto> GetHotEventDetailAsync(int id)
        {
            var p = new DynamicParameters();
            p.Add("_SQ", id);
            return _reportDbClient.QueryFirstAsync<HotEventDetailDto>("Admin_HotEventDetail_Get", p);
        }

        public Task CreateHotEventAsync(CreateHotEventRequest request, string createUser)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", request.SiteId);
            p.Add("_TpId", request.TpId);
            p.Add("_GameId", request.GameId);
            p.Add("_SportLeaguesTitleId", request.SportLeaguesTitleId);
            p.Add("_MatchId", request.MatchId);
            p.Add("_StartTime", request.StartTime.ToUniversalTime().DateTime);
            p.Add("_EndTime", request.EndTime.ToUniversalTime().DateTime);
            p.Add("_BattleDateTime", request.BattleTime.ToUniversalTime().DateTime);
            p.Add("_HomeSportLeaguesId", request.HomeSportLeaguesId);
            p.Add("_AwaySportLeaguesId", request.AwaySportLeaguesId);
            p.Add("_HomeSportTeamId", request.HomeTeamNameId);
            p.Add("_AwaySportTeamId", request.AwayTeamNameId);
            p.Add("_CreateUser", createUser);
            p.Add("_IsLiveStreaming", request.IsLiveStreaming);
            return _mainDbClient.ExecuteAsync("Admin_HotEventInfo_Ins_V2", p);
        }

        public Task SetHotEventDataAsync(SetHotEventDataRequest request, string updateUser)
        {
            var p = new DynamicParameters();
            p.Add("_SQ", request.Id);
            p.Add("_SportLeaguesTitleId", request.SportLeaguesTitleId);
            p.Add("_MatchId", request.MatchId);
            p.Add("_StartTime", request.StartTime.ToUniversalTime().DateTime);
            p.Add("_EndTime", request.EndTime.ToUniversalTime().DateTime);
            p.Add("_BattleDateTime", request.BattleTime.ToUniversalTime().DateTime);
            p.Add("_HomeSportLeaguesId", request.HomeSportLeaguesId);
            p.Add("_AwaySportLeaguesId", request.AwaySportLeaguesId);
            p.Add("_HomeSportTeamId", request.HomeSportTeamId);
            p.Add("_AwaySportTeamId", request.AwaySportTeamId);
            p.Add("_UpdateUser", updateUser);
            return _mainDbClient.ExecuteAsync("Admin_HotEventData_Set", p);
        }

        public Task SetHotEventStatusAsync(SetHotEventStatusRequest request, string updateUser)
        {
            var p = new DynamicParameters();
            p.Add("_SQ", request.Id);
            p.Add("_Status", (int)request.Status);
            p.Add("_UpdateUser", updateUser);
            return _mainDbClient.ExecuteAsync("Admin_HotEventStatus_Set", p);
        }

        public Task DeleteHotEventAsync(int id)
        {
            var p = new DynamicParameters();
            p.Add("_SQ", id);
            return _mainDbClient.ExecuteAsync("Admin_HotEvent_Del", p);
        }
    }
}






