using BackendSite.Service.Model;
using Dapper;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
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
                int? SiteId = null;
                string SpName = "Admin_Promotion_Get";
                return conn.Query<PromotionInfo>(SpName, new { BonusCode, SiteId }, commandType: CommandType.StoredProcedure);
            }
        }

        public PromotionInfo GetPromotion(string BonusCode, int SiteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();
            var promotion = conn.Query<PromotionInfo>("Admin_Promotion_Get", new { BonusCode, SiteId }, transaction: trans, commandType: CommandType.StoredProcedure).FirstOrDefault();
            if (promotion != null)
            {
                promotion.Specific = conn.Query<PromotionSpecific>("Admin_PromotionSpecific_Get", new { BonusCode, SiteId }, transaction: trans, commandType: CommandType.StoredProcedure);
            }
            trans.Commit();
            return promotion;
        }

        public int SetPromotion(bool IsAdd, PromotionInfo promotion)
        {
            int errorCode = 0;
            var refPromotion = GetPromotion(promotion.BonusCode, promotion.SiteId);
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();
            var p = new DynamicParameters();
            p.Add("BonusCode", promotion.BonusCode);
            p.Add("Description", promotion.Description);
            p.Add("PromotionType", promotion.PromotionType);
            p.Add("CategoryId", promotion.CategoryId);
            p.Add("PeriodFrom", promotion.UnixPeriodFrom);
            p.Add("PeriodTo", promotion.UnixPeriodTo);
            p.Add("ExpiryDays", promotion.ExpiryDays);
            p.Add("Sort", promotion.Sort);
            p.Add("Status", promotion.Status);
            p.Add("LastUpdateUser", promotion.LastUpdateUser);
            p.Add("SiteId", promotion.SiteId);
            p.Add("WalletType", promotion.WalletType);
            p.Add("TpId", promotion.TpId);
            p.Add("IsAdd", IsAdd);
            p.Add("ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            conn.Execute("Admin_Promotion_InsUpd", p, transaction: trans, commandType: CommandType.StoredProcedure);
            errorCode = p.Get<int>("ErrorCode");
            if(errorCode == 0)
            {
                List<PromotionSpecific> specificList = new List<PromotionSpecific>();

                foreach (var specific in promotion.Specific)
                {
                    specificList.Add(new PromotionSpecific
                    {
                        CurrencyId = specific.CurrencyId,
                        RollOver = specific.RollOver,
                        Bonus = specific.Bonus,
                        BonusType = specific.BonusType,
                        MaxBonus = specific.MaxBonus,
                        Status = specific.Status,
                        Usage = specific.Usage,
                        Details = specific.Details,
                        RequiredAmount = specific.RequiredAmount,
                        LockedType = specific.LockedType
                    });
                }

                conn.Execute("Admin_PromotionSpecificV2_Set", new
                {
                    promotion.BonusCode,
                    promotion.SiteId,
                    SpecificList = JsonConvert.SerializeObject(specificList)
                }, transaction: trans, commandType: CommandType.StoredProcedure);

                trans.Commit();
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

        public IEnumerable<PromoCategory> GetPromoCategories(int WalletType)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string SpName = "Admin_PromotionCategory_Get";
                return conn.Query<PromoCategory>(SpName, new { WalletType }, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
