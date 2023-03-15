using BackendSite.Service.Model;
using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.DAL
{
    public class AdminUserService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public AdminUserService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }
        public AdminUser GetAdminUserByUserName(string userName)
        {
            var para = new DynamicParameters();
            para.Add("UserName", userName);
            return _reportDbClient.QueryFirst<AdminUser>("LoginAdminUser_Get", para);
        }

        public IEnumerable<AdminUser> GetUserList(int startIndex, int pageSize, out int totalPages, out int rowCount)
        {
            var para = new DynamicParameters();
            para.Add("_StartIndex", startIndex);
            para.Add("_PageSize", pageSize);
            para.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            para.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<AdminUser>("Admin_User_GetAll_V2", para);

            totalPages = para.Get<int?>("_TotalPages") ?? 0;
            rowCount = para.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public IEnumerable<AssignSites> GetAssignSiteList(int userId)
        {
            return _mainDbClient.Query<AssignSites>("Admin_AuthorSite_Get", new DynamicParameters(new { UserId = userId }));
        }

        public void AssignSiteList(int UserId, IEnumerable<int> SiteIds)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            var para = new DynamicParameters();
            para.Add("UserId", UserId);
            var SpName = "Admin_AuthorSite_Del";
            _mainDbClient.Execute(SpName, tran, para);

            if (SiteIds != null && SiteIds.Any())
            {
                SpName = "Admin_AuthorSite_Ins";
                foreach (var id in SiteIds)
                {
                    var para2 = new DynamicParameters();
                    para2.Add("UserId", UserId);
                    para2.Add("SiteId", id);
                    _mainDbClient.Execute(SpName, tran, para2);
                }
            }

            tran.Commit();
        }

        public void AssignGroup(int UserId, int? GroupSEQ)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            var para = new DynamicParameters();
            para.Add("UserId", UserId);
            if (GroupSEQ == null)
            {
                var SpName = "Admin_AuthorGroup_Del";
                _mainDbClient.Execute(SpName, tran, para);
            }
            else
            {
                para.Add("GroupSEQ", GroupSEQ);
                var SpName = "Admin_AuthorGroup_Set";
                _mainDbClient.Execute(SpName, tran, para);
            }

            tran.Commit();
        }

        public void SetUserPassword(int UserId, string MD5Pwd)
        {
            var para = new DynamicParameters();
            para.Add("UserId", UserId);
            para.Add("UserPwd", MD5Pwd);
            _mainDbClient.Execute("Admin_User_SetPwd", para);
        }

        public void SetUserStatus(int UserId, int Status)
        {
            var para = new DynamicParameters();
            para.Add("UserId", UserId);
            para.Add("UserStatus", Status);
            _mainDbClient.Execute("Admin_User_SetStatus", para);
        }

        public IEnumerable<AdminUser> AddUser(string UserName, string MD5Pwd, IEnumerable<int> SiteList, int? GroupSEQ)
        {
            IEnumerable<AdminUser> userList = null;
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            var UserInfo = new DynamicParameters();
            UserInfo.Add("@UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            UserInfo.Add("@UserName", UserName);
            UserInfo.Add("@Userpwd", MD5Pwd);
            UserInfo.Add("@IsSuper", 0);
            UserInfo.Add("@Status", 1);

            _mainDbClient.Execute("Admin_User_Ins", tran, UserInfo);

            var UserId = UserInfo.Get<int?>("@UserId");

            if (UserId != null)
            {
                if (GroupSEQ != null)
                {
                    var para = new DynamicParameters();
                    para.Add("UserId", UserId);
                    para.Add("GroupSEQ", GroupSEQ);
                    var SpName = "Admin_AuthorGroup_Set";
                    _mainDbClient.Execute(SpName, tran, para);
                }

                if (SiteList != null)
                {
                    foreach (var SiteId in SiteList)
                    {
                        var para2 = new DynamicParameters();
                        para2.Add("UserId", UserId);
                        para2.Add("SiteId", SiteId);
                        _mainDbClient.Execute("Admin_AuthorSite_Ins", tran, para2);
                    }
                }

                userList = _reportDbClient.Query<AdminUser>("Admin_User_GetAll");
            }

            tran.Commit();

            return userList;
        }

        public AdminUser GetUser(int userId)
        {
            return _mainDbClient.QueryFirst<AdminUser>("Admin_User_Get", new DynamicParameters(new { UserId = userId }));
        }
    }
}
