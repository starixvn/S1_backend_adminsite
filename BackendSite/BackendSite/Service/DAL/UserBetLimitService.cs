using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class UserBetLimitService
    {
        #region Fields

        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        #endregion

        #region Constructor

        public UserBetLimitService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        #endregion

        #region Methods

        public async Task<int> InsertOrUpdate(int custId, int siteId, List<BetSetting> betSetting, string updateOperator)
        {
            var p = new DynamicParameters();

            p.Add("_CustId", custId);
            p.Add("_SiteId", siteId);
            p.Add("_BetSetting", JsonConvert.SerializeObject(betSetting));
            p.Add("_Operator", updateOperator);

            return await _mainDbClient.ExecuteAsync("Admin_CustBetLimit_Set", p);
        }

        public IEnumerable<UserBetLimit> GetUserBetLimit(IEnumerable<int> siteIds, string userName, DateTimeOffset startDate, DateTimeOffset endDate, int pageSize, int pageNumber, out int totalPages, out int rowCount)
        {
            var p = new DynamicParameters();
            p.Add("_SiteIds", JsonConvert.SerializeObject(siteIds));
            p.Add("_UserName", userName);
            p.Add("_StartDate", startDate.DateTime.AddDays(1).AddSeconds(-1));
            p.Add("_EndDate", endDate.DateTime);
            p.Add("_PageSize", Math.Abs(pageSize));
            p.Add("_PageNumber", Math.Abs(pageNumber));
            p.Add("_StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize));
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
         
            var result = _reportDbClient.Query<UserBetLimit>("Admin_CustBetLimit_Get_V2", p);
            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;
            return result;
        }

        public async Task<int> Delete(int custId)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId);
            return await _mainDbClient.ExecuteAsync("Admin_CustBetLimit_Del", p);
        }

        public async Task<int> UpdateStatus(int custId, int status, string updateOperator)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId);
            p.Add("_Status", status);
            p.Add("_Operator", updateOperator);
            return await _mainDbClient.ExecuteAsync("Admin_CustBetLimitStatus_Set", p);
        }

        public async Task<CustBetLimitData> UserBetLimitGetByCustId(int custId)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId);
            return await _reportDbClient.QueryFirstAsync<CustBetLimitData>("Admin_CustBetLimitByCustId_Get", p);
        }

        public async Task<IEnumerable<TpGameType>> GetTpGameTypeListbyCategoryAsync(int categoryId)
        {
            var p = new DynamicParameters();
            p.Add("CategoryId", categoryId);
            return await _reportDbClient.QueryAsync<TpGameType>("Admin_TpGameTypebyCategory_Get", p);
        }

        public async Task<IEnumerable<TpCategoryData>> GetTpCategoryList()
        {
            return await _reportDbClient.QueryAsync<TpCategoryData>("Admin_TPCategory_Get", null);
        }

        #endregion

    }
}
