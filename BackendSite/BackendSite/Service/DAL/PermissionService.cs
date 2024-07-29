using System.Data;

using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;

using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class PermissionService
    {
        private readonly IWebHostEnvironment env;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        private readonly Tools tools;

        public PermissionService(IWebHostEnvironment env, IConfiguration configuration, IServiceProvider provider, Tools tools)
        {
            this.env = env;
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
            this.tools = tools;
        }

        public string GetPermissionJSONData()
        {
            var contentRootPath = env.ContentRootPath;
            var initPermissionData = File.ReadAllText($"{contentRootPath}/App_Data/FunctionPermission.json");

            return initPermissionData;
        }

        public IEnumerable<RolePermission> GetRolePermission(int role)
        {
            return _reportDbClient.Query<RolePermission>("Admin_RolePermission_Get", new DynamicParameters(new { Role = role }));
        }
        public IEnumerable<UserPermissionData> GetUserPermissionData(int userId, int siteId)
        {
            return _reportDbClient.Query<UserPermissionData>("Admin_UserPermission_Get", new DynamicParameters(new { UserId = userId, SiteId = siteId }));
        }

        public IEnumerable<AdminRole> GetAdminRole()
        {
            return _reportDbClient.Query<AdminRole>("Admin_AdminRole_Get");
        }
        public int AddAdminRole(int siteId, string roleName, string remark, IEnumerable<int> currencyList, List<SetPermission> permission, int userId, string Operator)
        {
            var result = 1;
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();

            props.Add("RoleName", roleName);
            props.Add("Remark", remark);
            props.Add("CurrencyList", JsonConvert.SerializeObject(currencyList));
            props.Add("UserId", userId);
            props.Add("SiteId", siteId);
            props.Add("Operator", Operator);
            props.Add("SEQ", dbType: DbType.Int32, direction: ParameterDirection.Output);

            result = _mainDbClient.Execute("Admin_AdminRole_Ins_V2", trans, props);

            if (result <= 0)
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                return result;
            }
            var SEQ = props.Get<int?>("@SEQ");

            if (SEQ == null)
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                result = 0;
                return result;
            }
            foreach (var data in permission)
            {
                _mainDbClient.Execute("Admin_RolePermission_Ins", trans, new DynamicParameters(new { Role = SEQ, MenuId = data.DisPlayKey, Permission = JsonConvert.SerializeObject(data.PermissionIdList) }));
            }
            _mainDbClient.CommitAndDisposeConnection(conn, trans);
            return result;
        }

        public int UpdateAdminRole(int role, string roleName, string remark, IEnumerable<int> currencyList, List<SetPermission> permission, string Operator)
        {
            var result = 1;
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            props.Add("Id", role);
            props.Add("RoleName", roleName);
            props.Add("Remark", remark);
            props.Add("CurrencyList", JsonConvert.SerializeObject(currencyList));
            props.Add("Operator", Operator);

            result = _mainDbClient.Execute("Admin_AdminRole_Set_V2", trans, props);

            if (result <= 0)
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                return result;
            }

            _mainDbClient.Execute("Admin_RolePermission_Del", trans, new DynamicParameters(new { Role = role }));

            foreach (var data in permission)
            {
                _mainDbClient.Execute("Admin_RolePermission_Ins", trans, new DynamicParameters(new { Role = role, MenuId = data.DisPlayKey, Permission = JsonConvert.SerializeObject(data.PermissionIdList) }));
            }
            _mainDbClient.CommitAndDisposeConnection(conn, trans);
            return result;
        }

        public void DeleteAdminRole(int role)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            _mainDbClient.Execute("Admin_AdminRole_Del", trans, new DynamicParameters(new { Id = role }));
            _mainDbClient.Execute("Admin_RolePermission_Del", trans, new DynamicParameters(new { Role = role }));
            _mainDbClient.CommitAndDisposeConnection(conn, trans);
        }

        public IEnumerable<RoleList> GetRoleList(string roleName, int siteId)
        {
            return _reportDbClient.Query<RoleList>("Admin_RoleList_Get_V2",
                new DynamicParameters(new { RoleName = string.IsNullOrWhiteSpace(roleName) ? null : roleName, SiteId = siteId }));
        }
        public IEnumerable<UserList> GetAdminUserList(string userName, int siteId, bool status)
        {
            return _reportDbClient.Query<UserList>("Admin_UserList_Get_V3",
                new DynamicParameters(new { UserName = string.IsNullOrWhiteSpace(userName) ? null : userName, SiteId = siteId, Status = status }));
        }

        public int AddAdminUser(int siteId, int role, string userName, string password, IEnumerable<int> currencyList, List<SetPermission> permission, string Operator)
        {
            var result = 1;
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            List<int> roleList = new List<int>();
            roleList.Add(role);
            props.Add("UserName", userName);
            props.Add("Userpwd", tools.MD5Encrypt(password));
            props.Add("Operator", Operator);
            props.Add("SEQ", dbType: DbType.Int32, direction: ParameterDirection.Output);

            result = _mainDbClient.Execute("Admin_AdminUser_Ins_V2", trans, props);

            if (result <= 0)
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                return result;
            }
            var SEQ = props.Get<int?>("@SEQ");

            if (SEQ == null)
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                result = 0;
                return result;
            }

            result = _mainDbClient.Execute("Admin_AdminBrand_Ins", trans, new DynamicParameters
                (new { UserId = SEQ, SiteId = siteId, CurrencyList = JsonConvert.SerializeObject(currencyList), Role = JsonConvert.SerializeObject(roleList) }));

            if (result <= 0)
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                return result;
            }

            foreach (var data in permission)
            {
                _mainDbClient.Execute("Admin_UserPermission_Ins", trans, new DynamicParameters(new { UserId = SEQ, SiteId = siteId, MenuId = data.DisPlayKey, Permission = JsonConvert.SerializeObject(data.PermissionIdList) }));
            }
            _mainDbClient.CommitAndDisposeConnection(conn, trans);
            return result;
        }

        public int UpdateAdminUser(int siteId, int userId, int role, IEnumerable<int> currencyList, List<SetPermission> permission, string Operator)
        {
            var result = 1;
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            List<int> roleList = new List<int>();
            roleList.Add(role);

            result = _mainDbClient.Execute("Admin_AdminBrandRole_Set", trans, new DynamicParameters
                (new { UserId = userId, SiteId = siteId, CurrencyList = JsonConvert.SerializeObject(currencyList), Role = JsonConvert.SerializeObject(roleList) }));

            if (result <= 0)
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                return result;
            }

            result = _mainDbClient.Execute("Admin_AdminUser_Set", trans, new DynamicParameters(new { UserId = userId, Operator = Operator }));

            if (result <= 0)
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                return result;
            }

            _mainDbClient.Execute("Admin_UserPermission_Del", trans, new DynamicParameters(new { UserId = userId, SiteId = siteId }));

            foreach (var data in permission)
            {
                _mainDbClient.Execute("Admin_UserPermission_Ins", trans, new DynamicParameters(new { UserId = userId, SiteId = siteId, MenuId = data.DisPlayKey, Permission = JsonConvert.SerializeObject(data.PermissionIdList) }));
            }
            _mainDbClient.CommitAndDisposeConnection(conn, trans);
            return result;
        }
        public bool SetAdminUserPassword(ParamsWithLog<SetAdminUserPasswordReq> input)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            var propsLog = new DynamicParameters();

            props.Add("UserId", input.After.UserId);
            props.Add("Userpwd", tools.MD5Encrypt(input.After.Password));
            props.Add("Operator", input.Operator);

            #region propsLog
            propsLog.Add("_SiteId", input.SiteId);
            propsLog.Add("_CurrencyId", input.CurrencyId);
            propsLog.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.SetAdminUserPassword);
            propsLog.Add("_ActionType", (int)ActionType.Update);
            propsLog.Add("_RefTransId", string.Empty);
            propsLog.Add("_BeforeDetailInfo", input.After.ToLog());
            propsLog.Add("_AfterDetailInfo", input.After.ToLog());
            propsLog.Add("_Operator", input.Operator);
            #endregion

            var result = _mainDbClient.Execute("Admin_UserPassword_Set_V2", trans, props) > 0;
            var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, propsLog) > 0;
            if (logResult & result)
            {
                _mainDbClient.CommitAndDisposeConnection(conn, trans);
            }
            else
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
            }
            return logResult & result;
        }
        public bool SetAdminUserStatus(ParamsWithLog<SetAdminUserStatusReq> input)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            var propsLog = new DynamicParameters();

            #region
            props.Add("UserId", input.After.UserId);
            props.Add("Status", (int)input.After.Status);
            props.Add("Operator", input.Operator);
            #endregion

            #region propsLog
            propsLog.Add("_SiteId", input.SiteId);
            propsLog.Add("_CurrencyId", input.CurrencyId);
            propsLog.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.SetAdminUserStatus);
            propsLog.Add("_ActionType", (int)ActionType.Update);
            propsLog.Add("_RefTransId", string.Empty);
            propsLog.Add("_BeforeDetailInfo", input.Before.ToLog());
            propsLog.Add("_AfterDetailInfo", input.After.ToLog());
            propsLog.Add("_Operator", input.Operator);
            #endregion

            var r = _mainDbClient.Execute("Admin_UserStatus_Set_V2", trans, props) > 0;
            var result = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, propsLog) > 0;
            if (r & result)
            {
                _mainDbClient.CommitAndDisposeConnection(conn, trans);
            }
            else
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
            }
            return r & result;
        }

        public IEnumerable<AdminBrandRole> GetAdminBrandRole(int role)
        {
            return _reportDbClient.Query<AdminBrandRole>("Admin_AdminBrandRole_Get",
                new DynamicParameters(new { Role = role }));
        }

        public IEnumerable<AdminBrand> GetAssignBrand(IEnumerable<int> selectSite, string userName)
        {
            return _reportDbClient.Query<AdminBrand>("Admin_AdminBrand_Get",
                new DynamicParameters(
                    new { SelectSite = JsonConvert.SerializeObject(selectSite), UserName = string.IsNullOrWhiteSpace(userName) ? null : userName }));
        }

        public IEnumerable<UserBrand> GetUserBrand(int userId)
        {
            return _reportDbClient.Query<UserBrand>("Admin_UserBrand_Get",
                new DynamicParameters(
                    new { UserId = userId }));
        }

        public bool SetAdminBrand(SetAdminBrandReq input, string userName, List<UserBrand> beforeUserBrands, List<UserBrand> afterUserBrands)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            var propsLog = new DynamicParameters();

            props.Add("UserId", input.UserId);
            props.Add("SiteId", input.SiteId);
            props.Add("Status", (int)input.Status);

            #region propsLog
            propsLog.Add("UserId", input.UserId);
            propsLog.Add("BeforeDetailInfo", JsonConvert.SerializeObject(beforeUserBrands));
            propsLog.Add("AfterDetailInfo", JsonConvert.SerializeObject(afterUserBrands));
            propsLog.Add("Operator", userName);
            #endregion

            var result = _mainDbClient.Execute("Admin_AdminBrand_Set", trans, props) > 0;
            var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentBrandLog_Ins", trans, propsLog) > 0;
            if (logResult & result)
            {
                _mainDbClient.CommitAndDisposeConnection(conn, trans);
            }
            else
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
            }
            return logResult & result;
        }
    }
}
