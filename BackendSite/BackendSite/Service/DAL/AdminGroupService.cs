using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BackendSite.Service.DAL
{
    public class AdminGroupService
    {
        private IConfiguration configuration;

        public AdminGroupService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<AdminGroup> GetGroupList(int startIndex, int pageSize,out int totalPages,out int rowCount)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var para = new DynamicParameters();
            para.Add("_StartIndex", startIndex);
            para.Add("_PageSize", pageSize);
            para.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            para.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = conn.Query<AdminGroup>("Admin_Group_Get_V2", para, commandType: CommandType.StoredProcedure);

            totalPages = para.Get<int?>("_TotalPages") ?? 0;
            rowCount = para.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public IEnumerable<AdminGroup> GetGroupByIdOrAll(int groupId,bool isAll)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var para = new DynamicParameters();
            para.Add("_GroupId", groupId);
            para.Add("_SelectAll", isAll);

            return conn.Query<AdminGroup>("Admin_Group_GetByIdOrAll", para, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<GroupPermisison> GetGroupPermissions(int SEQ)
        {
            string FuncKey = null;
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<GroupPermisison>("Admin_GroupFunc_Get", new { SEQ, FuncKey }, commandType: CommandType.StoredProcedure);
        }

        public GroupPermisison GetGroupPermissions(int SEQ, string FuncKey)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<GroupPermisison>("Admin_GroupFunc_Get", new { SEQ, FuncKey }, commandType: CommandType.StoredProcedure).FirstOrDefault();
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

            conn.Execute(SpName, _UserGroup, tran, commandType: CommandType.StoredProcedure);

            var SEQ = _UserGroup.Get<int?>("@SEQ");
            if (SEQ != null && GroupInfo.Permission != null)
            {
                SpName = "Admin_GroupFunc_Ins";
                foreach (var data in GroupInfo.Permission)
                {
                    var Json_SubFunctionKeys = JsonConvert.SerializeObject(data.SubFuncKeys);
                    var SubFunctionKeys = Json_SubFunctionKeys == "null" ? null : Json_SubFunctionKeys;
                    conn.Execute(SpName, new { GroupSEQ = SEQ, data.FuncKey, SubFuncKeys = SubFunctionKeys }, tran, commandType: CommandType.StoredProcedure);
                }
            }

            tran.Commit();
        }

        public void DeleteGroup(int GroupSEQ)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            var SpName = "Admin_GroupFunc_Del";
            conn.Execute(SpName, new { GroupSEQ }, tran, commandType: CommandType.StoredProcedure);

            SpName = "Admin_Group_Del";
            conn.Execute(SpName, new { GroupSEQ }, tran, commandType: CommandType.StoredProcedure);

            tran.Commit();
        }

        public AdminGroup GetGroup(int GroupSEQ)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<AdminGroup>("Admin_Group_Get", new { SEQ = GroupSEQ, SelectAll = 0 }, commandType: CommandType.StoredProcedure);
        }

        public void UpdateGroup(GroupWithPermission GroupInfo)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            var SpName = "Admin_Group_Set";
            conn.Execute(SpName, new
            {
                GroupInfo.GroupSEQ,
                GroupInfo.GroupName,
                GroupInfo.Remark
            }, tran, commandType: CommandType.StoredProcedure);

            SpName = "Admin_GroupFunc_Del";
            conn.Execute(SpName, new { GroupInfo.GroupSEQ }, tran, commandType: CommandType.StoredProcedure);

            if (GroupInfo.Permission != null)
            {
                SpName = "Admin_GroupFunc_Ins";
                foreach (var data in GroupInfo.Permission)
                {
                    var Json_SubFunctionKeys = JsonConvert.SerializeObject(data.SubFuncKeys);
                    var SubFunctionKeys = Json_SubFunctionKeys == "null" ? null : Json_SubFunctionKeys;
                    conn.Execute(SpName, new { GroupInfo.GroupSEQ, data.FuncKey, SubFuncKeys = SubFunctionKeys }, tran, commandType: CommandType.StoredProcedure);
                }
            }

            tran.Commit();
        }
    }
}
