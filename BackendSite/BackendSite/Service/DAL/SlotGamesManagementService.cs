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

        public List<SlotGameList> GetSlotGameList(int siteId, int tpId, string name, SlotGameFilterStatusEnum status, int pageNumber, int pageSize,out int totalPages,out int rowCount)
        {
            var parameter = new DynamicParameters();
            parameter.Add("SiteId", siteId, DbType.Int32);
            parameter.Add("TpId", tpId, DbType.Int32);
            parameter.Add("Status", (int)status, DbType.Int32);
            parameter.Add("Names", string.IsNullOrWhiteSpace(name) ? null : name);
            parameter.Add("StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), dbType: DbType.Int32);
            parameter.Add("PageSize", Math.Abs(pageSize), DbType.Int32);
            parameter.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameter.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<SlotGameList>("Admin_SlotGame_Get", parameter).ToList();
            totalPages = parameter.Get<int?>("TotalPages") ?? 0;
            rowCount = parameter.Get<int?>("RowCount") ?? 0;
            return result;
        }

        public int SetSlotGame(int siteId, int currencyId,int slotGameId, SlotGameStatusEnum status, string Operator)
        {
            var parameter = new DynamicParameters();
            parameter.Add("SiteId", siteId, DbType.Int32);
            parameter.Add("CurrencyId", currencyId, DbType.Int32);
            parameter.Add("SlotGameId", slotGameId, DbType.Int32);
            parameter.Add("Status", (int)status, DbType.Int32);
            parameter.Add("Operator", Operator);

            return _mainDbClient.Execute("Admin_SlotGameStatus_Set", parameter);
        }

        public List<SlotGameCategoryList> GetSlotGameCategoryList(int siteId, int tpId, string name, SlotGameCategoryEnum category, int pageNumber, int pageSize, out int totalPages, out int rowCount)
        {
            var parameter = new DynamicParameters();
            parameter.Add("SiteId", siteId, DbType.Int32);
            parameter.Add("TpId", tpId, DbType.Int32);
            parameter.Add("CategoryId", (int)category, DbType.Int32);
            parameter.Add("Names", string.IsNullOrWhiteSpace(name) ? null : name);
            parameter.Add("StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), dbType: DbType.Int32);
            parameter.Add("PageSize", Math.Abs(pageSize), DbType.Int32);
            parameter.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameter.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<SlotGameCategoryList>("Admin_SlotGameCategory_Get", parameter).ToList();
            totalPages = parameter.Get<int?>("TotalPages") ?? 0;
            rowCount = parameter.Get<int?>("RowCount") ?? 0;
            return result;
        }
        public int SetSlotGameCategory(int siteId, int currencyId, int slotGameId, int categoryId, SlotGameDetailStatusEnum status,string Operator)
        {
            var parameter = new DynamicParameters();
            parameter.Add("SiteId", siteId, DbType.Int32);
            parameter.Add("CurrencyId", currencyId, DbType.Int32);
            parameter.Add("SlotGameId", slotGameId, DbType.Int32);
            parameter.Add("CategoryId", categoryId, DbType.Int32);
            parameter.Add("Status", (int)status, DbType.Int32);
            parameter.Add("Operator", Operator);

            return _mainDbClient.Execute("Admin_SlotGameDetail_Set", parameter);
        }
    }
}
