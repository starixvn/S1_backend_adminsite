using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Data;

namespace BackendSite.Service.DAL
{
    public class AdminGroupService
    {
        private readonly IConfiguration configuration;
        private readonly MySqlClient _reportDbClient;
        private readonly MySqlClient _mainDbClient;

        public AdminGroupService(IConfiguration configuration, IServiceProvider provider)
        {
            this.configuration = configuration;
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
        }

        public IEnumerable<AdminGroup> GetGroupList(int startIndex, int pageSize,out int totalPages,out int rowCount)
        {
            var para = new DynamicParameters();
            para.Add("_StartIndex", startIndex);
            para.Add("_PageSize", pageSize);
            para.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            para.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<AdminGroup>("Admin_Group_Get_V2", para);

            totalPages = para.Get<int?>("_TotalPages") ?? 0;
            rowCount = para.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public IEnumerable<AdminGroup> GetGroupByIdOrAll(int groupId,bool isAll)
        {
            var para = new DynamicParameters();
            para.Add("_GroupId", groupId);
            para.Add("_SelectAll", isAll);

            return _reportDbClient.Query<AdminGroup>("Admin_Group_GetByIdOrAll", para);
        }

        public IEnumerable<GroupPermisison> GetGroupPermissions(int SEQ)
        {
            var para = new DynamicParameters();
            para.Add("SEQ", SEQ);
            return _reportDbClient.Query<GroupPermisison>("Admin_GroupFunc_Get", para);
        }

        public void AddGroup(GroupWithPermission GroupInfo)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            var SpName = "Admin_Group_Ins";
            var _UserGroup = new DynamicParameters();
            _UserGroup.Add("@SEQ", dbType: DbType.Int32, direction: ParameterDirection.Output);
            _UserGroup.Add("@GroupName", GroupInfo.GroupName);
            _UserGroup.Add("@Remark", GroupInfo.Remark);

            _mainDbClient.Execute(SpName, _UserGroup, conn, tran);

            var SEQ = _UserGroup.Get<int?>("@SEQ");
            if (SEQ != null && GroupInfo.Permission != null)
            {
                SpName = "Admin_GroupFunc_Ins";
                foreach (var data in GroupInfo.Permission)
                {
                    var Json_SubFunctionKeys = JsonConvert.SerializeObject(data.SubFuncKeys);
                    var SubFunctionKeys = Json_SubFunctionKeys == "null" ? null : Json_SubFunctionKeys;
                    var para = new DynamicParameters();
                    para.Add("GroupSEQ", SEQ);
                    para.Add("FuncKey", data.FuncKey);
                    para.Add("SubFuncKeys", SubFunctionKeys);
                    _mainDbClient.Execute(SpName, para, conn ,tran);
                }
            }

            tran.Commit();
        }

        public void DeleteGroup(int GroupSEQ)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            var para = new DynamicParameters();
            para.Add("GroupSEQ", GroupSEQ);
            var SpName = "Admin_GroupFunc_Del";
            _mainDbClient.Execute(SpName, para, conn, tran);

            SpName = "Admin_Group_Del";
            _mainDbClient.Execute(SpName, para, conn, tran);

            tran.Commit();
        }

        public void UpdateGroup(GroupWithPermission GroupInfo)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            var SpName = "Admin_Group_Set";
            var para = new DynamicParameters();
            para.Add("GroupSEQ", GroupInfo.GroupSEQ);
            para.Add("GroupName", GroupInfo.GroupName);
            para.Add("Remark", GroupInfo.Remark);
            _mainDbClient.Execute(SpName, para, conn, tran);

            SpName = "Admin_GroupFunc_Del";
            var para2 = new DynamicParameters();
            para2.Add("GroupSEQ", GroupInfo.GroupSEQ);
            _mainDbClient.Execute(SpName, para2 , conn, tran);

            if (GroupInfo.Permission != null)
            {
                SpName = "Admin_GroupFunc_Ins";
                foreach (var data in GroupInfo.Permission)
                {
                    var Json_SubFunctionKeys = JsonConvert.SerializeObject(data.SubFuncKeys);
                    var SubFunctionKeys = Json_SubFunctionKeys == "null" ? null : Json_SubFunctionKeys;
                    var para3 = new DynamicParameters();
                    para3.Add("GroupSEQ", GroupInfo.GroupSEQ);
                    para3.Add("FuncKey", data.FuncKey);
                    para3.Add("SubFuncKeys", SubFunctionKeys);
                    _mainDbClient.Execute(SpName, para3, conn, tran);
                }
            }

            tran.Commit();
        }
    }
}
