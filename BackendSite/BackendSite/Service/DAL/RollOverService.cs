using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
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
    public class RollOverService
    {
        private readonly IConfiguration configuration;
        public RollOverService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public int SetRollOver(int custId, int siteId, string userName, string bonusCode, decimal rollOver, string Memo, string RefTransId, out decimal afterRollOver, MySqlConnection conn, MySqlTransaction trans)
        {
            var p = new DynamicParameters(new { CustId = custId, SiteId = siteId, UserName = userName, BonusCode = bonusCode, RollOver = rollOver, Memo = Memo, RefTransId = RefTransId });
            p.Add("AfterRollOver", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            var result = conn.Execute("Common_RollOver_Set", p, transaction: trans, commandType: CommandType.StoredProcedure);
            afterRollOver = p.Get<decimal?>("AfterRollOver") ?? 0;
            return result;
        }
        public CustRollOver GetCustRollOver(int custId, string bonusCode)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var result = conn.QueryFirstOrDefault<CustRollOver>("Common_RollOver_Get", new { CustId = custId, BonusCode = bonusCode }, commandType: CommandType.StoredProcedure);
            return result;
        }
    }
}
