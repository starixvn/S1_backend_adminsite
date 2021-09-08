using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class FireBaseService
    {
        private readonly IConfiguration configuration;
        private readonly CourierAdapter courierAdapter;
        private readonly string commonServiceUrl;

        public FireBaseService(IConfiguration configuration, CourierAdapter courierAdapter)
        {
            this.configuration = configuration;
            this.courierAdapter = courierAdapter;
            commonServiceUrl = configuration.GetValue<string>("CommonService");
        }
        public IEnumerable<FireBaseMessageInfo> GetFireBaseMessageList(int fireBaseId)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport")))
            {
             
                return conn.Query<FireBaseMessageInfo>("Admin_FirebaseData_Get", new { FirebaseId = fireBaseId }, commandType: CommandType.StoredProcedure);
            }
        }
        public IEnumerable<FireBaseCustInfo> GetFireBaseCustList(int siteId, int custId )
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport")))
            {

                return conn.Query<FireBaseCustInfo>("Admin_FirebaseCustList_Get",new { SiteId = siteId, CustId = custId }, commandType: CommandType.StoredProcedure);
            }
        }
        public async Task<ApiResult<IEnumerable<FireBaseReturn>>> SendFireBaseNotificationAsync(IEnumerable<FireBaseMessage> fireBaseMessage)
        {
            var apiUrl = $"{commonServiceUrl}FireBase/SendFireBaseNotification";
            dynamic req = new
            {
                FireBaseMessages = fireBaseMessage
            };
            var apiResult = await courierAdapter.MyPostAsync < IEnumerable < FireBaseReturn >, dynamic>(apiUrl, req);

            return apiResult;
        }
        public bool UpdateFireBaseSendStatus(int firebaseId, int status)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_FirebaseUpdateStatus_Set", new { FirebaseId = firebaseId, Status = status }, commandType: CommandType.StoredProcedure) > 0;
        }
    }
}
