using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class AdminGroupService
    {
        private IConfiguration configuration;

        public AdminGroupService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<AdminGroup> GetGroupList()
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<AdminGroup>("Admin_Group_Get", new { SEQ = 0, SelectAll = 1 }, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<GroupPermisison> GetGroupPermissions(int SEQ)
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<GroupPermisison>("Admin_GroupFunc_Get", new { SEQ }, commandType: CommandType.StoredProcedure);
        }

        public void AddGroup(GroupWithPermission GroupInfo)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    string SpName = "Admin_Group_Ins";
                    DynamicParameters _UserGroup = new DynamicParameters();
                    _UserGroup.Add("@SEQ", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    _UserGroup.Add("@GroupName", GroupInfo.GroupName);
                    _UserGroup.Add("@Remark", GroupInfo.Remark);

                    conn.Execute(SpName, _UserGroup, tran, commandType: CommandType.StoredProcedure);

                    int? SEQ = _UserGroup.Get<int?>("@SEQ");
                    if (SEQ != null && GroupInfo.Permission != null)
                    {
                        SpName = "Admin_GroupFunc_Ins";
                        foreach (var data in GroupInfo.Permission)
                        {
                            string Json_SubFunctionKeys = JsonConvert.SerializeObject(data.SubFunctionKeys);
                            string SubFunctionKeys = Json_SubFunctionKeys == "null" ? null : Json_SubFunctionKeys;
                            conn.Execute(SpName, new { GroupSEQ = SEQ, FuncKey = data.FunctionKey, SubFuncKey = SubFunctionKeys }, tran, commandType: CommandType.StoredProcedure);
                        }
                    }

                    tran.Commit();
                }
            }
        }

        public void DeleteGroup(int GroupSEQ)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    string SpName = "Admin_GroupFunc_Del";
                    conn.Execute(SpName, new { GroupSEQ }, tran, commandType: CommandType.StoredProcedure);

                    SpName = "Admin_Group_Del";
                    conn.Execute(SpName, new { GroupSEQ }, tran, commandType: CommandType.StoredProcedure);

                    tran.Commit();
                }
            }
        }

        public AdminGroup GetGroup(int GroupSEQ)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.QueryFirstOrDefault<AdminGroup>("Admin_Group_Get", new { SEQ = GroupSEQ, SelectAll = 0 }, commandType: CommandType.StoredProcedure);
            }
        }

        public void UpdateGroup(GroupWithPermission GroupInfo)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    string SpName = "Admin_Group_Set";
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
                            string Json_SubFunctionKeys = JsonConvert.SerializeObject(data.SubFunctionKeys);
                            string SubFunctionKeys = Json_SubFunctionKeys == "null" ? null : Json_SubFunctionKeys;
                            conn.Execute(SpName, new { GroupInfo.GroupSEQ, FuncKey = data.FunctionKey, SubFuncKey = SubFunctionKeys }, tran, commandType: CommandType.StoredProcedure);
                        }
                    }

                    tran.Commit();
                }
            }
        }
    }
}
