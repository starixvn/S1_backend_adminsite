using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Dapper;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class UserBetLimitService
    {
        private readonly MySqlClient _sqlClient;

        public UserBetLimitService(MySqlClient sqlClient)
        {
            _sqlClient = sqlClient;
        }

        public async Task<IEnumerable<TpCategoryData>> GetTpCategoryList()
        {
            return await _sqlClient.QueryAsync<TpCategoryData>("Admin_TPCategory_Get", null);
        }

        public async Task<int> InsertOrUpdate(int custId, int siteId, List<BetSetting> betSetting, string updateOperator)
        {
            var p = new DynamicParameters();

            p.Add("_CustId", custId);
            p.Add("_SiteId", siteId);
            p.Add("_BetSetting", JsonConvert.SerializeObject(betSetting));
            p.Add("_Operator", updateOperator);

            return await _sqlClient.ExecuteAsync("Admin_CustBetLimit_Set", p);
        }
    }
}