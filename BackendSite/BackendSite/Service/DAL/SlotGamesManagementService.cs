using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class SlotGamesManagementService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public SlotGamesManagementService(IEnumerable<MySqlClient> dbClient)
        {
            _mainDbClient = dbClient.FirstOrDefault(a => a.DataBase == DataBase.Main);
            _reportDbClient = dbClient.FirstOrDefault(a => a.DataBase == DataBase.Report);
        }

        public List<SlotGameList> GetSlotGameList(int Site, int TPid, string Name,int Status, int PageNumber, int PageSize,out int TotalPages,out int RowCount)
        {
            var parameter = new DynamicParameters();
            parameter.Add("SiteId", Site, DbType.Int32);
            parameter.Add("TPId", TPid, DbType.Int32);
            parameter.Add("Status", Status, DbType.Int32);
            parameter.Add("Names", string.IsNullOrWhiteSpace(Name) ? null : Name);
            parameter.Add("PageNumber", Math.Abs(PageNumber), dbType: DbType.Int32);
            parameter.Add("StartIndex", (Math.Abs(PageNumber) - 1) * Math.Abs(PageSize), dbType: DbType.Int32);
            parameter.Add("PageSize", Math.Abs(PageSize), DbType.Int32);

            parameter.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameter.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<SlotGameList>("Admin_SlotGame_Get", parameter).ToList();
            TotalPages = parameter.Get<int?>("TotalPages") ?? 0;
            RowCount = parameter.Get<int?>("RowCount") ?? 0;
            return result;
        }

        public int SetSlotGame(int Site,int CurrencyId,int SlotGameId,string Operator)
        {
            var parameter = new DynamicParameters();
            parameter.Add("SiteId", Site, DbType.Int32);
            parameter.Add("CurrencyId", CurrencyId, DbType.Int32);
            parameter.Add("SlotGameId", SlotGameId, DbType.Int32);
            parameter.Add("Operator", Operator);

            return _mainDbClient.Execute("Admin_SlotGameStatus_Set", parameter);
        }
    }
}
