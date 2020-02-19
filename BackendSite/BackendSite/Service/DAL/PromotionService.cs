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
    public class PromotionService
    {
        private readonly IConfiguration configuration;

        public PromotionService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public IEnumerable<Promotion> GetPromotionListByCustId(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<Promotion>("Admin_PromotionListByCustId_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }

        public int CancelBonusByBonusId(int bonusId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();
            int result = 0;
            result = conn.Execute("Common_CancelBonusByBonusId_Set", new { BonusId = bonusId }, transaction: trans, commandType: CommandType.StoredProcedure);
            trans.Commit();
            return result;
        }
    }
}
