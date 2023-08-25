using System.Data;

using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;

namespace BackendSite.Service.DAL
{
    public class AdminService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public AdminService(IEnumerable<MySqlClient> sqlClients)
        {
            _mainDbClient = sqlClients.FirstOrDefault(p => p.DataBase == DataBase.Main);
            _reportDbClient = sqlClients.FirstOrDefault(p => p.DataBase == DataBase.Report);
        }

        public bool ChangePwd(int siteId, int userId, ChangePwd changePwd, out string Md5Userpwd)
        {
            Md5Userpwd = null;
            var p = new DynamicParameters(new { SiteId = siteId, UserId = userId, Md5OldPassword = changePwd.OldPwd, Md5NewPassword = changePwd.NewPwd });
            p.Add("Md5Userpwd", dbType: DbType.String, direction: ParameterDirection.Output);
            var result = _mainDbClient.Execute("AdminAccountChangeSelfPwd_Set", p);
            Md5Userpwd = p.Get<string>("Md5Userpwd");
            return result > 0;
        }

        public SpecialPasswordManagement GetSpecialPasswordData(int sitId)
        {
            return _reportDbClient.Query<SpecialPasswordManagement>("Admin_SpecialPassword_Get", new DynamicParameters(new { SiteId = sitId })).FirstOrDefault();
        }

        public int SetSpecialPasswordData(int SiteId, string Password, string Operator)
        {
            var props = new DynamicParameters();
            props.Add("SiteId", SiteId);
            props.Add("Password", Password);
            props.Add("Operator", Operator);

            return _mainDbClient.Execute("Admin_SpecialPassword_Set", props);
        }

        public IEnumerable<AdminMenuDto> GetAllAdminMenu()
        {
            return _reportDbClient.Query<AdminMenuDto>("Admin_AdminMenu_GetAll");
        }

        public IEnumerable<AdminPermissionDto> GetAllAdminPermission()
        {
            return _reportDbClient.Query<AdminPermissionDto>("Admin_AdminPermission_GetAll");
        }
    }
}
