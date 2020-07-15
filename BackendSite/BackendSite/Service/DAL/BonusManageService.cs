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

        public int SetPromotion(bool IsAdd, PromotionInfo promotion)
        {
            int errorCode = 0;
            try
            {
                using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
                conn.Open();
                using var trans = conn.BeginTransaction();
                conn.Execute("Admin_Promotion_InsUpd", new
                {
                    promotion.BonusCode,
                    promotion.Description,
                    promotion.PromotionType,
                    promotion.CategoryId,
                    PeriodFrom = promotion.UnixPeriodFrom,
                    PeriodTo = promotion.UnixPeriodTo,
                    promotion.ExpiryDays,
                    promotion.Sort,
                    Status = 0,
                    promotion.LastUpdateUser,
                    IsAdd,
                    promotion.SiteId
                }, transaction: trans, commandType: CommandType.StoredProcedure);


                foreach (var specific in promotion.Specific)
                {
                    conn.Execute("Admin_PromotionSpecific_InsUpd", new
                    {
                        promotion.BonusCode,
                        specific.CurrencyId,
                        promotion.SiteId,
                        specific.RollOver,
                        specific.Bonus,
                        specific.BonusType,
                        specific.MaxBonus,
                        specific.Status,
                        IsAdd,
                        specific.Details
                    }, transaction: trans, commandType: CommandType.StoredProcedure);
                }

                trans.Commit();
            }
            catch (Exception ex)
            {
                errorCode = 999;
            }

            return errorCode;
        }

        public int SetPromotionStatus(string BonusCode, int SiteId, int Status, string LastUpdateUser)
        {
            int errorCode = 0;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
                {
                    string SpName = "Admin_Promotion_SetStatus";
                    conn.Execute(SpName, new { BonusCode, SiteId, Status, LastUpdateUser }, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception)
            {
                errorCode = 999;
            }

            return errorCode;
        }
    }
}
