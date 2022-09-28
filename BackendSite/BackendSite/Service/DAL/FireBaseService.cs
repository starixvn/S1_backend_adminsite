using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using MySql.Data.MySqlClient;
using System.Data;

namespace BackendSite.Service.DAL
{
    public class FireBaseService
    {
        private readonly IConfiguration configuration;
        private readonly CourierAdapter courierAdapter;
        private readonly string commonServiceUrl;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public FireBaseService(IServiceProvider provider)
        {
            configuration = provider.GetRequiredService<IConfiguration>();
            courierAdapter = provider.GetRequiredService<CourierAdapter>();
            commonServiceUrl = configuration.GetValue<string>("CommonService");
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }
        public IEnumerable<FireBaseMessageInfo> GetFireBaseMessageList(int fireBaseId)
        {
            return _reportDbClient.Query<FireBaseMessageInfo>("Admin_FirebaseData_Get", new { FirebaseId = fireBaseId });
        }
        public IEnumerable<FireBaseCustInfo> GetFireBaseCustList(int siteId, int custId )
        {
            return _reportDbClient.Query<FireBaseCustInfo>("Admin_FirebaseCustList_Get",new { SiteId = siteId, CustId = custId });
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
            return _mainDbClient.Execute("Admin_FirebaseUpdateStatus_Set", new { FirebaseId = firebaseId, Status = status }) > 0;
        }
    }
}
