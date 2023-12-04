using System.Data;

using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;

using MySql.Data.MySqlClient;

using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class HotGameService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        private MySqlConnection _connection;
        private MySqlTransaction _transaction;

        public HotGameService(IEnumerable<MySqlClient> dbClient)
        {
            var mySqlClients = dbClient.ToList();
            _mainDbClient = mySqlClients.FirstOrDefault(a => a.DataBase == DataBase.Main);
            _reportDbClient = mySqlClients.FirstOrDefault(a => a.DataBase == DataBase.Report);
        }

        public async Task<IEnumerable<GetHotGameListDto>> GetDataList(int siteId, int currencyId)
        {
            var p = new DynamicParameters(new { _SiteId = siteId, _CurrencyId = currencyId });
            return await _reportDbClient.QueryAsync<GetHotGameListDto>("Admin_HotGames_Get", p);
        }

        public async Task<GetHotGameDetailDto> GetDetail(int id)
        {
            var p = new DynamicParameters(new { _id = id });
            return await _reportDbClient.QueryFirstAsync<GetHotGameDetailDto>("Admin_HotGamesById_Get", p);
        }

        public async Task<int> CreateOrUpdate(SetHotGameRequest request, string updateOperator)
        {
            var p = new DynamicParameters();
            p.Add("_Id", request.Id);
            p.Add("_SiteId", request.SiteId);
            p.Add("_CurrencyId", request.CurrencyId);
            p.Add("_LangId", request.LangId);
            p.Add("_TpId", request.TpId);
            p.Add("_GameId", request.GameId);
            p.Add("_TpGameType", request.TpGameType);
            p.Add("_TpCategory", request.TpCategory);
            p.Add("_GameName", request.GameName);
            p.Add("_GameLogo", JsonConvert.SerializeObject(request.GameLogo));
            p.Add("_TpName", request.TpName);
            p.Add("_TpLogo", request.TpLogo);
            p.Add("_Operator", updateOperator);
            return await _mainDbClient.ExecuteAsync("Admin_HotGames_Set", p);
        }

        public async Task<int> Delete(int id)
        {
            var p = new DynamicParameters();
            p.Add("_Id", id);
            return await _mainDbClient.ExecuteAsync("Admin_HotGames_Del", p);
        }

        public async Task<int> Sorting(int id, int sort)
        {
            var p = new DynamicParameters();
            p.Add("_Id", id);
            p.Add("_Sort", sort);
            return await _mainDbClient.ExecuteAsync("Admin_HotGamesSort_Set", _transaction, p);
        }

        public async Task BeginTransaction()
        {
            _connection = _mainDbClient.GetMySqlConnection();
            await _connection.OpenAsync();
            _transaction = await _connection.BeginTransactionAsync();
        }

        public async Task Commit()
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }

        public async Task RollBack()
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }

        #region ApiV2

        public async Task<GetHotGameListDtoV2Wrapper> GetDataListV2(GetHotGameDataListRequestV2 request)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", request.SiteId);
            p.Add("_CurrencyId", request.CurrencyId);
            p.Add("_TopCategory", (int)request.TopCategory);
            p.Add("_TpId", request.TpId);
            p.Add("_TpGameType", request.TpGameType);
            p.Add("_PageSize", Math.Abs(request.PageSize));
            p.Add("_StartIndex", (Math.Abs(request.PageNumber) - 1) * Math.Abs(request.PageSize));
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = new GetHotGameListDtoV2Wrapper
            {
                Data = await _reportDbClient.QueryAsync<GetHotGameListDtoV2>("Admin_HotGames_Get_V3", p),
                TotalPages = p.Get<int?>("_TotalPages") ?? 0,
                RowCount = p.Get<int?>("_RowCount") ?? 0
            };

            return result;
        }

        public async Task<GetHotGameDetailDtoV2> GetDetailV2(int id, int siteId)
        {
            var p = new DynamicParameters(new { _id = id, _siteId = siteId });
            return await _reportDbClient.QueryFirstAsync<GetHotGameDetailDtoV2>("Admin_HotGamesById_Get_V3", p);
        }

        public async Task<int> CreateOrUpdateV2(SetHotGameRequestV2 request, string updateOperator)
        {
            var p = new DynamicParameters();
            p.Add("_Id", request.Id);
            p.Add("_SiteId", request.SiteId);
            p.Add("_CurrencyId", request.CurrencyId);
            p.Add("_LangId", request.LangId);
            p.Add("_TpId", request.TpId);
            p.Add("_GameId", request.GameId);
            p.Add("_TpGameType", request.TpGameType);
            p.Add("_TpCategory", request.TpCategory);
            p.Add("_GameName", request.GameName);
            p.Add("_TpName", request.TpName);
            p.Add("_TopCategory", request.TopCategory);
            p.Add("_Operator", updateOperator);
            return await _mainDbClient.ExecuteAsync("Admin_HotGames_Set_V3", p);
        }

        public async Task<int> DeleteV2(int id, int siteId)
        {
            var p = new DynamicParameters();
            p.Add("_Id", id);
            p.Add("_SiteId", siteId);
            return await _mainDbClient.ExecuteAsync("Admin_HotGames_Del_V2", p);
        }

        public async Task<int> SortingV2(int id, int sort, int siteId)
        {
            var p = new DynamicParameters();
            p.Add("_Id", id);
            p.Add("_Sort", sort);
            p.Add("_SiteId", siteId);
            return await _mainDbClient.ExecuteAsync("Admin_HotGamesSort_Set_V2", _transaction, p);
        }

        #endregion

    }
}