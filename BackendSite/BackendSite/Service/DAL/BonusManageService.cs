using BackendSite.Service.Model;
using Dapper;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
    public class BonusManageService
    {
        private readonly IConfiguration configuration;

        public BonusManageService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<PromotionInfo> GetPromotionList(string BonusCode)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string SpName = "Admin_Promotion_Get";
                return conn.Query<PromotionInfo>(SpName, new { BonusCode }, commandType: CommandType.StoredProcedure);
            }
        }

        public PromotionInfo GetPromotion(string BonusCode)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();
            var promotion = conn.Query<PromotionInfo>("Admin_Promotion_Get", new { BonusCode }, transaction: trans, commandType: CommandType.StoredProcedure).FirstOrDefault();
            if (promotion != null)
            {
                promotion.Specific = conn.Query<PromotionSpecific>("Admin_PromotionSpecific_Get", new { BonusCode }, transaction: trans, commandType: CommandType.StoredProcedure);
            }
            trans.Commit();
            return promotion;
        }
    }
}
