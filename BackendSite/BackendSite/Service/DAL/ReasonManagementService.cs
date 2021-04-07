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
    public class ReasonManagementService
    {
        private readonly IConfiguration configuration;

        public ReasonManagementService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<ReasonManagementFuncKey> GetFuncKey()
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<ReasonManagementFuncKey>("Admin_BankFinancialReason_FuncKey_Get", commandType: CommandType.StoredProcedure);
        }
    }
}
