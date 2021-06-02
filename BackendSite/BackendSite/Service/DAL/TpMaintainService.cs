using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

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
            return conn.Query<CommonTpInfo>("Common_TpList_Get", new { }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<TpMaintain> GetTpMaintainList(int SiteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<TpMaintain>("TpMaintain_Get", new { SiteId }, commandType: CommandType.StoredProcedure);
        }

        public bool InsertTpMaintain(TpMaintain input)
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirst<int>("Admin_TpMaintain_Ins", new DynamicParameters(input), commandType: CommandType.StoredProcedure) > 0;
        }

        public bool DeleteTpMaintain(TpMaintain input)
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_TpMaintain_Del", new { input.SN }, commandType: CommandType.StoredProcedure) > 0;
        }
    }
}
