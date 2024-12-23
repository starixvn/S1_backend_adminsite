using System.Data;

using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class NewAdminUserService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        private readonly ILogger<NewAdminUserService> _logger;

        public NewAdminUserService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
            _logger = provider.GetRequiredService<ILogger<NewAdminUserService>>();
        }
        public NewAdminUser GetAdminUserByUserName(string userName)
        {
            var props = new DynamicParameters();
            props.Add("UserName", userName);
            return _reportDbClient.QueryFirst<NewAdminUser>("LoginNewAdminUser_Get", props);
        }

        public int SetLoginTime(int userId)
        {
            var props = new DynamicParameters();
            props.Add("UserId", userId);
            return _mainDbClient.Execute("Admin_LoginTime_Set", props);
        }

        public List<AssignBrand> GetAssignSiteListV2(int userId)
        {
            return _reportDbClient.Query<AssignBrand>("Admin_AuthorBrand_Get", new DynamicParameters(new { UserId = userId })).ToList();
        }

        public AdminUserV2 GetUserV2(int userId, int sitId)
        {
            return _mainDbClient.QueryFirst<AdminUserV2>("Admin_AdminUserBySite_Get", new DynamicParameters(new { UserId = userId, SiteId = sitId }));
        }
        public AdminUserData GetUserV2(int userId)
        {
            return _reportDbClient.QueryFirst<AdminUserData>("Admin_AdminUserData_Get", new DynamicParameters(new { UserId = userId }));
        }
        public bool ChangePwdV2(int userId, string oldPwd, string newPwd, out string Md5Userpwd)
        {
            Md5Userpwd = null;
            var p = new DynamicParameters(new { UserId = userId, Md5OldPassword = oldPwd, Md5NewPassword = newPwd });
            p.Add("Md5Userpwd", dbType: DbType.String, direction: ParameterDirection.Output);
            var result = _mainDbClient.Execute("Admin_AccountChangeSelfPwd_Set", p);
            Md5Userpwd = p.Get<string>("Md5Userpwd");
            return result > 0;
        }
        
        public IEnumerable<SpecialAdminUser> GetSpecialAdminUserList(int refId, AdminUserTypeEnum userType, int userId = 0)
        {
            return _reportDbClient.Query<SpecialAdminUser>("Admin_SpecialAdminUserList_Get",
                    new
                    { 
                        _RefId = refId,
                        _UserType = (int)userType,
                        _UserId = userId
                    });
        }
        
        public (bool Success, int UserId) CreateSpecialAdminUser(SetSpecialAdminUserParams input)
        {
            var conn = _mainDbClient.GetMySqlConnection();
            conn.OpenAsync();
            using var trans = conn.BeginTransaction();
            
            var p = new DynamicParameters(new
            { 
                _UserName = input.UserName,
                _Userpwd = input.Userpwd,
                _UserType = (int)input.UserType,
                _RefId = input.RefId,
                _SiteIds = JsonConvert.SerializeObject(input.SiteIds),
                _CurrencyIds = JsonConvert.SerializeObject(input.CurrencyIds),
                _GroupIds = JsonConvert.SerializeObject(input.GroupIds),
                _Operator = input.Operator
            });
            
            p.Add("_Result", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            _ = _mainDbClient.Execute("Admin_SpecialAdminUser_Ins", trans, p);
            
            var resultCode = p.Get<int>("_Result");
            var userId = p.Get<int>("_UserId");
            var success = resultCode == 0;
            
            if(success)
                trans.Commit();
            else
                _logger.LogError($"Admin_SpecialAdminUser_Ins failed, resultCode={resultCode}");
            
            return (success, userId);
        }
        
    }
}
