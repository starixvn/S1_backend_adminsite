using BackendSite.Service.Model;
using Dapper;
using MySql.Data.MySqlClient;
using System.Data;

namespace BackendSite.Service.DAL
{
    public class RollOverService
    {
        private readonly IConfiguration configuration;
        public RollOverService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public CustRollOver GetCustRollOver(int custId, string bonusCode)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var result = conn.QueryFirstOrDefault<CustRollOver>("Common_RollOver_Get", new { CustId = custId, BonusCode = bonusCode }, commandType: CommandType.StoredProcedure);
            return result;
        }
    }
}
