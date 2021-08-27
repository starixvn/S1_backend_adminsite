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

        public async Task<int> TpDisableUserSet(DisableUser tpDisableUser)
        {
            var p = new DynamicParameters();

            p.Add("_CustId", tpDisableUser.CustId);
            p.Add("_SiteId", tpDisableUser.SiteId);
            p.Add("_GameInfo", JsonConvert.SerializeObject(tpDisableUser.DisableTpGame));
            p.Add("_UpdateOperator", tpDisableUser.UpdateOperator);

            return await _sqlClient.ExecuteAsync("Admin_TpDisableUser_Set", p);
        }
    }
}