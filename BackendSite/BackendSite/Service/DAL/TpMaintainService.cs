using BackendSite.Service.Model.TpMaintain;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using TpGameType = BackendSite.Service.Model.TpMaintain.TpGameType;

namespace BackendSite.Service.DAL
{
    public class TpMaintainService
    {
        private readonly IConfiguration configuration;

        public TpMaintainService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<CommonTpInfo> GetCommonTpList()
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<CommonTpInfo>("Common_TpList_Get", commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<TpMaintain> GetTpMaintainList()
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<TpMaintain>("Admin_TpMaintain_Get", commandType: CommandType.StoredProcedure);
        }

        public bool InsertTpMaintain(TpMaintain input)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirst<int>("Admin_TpMaintainV2_Ins", new DynamicParameters(input), commandType: CommandType.StoredProcedure) > 0;
        }

        public bool UpdateTpMaintain(TpMaintain input)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_TpMaintain_Set", new DynamicParameters(input), commandType: CommandType.StoredProcedure) > 0;
        }

        public bool DeleteTpMaintain(int sn)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_TpMaintain_Del", new { sn }, commandType: CommandType.StoredProcedure) > 0;
        }

        public IEnumerable<TpGameType> GetTpGameTypeList()
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<TpGameType>("Admin_TpGameType_Get", commandType: CommandType.Text);
        }

        public IEnumerable<TpCategory> GetTPCategoryList()
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<TpCategory>("Admin_TPCategory_Get", commandType: CommandType.Text);
        }
    }
}
