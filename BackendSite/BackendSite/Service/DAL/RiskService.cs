using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Dapper;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class RiskService
    {
        private readonly MySqlClient _sqlClient;

        public RiskService(MySqlClient sqlClient)
        {
            _sqlClient = sqlClient;
        }

        public async Task<int> TpDisableUserSet(int custId, int siteId, List<DisableTpGame> gameIndo, string updateOperator)
        {
            var p = new DynamicParameters();

            p.Add("_CustId", custId);
            p.Add("_SiteId", siteId);
            p.Add("_GameInfo", JsonConvert.SerializeObject(gameIndo));
            p.Add("_UpdateOperator", updateOperator);

            return await _sqlClient.ExecuteAsync("Admin_TpDisableUser_Set", p);
        }
    }
}