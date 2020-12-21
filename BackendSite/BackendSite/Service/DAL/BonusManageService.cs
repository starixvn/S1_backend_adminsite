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

        public IEnumerable<PromotionInfo> GetPromotionList(string BonusCode, FilterPara filterPara)
        {
            int siteidlength = filterPara.SiteidFilter == null ? 0 : filterPara.SiteidFilter.Length;
            int promotionTypelength = filterPara.PromotionTypeFilter == null ? 0 : filterPara.PromotionTypeFilter.Length;
            int[] SiteIdFilterArray = new int[siteidlength];
            int[] PromotionTypeFilterArray = new int[promotionTypelength];

            if (siteidlength > 0)
            {
                for (var i = 0; i < filterPara.SiteidFilter.Length; i++)
                {
                    SiteIdFilterArray[i] = filterPara.SiteidFilter[i].value;
                }
            }
            if(promotionTypelength > 0)
            {
                for (var i = 0; i < filterPara.PromotionTypeFilter.Length; i++)
                {
                    PromotionTypeFilterArray[i] = filterPara.PromotionTypeFilter[i].value;
                }
            }
            string SiteIdFilter = JsonConvert.SerializeObject(SiteIdFilterArray);
            string PromotionTypeFilter = JsonConvert.SerializeObject(PromotionTypeFilterArray);
            string PermissionSiteIds = JsonConvert.SerializeObject(filterPara.SiteArray);
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                int? SiteId = null;
                string SpName = "Admin_PromotionList_Get";
                return conn.Query<PromotionInfo>(SpName, new { BonusCode, SiteId, SiteIdFilter, PermissionSiteIds, PromotionTypeFilter, Status = filterPara.Status }, commandType: CommandType.StoredProcedure);
            }
        }

        public PromotionInfo GetPromotion(string BonusCode, int SiteId)
        {
            string PermissionSiteIds = null;
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();
            var promotion = conn.Query<PromotionInfo>("Admin_Promotion_Get", new { BonusCode, SiteId, PermissionSiteIds }, transaction: trans, commandType: CommandType.StoredProcedure).FirstOrDefault();
            if (promotion != null)
            {
                promotion.Specific = conn.Query<PromotionSpecific>("Admin_PromotionSpecific_Get", new { BonusCode, SiteId }, transaction: trans, commandType: CommandType.StoredProcedure);

                if (promotion.Specific != null)
                {
                    var rebateList = conn.Query<PromotionRebate1>("Admin_PromotionRebate_Get", new { BonusCode, SiteId }, transaction: trans, commandType: CommandType.StoredProcedure);
                    foreach(var s in promotion.Specific)
                    {
                        if (!string.IsNullOrEmpty(s.VN_AffIds))
                        {
                            s.VN_AffIds = JsonConvert.DeserializeObject<string[]>(s.VN_AffIds) == null ? null : string.Join(',', JsonConvert.DeserializeObject<string[]>(s.VN_AffIds));
                        }

                        if (!string.IsNullOrEmpty(s.VN_AffCodes))
                        {
                            s.VN_AffCodes = JsonConvert.DeserializeObject<string[]>(s.VN_AffCodes) == null ? null : string.Join(',', JsonConvert.DeserializeObject<string[]>(s.VN_AffCodes));
                        }

                        s.RebateList = rebateList.Where(x => x.CurrencyId == s.CurrencyId);
                    }
                }
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
            p.Add("RedeemHours", promotion.RedeemHours);
            p.Add("PeriodType", promotion.PeriodType);
            p.Add("PeriodDay", promotion.PeriodDay);
            p.Add("SpecialCode", promotion.SpecialCode);
            p.Add("IsSpecial", promotion.IsSpecial);
            p.Add("ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            conn.Execute("Admin_Promotion_Set", p, transaction: trans, commandType: CommandType.StoredProcedure);
            errorCode = p.Get<int>("ErrorCode");
            if (errorCode == 0)
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
                        MaxBonus = specific.BonusType == 2 ? specific.Bonus : specific.MaxBonus,
                        Status = specific.Status,
                        Usage = specific.Usage,
                        Details = specific.Details,
                        RequiredAmount = specific.RequiredAmount,
                        LockedType = specific.LockedType,
                        MinPayout = specific.MinPayout,
                        VN_AffIds = !string.IsNullOrEmpty(specific.VN_AffIds) ? JsonConvert.SerializeObject(specific.VN_AffIds.Split(",")) : null,
                        VN_AffCodes = !string.IsNullOrEmpty(specific.VN_AffCodes) ? JsonConvert.SerializeObject(specific.VN_AffCodes.Split(",")) : null
                    });
                }

                conn.Execute("Admin_PromotionSpecific_Set", new
                {
                    promotion.BonusCode,
                    promotion.SiteId,
                    SpecificList = JsonConvert.SerializeObject(specificList)
                }, transaction: trans, commandType: CommandType.StoredProcedure);

                if (promotion.PromotionType == 6 || promotion.PromotionType == 7)
                {
                    List<PromotionRebate1> RebateList = new List<PromotionRebate1>();
                    foreach (var s in promotion.Specific)
                    {
                        foreach (var r in s.RebateList)
                        {
                            RebateList.Add(new PromotionRebate1
                            {
                                CurrencyId = s.CurrencyId,
                                GameType = r.GameType,
                                Percentage = r.Percentage,
                                Status = r.Status,
                            });
                        }
                    }

                    conn.Execute("Admin_PromotionRebate_Set", new
                    {
                        promotion.BonusCode,
                        promotion.SiteId,
                        RebateList = JsonConvert.SerializeObject(RebateList)
                    }, transaction: trans, commandType: CommandType.StoredProcedure);
                }

                trans.Commit();
            }

            return errorCode;
        }
        public int DuplicatePromotion(string bonusCode, int siteId, string newBonusCode)
        {
            int errorCode = 0;
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();
            var result = conn.Execute("Admin_PromotionDuplicate_Set", new { BonusCode = bonusCode, SiteId = siteId, NewBonusCode = newBonusCode }, transaction: trans, commandType: CommandType.StoredProcedure);
            trans.Commit();

            if (result == 0)
            {
                errorCode = 1;
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
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport")))
            {
                string SpName = "Admin_PromotionCategory_Get";
                return conn.Query<PromoCategory>(SpName, new { WalletType }, commandType: CommandType.StoredProcedure);
            }
        }
        public IEnumerable<TpGameType> GetTpGameList()
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<TpGameType>("Admin_TpGameType_Get", commandType: CommandType.StoredProcedure);
        }

        public async Task<BonusImportResult> BonusImportAsync(BonusImportPara bonusImportPara)
        {
            BonusImportResult Result = new BonusImportResult() {
                SiteId = bonusImportPara.SiteId,
                UserName = bonusImportPara.UserName,
                BonusAmount = bonusImportPara.BonusAmount,
                RollOver = bonusImportPara.RollOver,
                ExpiryDay = bonusImportPara.ExpiryDay,
                Memo = bonusImportPara.Memo,
                Status = 0,
                ErrorMessage = string.Empty
            };
            
            try
            {
                using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
                conn.Open();

                using var trans = conn.BeginTransaction();

                DynamicParameters p = new DynamicParameters();
                p.Add("@SiteId", bonusImportPara.SiteId);
                p.Add("@UserName", bonusImportPara.UserName);
                var customer = conn.Query<Customer>("Admin_CustInfoByUserName_Get", p, trans, commandType: CommandType.StoredProcedure).ToList();

                //檢核是否有該筆UserName
                if (customer.Count == 0)
                {
                    Result.Status = 1;
                    Result.ErrorMessage = $"SiteId:{bonusImportPara.SiteId} UserName:{bonusImportPara.UserName} BonusAmount:{bonusImportPara.BonusAmount} 查詢不到該筆UserName";
                }
                else
                {
                    DynamicParameters custBonusPara = new DynamicParameters();
                    custBonusPara.Add("@CustId", customer.FirstOrDefault().CustId);
                    var bonusInfo = conn.Query<CustBonus>("Admin_CustBonus_Get", custBonusPara, trans, commandType: CommandType.StoredProcedure).ToList();
                    

                    //查詢該名User是否有未完成的Bonus活動
                    if (bonusInfo.Count > 0)
                    {
                        Result.Status = 1;
                        Result.ErrorMessage = $"SiteId:{bonusImportPara.SiteId} UserName:{bonusImportPara.UserName} BonusAmount:{bonusImportPara.BonusAmount} 該名User還有未完成的Bonus活動";
                    }
                    else
                    {
                        DynamicParameters para = new DynamicParameters();
                        para.Add("@PromotionType", 3);
                        para.Add("@BonusCode", "X");
                        para.Add("@CustId", customer.FirstOrDefault().CustId);
                        para.Add("@UserName", bonusImportPara.UserName);
                        para.Add("@CurrencyId", 4);
                        para.Add("@SiteId", bonusImportPara.SiteId);
                        para.Add("@BonusInfo", JsonConvert.SerializeObject(bonusImportPara));
                        para.Add("@LockedAmount", 0);
                        para.Add("@BonusAmount", bonusImportPara.BonusAmount);
                        para.Add("@ExpiryDays", bonusImportPara.ExpiryDay);
                        para.Add("@Memo", bonusImportPara.Memo);
                        para.Add("@Prefix", "BD");
                        para.Add("@TransId", dbType: DbType.String, direction: ParameterDirection.Output);
                        conn.Execute("Member_BonusInfo_Ins", para, transaction: trans, commandType: CommandType.StoredProcedure);

                        string transId = para.Get<string>("TransId");
                        int.TryParse(transId.Replace("BD", ""), out int BonusId);
                        long expiryTime = DateTimeOffset.Now.AddDays(bonusImportPara.ExpiryDay).ToUnixTimeSeconds();

                        DynamicParameters para2 = new DynamicParameters();
                        para2.Add("@BonusId", BonusId);
                        para2.Add("@PromoCategoryId", bonusImportPara.CategoryId);
                        para2.Add("@TpId", bonusImportPara.TpId);
                        para2.Add("@CustId", customer.FirstOrDefault().CustId);
                        para2.Add("@UserName", bonusImportPara.UserName);
                        para2.Add("@SiteId", bonusImportPara.SiteId);
                        para2.Add("@BonusAmount", bonusImportPara.BonusAmount);
                        para2.Add("@LockedAmount", 0);
                        para2.Add("@Rollover", bonusImportPara.BonusAmount * bonusImportPara.RollOver);
                        para2.Add("@ExpiryTime", expiryTime);
                        para2.Add("@TYPE", 5);
                        para2.Add("@Memo", null);
                        para2.Add("@RefTransId", transId);
                        conn.Execute("Common_Bonus_Ins", para2, transaction: trans, commandType: CommandType.StoredProcedure);
                    }
                }
                
                await Task.Run(() => trans.Commit());
               
            }
            catch (Exception ex)
            {
                Result.Status = 1;
                Result.ErrorMessage = $"SiteId:{bonusImportPara.SiteId} UserName:{bonusImportPara.UserName} BonusAmount:{bonusImportPara.BonusAmount} " + ex.ToString();
            }
            return Result;
        }
    }
}
