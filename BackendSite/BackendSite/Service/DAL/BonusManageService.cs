using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.Model;
using BackendSite.Service.Model.PromotionDisplayCondition;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BackendSite.Service.DAL
{
    public class BonusManageService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BonusManageService> _logger;

        public BonusManageService(IConfiguration configuration,
            ILogger<BonusManageService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public int SetPromotionStatus(string bonusCode, int siteId, int status, string lastUpdateUser)
        {
            int errorCode = 0;
            try
            {
                using (MySqlConnection conn =
                    new MySqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {
                    string SpName = "Admin_Promotion_SetStatus";
                    conn.Execute(SpName, new {bonusCode, siteId, status, lastUpdateUser},
                        commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception)
            {
                errorCode = 999;
            }

            return errorCode;
        }

        public IEnumerable<PromoCategory> GetPromoCategories(int walletType)
        {
            using (MySqlConnection conn =
                new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport")))
            {
                string SpName = "Admin_PromotionCategory_Get";
                return conn.Query<PromoCategory>(SpName, new {walletType}, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<TpGameType> GetTpGameList()
        {
            using MySqlConnection conn =
                new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<TpGameType>("Admin_TpGameType_Get", commandType: CommandType.StoredProcedure);
        }

        public async Task<BonusImportResult> BonusImportAsync(BonusImportPara bonusImportPara)
        {
            BonusImportResult result = new BonusImportResult()
            {
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
                using MySqlConnection conn =
                    new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
                conn.Open();

                using var trans = conn.BeginTransaction();

                DynamicParameters p = new DynamicParameters();
                p.Add("@SiteId", bonusImportPara.SiteId);
                p.Add("@UserName", bonusImportPara.UserName);
                var customer = conn.Query<Customer>("Admin_CustInfoByUserName_Get", p, trans,
                    commandType: CommandType.StoredProcedure).ToList();

                //檢核是否有該筆UserName
                if (customer.Count == 0)
                {
                    result.Status = 1;
                    result.ErrorMessage =
                        $"SiteId:{bonusImportPara.SiteId} UserName:{bonusImportPara.UserName} BonusAmount:{bonusImportPara.BonusAmount} 查詢不到該筆UserName";
                }
                else
                {
                    DynamicParameters custBonusPara = new DynamicParameters();
                    custBonusPara.Add("@CustId", customer.FirstOrDefault().CustId);
                    var bonusInfo = conn.Query<CustBonus>("Admin_CustBonus_Get", custBonusPara, trans,
                        commandType: CommandType.StoredProcedure).ToList();


                    //查詢該名User是否有未完成的Bonus活動
                    if (bonusInfo.Count > 0)
                    {
                        result.Status = 1;
                        result.ErrorMessage =
                            $"SiteId:{bonusImportPara.SiteId} UserName:{bonusImportPara.UserName} BonusAmount:{bonusImportPara.BonusAmount} 該名User還有未完成的Bonus活動";
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
                        conn.Execute("Member_BonusInfo_Ins", para, transaction: trans,
                            commandType: CommandType.StoredProcedure);

                        string transId = para.Get<string>("TransId");
                        int.TryParse(transId.Replace("BD", ""), out int bonusId);
                        long expiryTime = DateTimeOffset.Now.AddDays(bonusImportPara.ExpiryDay).ToUnixTimeSeconds();

                        DynamicParameters para2 = new DynamicParameters();
                        para2.Add("@BonusId", bonusId);
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
                        conn.Execute("Common_Bonus_Ins", para2, transaction: trans,
                            commandType: CommandType.StoredProcedure);
                    }
                }

                await Task.Run(() => trans.Commit());
            }
            catch (Exception ex)
            {
                result.Status = 1;
                result.ErrorMessage =
                    $"SiteId:{bonusImportPara.SiteId} UserName:{bonusImportPara.UserName} BonusAmount:{bonusImportPara.BonusAmount} " +
                    ex.ToString();
            }

            return result;
        }

        public int SetScratchCardCode(string specialCode, string scratchCardCode, string lastUpdateUser)
        {
            int errorCode = 0;
            try
            {
                using (MySqlConnection conn =
                    new MySqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {
                    string SpName = "Admin_ScratchCardCode_set";
                    conn.Execute(SpName, new {specialCode, scratchCardCode, lastUpdateUser},
                        commandType: CommandType.StoredProcedure);
                    // logger.LogInformation($"success - SpecialCode:{SpecialCode} ScratchCardCode:{ScratchCardCode}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                // logger.LogInformation($"Fail - SpecialCode:{SpecialCode} ScratchCardCode:{ScratchCardCode} error:{e.Message}");
            }

            return errorCode;
        }

        public IEnumerable<ScratchCard> GetScratchCardList(string specialCode)
        {
            using (MySqlConnection conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                string SpName = "Admin_ScratchCardCodeList_Get";
                return conn.Query<ScratchCard>(SpName, new {SpecialCode = specialCode, CustId = -1},
                    commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<ScratchCardExport> GetScratchCardListExport(string specialCode)
        {
            using (MySqlConnection conn =
                new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport")))
            {
                string SpName = "Admin_ScratchCardCodeListExport_Get";
                return conn.Query<ScratchCardExport>(SpName, new {SpecialCode = specialCode},
                    commandType: CommandType.StoredProcedure);
            }
        }
        public IEnumerable<LeaderboardExtraPointList> GetLeaderboardExtraPointList(IEnumerable<int> selectSite, int categoryType, int currencyId, long startDate, long endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            string SelectSite = null;
            if (selectSite != null && selectSite.Any())
            {
                SelectSite = JsonConvert.SerializeObject(selectSite);
            }
            totalPages = 0;

            var p = new DynamicParameters(new { SelectSite, CategoryType = categoryType, CurrencyId = currencyId, PageSize = pageSize });
            p.Add("StartDate", startDate);
            p.Add("EndDate", endDate);
            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.Query<LeaderboardExtraPointList>("Admin_LeaderboardExtraPoint_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return result;
        }

        public int SetLeaderboardExtraPoint(long activeDate, int categoryId, string userName, decimal extraPoint, string lastUpdateUser)
        {
            
            int Rtn = -1;

            var p = new DynamicParameters(new { _ActiveDate = activeDate, _CategoryId = categoryId, _UserName = userName, _ExtraPoint = extraPoint, _LastUpdateUser = lastUpdateUser });
            p.Add("RTN", dbType: DbType.Int32, direction: ParameterDirection.Output);

            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            int result = conn.Execute("Admin_LeaderboardExtraPoint_Set", p, commandType: CommandType.StoredProcedure);
            Rtn = p.Get<int?>("RTN") ?? -1;
            return Rtn;
        }

        #region Public Method

        /// <summary>
        /// 取得促銷資料清單
        /// </summary>
        /// <param name="siteIds"></param>
        /// <param name="promotionTypes"></param>
        /// <param name="status"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="device"></param>
        /// <param name="bonusCode"></param>
        /// <param name="permissionSiteIds"></param>
        /// <returns></returns>
        public IEnumerable<PromotionInfo> GetPromotionList(string siteIds,
            string promotionTypes,
            int status,
            long startDate,
            long endDate,
            int device,
            string bonusCode,
            string permissionSiteIds)
        {
            var p = new DynamicParameters();
            p.Add("_SiteIds", siteIds);
            p.Add("_PromotionTypes", promotionTypes);
            p.Add("_Status", status);
            p.Add("_StartDate", startDate);
            p.Add("_EndDate", endDate);
            p.Add("_Device", device);
            p.Add("_BonusCode", bonusCode);
            p.Add("_PermissionSiteIds", permissionSiteIds);

            using (MySqlConnection conn =
                new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport")))
            {
                return conn.Query<PromotionInfo>("Admin_PromotionListV3_Get", p,
                    commandType: CommandType.StoredProcedure);
            }
        }

        /// <summary>
        /// 取得指定促銷流程
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public PromotionInfo GetPromotionFlow(string bonusCode, int siteId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();

            var promotion = GetPromotion(bonusCode, siteId, conn, trans);

            if (promotion != null)
            {
                promotion.PromotionDisplayCondition = GetPromotionDisplayCondition(bonusCode, siteId, conn, trans);
                promotion.Specific = GetPromotionSpecific(bonusCode, siteId, conn, trans);

                if (promotion.Specific != null)
                {
                    var rebateList = GetPromotionRebate(bonusCode, siteId, conn, trans);

                    foreach (var s in promotion.Specific)
                    {
                        if (!string.IsNullOrEmpty(s.VN_AffIds))
                        {
                            s.VN_AffIds = JsonConvert.DeserializeObject<string[]>(s.VN_AffIds) == null
                                ? null
                                : string.Join(',', JsonConvert.DeserializeObject<string[]>(s.VN_AffIds));
                        }

                        if (!string.IsNullOrEmpty(s.VN_AffCodes))
                        {
                            s.VN_AffCodes = JsonConvert.DeserializeObject<string[]>(s.VN_AffCodes) == null
                                ? null
                                : string.Join(',', JsonConvert.DeserializeObject<string[]>(s.VN_AffCodes));
                        }

                        s.RebateList = rebateList.Where(x => x.CurrencyId == s.CurrencyId);
                    }
                }
            }

            trans.Commit();

            return promotion;
        }

        /// <summary>
        /// 取得促銷基本資料
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public PromotionInfo GetPromotion(string bonusCode, int siteId,
            MySqlConnection connection = null,
            MySqlTransaction transaction = null)
        {
            var p = new DynamicParameters();
            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);

            if (connection == null)
            {
                using MySqlConnection conn =
                    new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
                return conn.QueryFirst<PromotionInfo>("Admin_PromotionV2_Get", p,
                    commandType: CommandType.StoredProcedure);
            }
            else
            {
                return connection.QueryFirst<PromotionInfo>("Admin_PromotionV2_Get",
                    p, transaction: transaction,
                    commandType: CommandType.StoredProcedure);
            }
        }

        /// <summary>
        /// 取得指定促銷規則??
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public IEnumerable<PromotionSpecific> GetPromotionSpecific(string bonusCode, int siteId,
            MySqlConnection connection = null,
            MySqlTransaction transaction = null)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);

            if (connection == null)
            {
                using MySqlConnection conn =
                    new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
                return conn.Query<PromotionSpecific>("Admin_PromotionSpecificV3_Get", p,
                    commandType: CommandType.StoredProcedure);
            }
            else
            {
                return connection.Query<PromotionSpecific>("Admin_PromotionSpecificV3_Get", p, transaction: transaction,
                    commandType: CommandType.StoredProcedure);
            }
        }

        /// <summary>
        /// 取得指定促銷回饋
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public IEnumerable<PromotionRebate1> GetPromotionRebate(string bonusCode, int siteId,
            MySqlConnection connection = null,
            MySqlTransaction transaction = null)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);

            if (connection == null)
            {
                using MySqlConnection conn =
                    new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
                return conn.Query<PromotionRebate1>("Admin_PromotionRebateV2_Get", p,
                    commandType: CommandType.StoredProcedure);
            }
            else
            {
                return connection.Query<PromotionRebate1>("Admin_PromotionRebateV2_Get", p, transaction: transaction,
                    commandType: CommandType.StoredProcedure);
            }
        }

        /// <summary>
        /// 取得促銷顯示條件
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public IEnumerable<PromotionDisplayConditionModel> GetPromotionDisplayCondition(string bonusCode, int siteId,
            MySqlConnection connection = null,
            MySqlTransaction transaction = null)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);

            if (connection == null)
            {
                using MySqlConnection conn =
                    new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
                return conn.Query<PromotionDisplayConditionModel>("Admin_PromotionDisplayCondition_Get", p,
                    commandType: CommandType.StoredProcedure);
            }
            else
            {
                return connection.Query<PromotionDisplayConditionModel>("Admin_PromotionDisplayCondition_Get", p,
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure);
            }
        }

        /// <summary>
        /// 設定促銷流程
        /// </summary>
        /// <param name="isAdd"></param>
        /// <param name="promotion"></param>
        /// <param name="promotionDisplayConditionList"></param>
        /// <param name="expectedDelSpecificRow"></param>
        /// <param name="expectedDelRebateRow"></param>
        /// <param name="expectedDelConditionRow"></param>
        /// <returns></returns>
        public bool SetPromotionFlow(bool isAdd, PromotionInfo promotion,
            List<PromotionDisplayConditionModel> promotionDisplayConditionList, int expectedDelSpecificRow,
            int expectedDelRebateRow, int expectedDelConditionRow,string Operator)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();

            //Step1:Set Promotion
            var setAffectCount = SetPromotion(promotion, conn, trans);

            if (setAffectCount == 0)
            {
                trans.Rollback();
                return false;
            }

            if (isAdd && setAffectCount != 1)
            {
                trans.Rollback();
                return false;
            }

            //Setp:inset AdjustmentinfoLog
            #region
            var insertcount = InsertAdjustmentInfoLog(promotion, conn, trans, Operator);
            if (insertcount == 0)
            {
                trans.Rollback();
                return false;
            }
            #endregion


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
                    VN_AffIds = !string.IsNullOrEmpty(specific.VN_AffIds)
                        ? JsonConvert.SerializeObject(specific.VN_AffIds.Split(","))
                        : null,
                    VN_AffCodes = !string.IsNullOrEmpty(specific.VN_AffCodes)
                        ? JsonConvert.SerializeObject(specific.VN_AffCodes.Split(","))
                        : null,
                    TotalUsageLimit = specific.TotalUsageLimit
                });
            }

            //Step2-1:Set Promotion Specific
            if (!SetPromotionSpecific(promotion.BonusCode, promotion.SiteId, specificList, conn, trans))
            {
                trans.Rollback();
                return false;
            }

            //Step2-2:If Update,Delete old Promotion Specific
            if (setAffectCount == 2)
            {
                DeletePromotionSpecific(promotion.BonusCode, promotion.SiteId, specificList,
                    expectedDelSpecificRow,
                    conn, trans);
            }

            //Step3-1:Set Promotion Display Condition
            if (promotionDisplayConditionList.Any())
            {
                if (!SetPromotionDisplayCondition(promotion.BonusCode, promotion.SiteId, promotionDisplayConditionList,
                    conn, trans))
                {
                    trans.Rollback();
                    return false;
                }
            }

            //Step3-2:If Update,Delete old Promotion Display Condition
            if (setAffectCount == 2)
            {
                DeletePromotionDisplayCondition(promotion.BonusCode, promotion.SiteId, promotionDisplayConditionList,
                    expectedDelConditionRow,
                    conn, trans);
            }

            if (promotion.PromotionType == 6 || promotion.PromotionType == 7)
            {
                List<PromotionRebate1> rebateList = new List<PromotionRebate1>();
                foreach (var s in promotion.Specific)
                {
                    foreach (var r in s.RebateList)
                    {
                        rebateList.Add(new PromotionRebate1
                        {
                            CurrencyId = s.CurrencyId,
                            GameType = r.GameType,
                            Percentage = r.Percentage,
                            Status = r.Status,
                        });
                    }
                }

                //Step4-1:Set Promotion Rebate
                if (!SetPromotionRebate(promotion.BonusCode, promotion.SiteId, rebateList, conn, trans))
                {
                    trans.Rollback();
                    return false;
                }

                //Step4-2:If Update,Delete old Rebate
                if (setAffectCount == 2)
                {
                    DeletePromotionRebate(promotion.BonusCode, promotion.SiteId, rebateList, expectedDelRebateRow,
                        conn, trans);
                }
            }

            trans.Commit();
            return true;
        }

        /// <summary>
        /// 複製促銷流程
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <param name="newBonusCode"></param>
        /// <param name="expectedSpecificRow"></param>
        /// <param name="expectedRebateRow"></param>
        /// <param name="expectedConditionRow"></param>
        /// <returns></returns>
        public bool DuplicatePromotionFlow(string bonusCode, int siteId, string newBonusCode, int expectedSpecificRow,
            int expectedRebateRow, int expectedConditionRow)
        {
            using (MySqlConnection connection =
                new MySqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();
                using var transaction = connection.BeginTransaction();

                var promotionRessult = DuplicatePromotion(bonusCode, siteId, newBonusCode, connection, transaction);

                if (!promotionRessult)
                {
                    transaction.Rollback();
                    return false;
                }

                var promotionSpecificRessult = DuplicatePromotionSpecific(bonusCode, siteId, newBonusCode,
                    expectedSpecificRow, connection, transaction);

                if (!promotionSpecificRessult)
                {
                    transaction.Rollback();
                    return false;
                }

                if (expectedRebateRow > 0)
                {
                    var promotionRebateRessult = DuplicatePromotionRebate(bonusCode, siteId, newBonusCode,
                        expectedRebateRow, connection, transaction);

                    if (!promotionRebateRessult)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

                if (expectedConditionRow > 0)
                {
                    var promotionConditionRessult = DuplicatePromotionDisplayCondition(bonusCode, siteId, newBonusCode,
                        expectedConditionRow, connection, transaction);

                    if (!promotionConditionRessult)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

                transaction.Commit();
                return true;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 設定促銷基本資料
        /// </summary>
        /// <param name="promotion"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private int SetPromotion(PromotionInfo promotion, MySqlConnection connection,
            MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", promotion.BonusCode);
            p.Add("_Description", promotion.Description);
            p.Add("_PromotionType", promotion.PromotionType);
            p.Add("_CategoryId", promotion.CategoryId);
            p.Add("_PeriodFrom", promotion.UnixPeriodFrom);
            p.Add("_PeriodTo", promotion.UnixPeriodTo);
            p.Add("_ExpiryDays", promotion.ExpiryDays);
            p.Add("_Sort", promotion.Sort);
            p.Add("_Status", promotion.Status);
            p.Add("_LastUpdateUser", promotion.LastUpdateUser);
            p.Add("_SiteId", promotion.SiteId);
            p.Add("_WalletType", promotion.WalletType);
            p.Add("_TpId", promotion.TpId);
            p.Add("_RedeemHours", promotion.RedeemHours);
            p.Add("_PeriodType", promotion.PeriodType);
            p.Add("_PeriodDay", promotion.PeriodDay);
            p.Add("_SpecialCode", promotion.SpecialCode);
            p.Add("_IsSpecial", promotion.IsSpecial);
            p.Add("_OnlyRegistered", promotion.OnlyRegistered);
            p.Add("_PromotionDeviceType", promotion.PromotionDeviceType);
            p.Add("_PromotionAction", promotion.PromotionAction);
            p.Add("_AutoExtend", promotion.AutoExtend);
            p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("Admin_PromotionV4_Set", p, transaction: transaction,
                commandType: CommandType.StoredProcedure);
            var affectCount = p.Get<int?>("_AffectCount") == null ? 0 : p.Get<int>("_AffectCount");

            return affectCount;
        }


        /// <summary>
        /// 設定促銷規則??
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <param name="specificList"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private bool SetPromotionSpecific(string bonusCode, int siteId, List<PromotionSpecific> specificList,
            MySqlConnection connection,
            MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);
            p.Add("_SpecificList", JsonConvert.SerializeObject(specificList));
            p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("Admin_PromotionSpecificV4_Set", p, transaction: transaction,
                commandType: CommandType.StoredProcedure);

            var affectCount = p.Get<int?>("_AffectCount") == null ? 0 : p.Get<int>("_AffectCount");

            return affectCount > 0;
        }

        /// <summary>
        /// 刪除促銷規則??
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <param name="specificList"></param>
        /// <param name="expectedRow"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private bool DeletePromotionSpecific(string bonusCode, int siteId, List<PromotionSpecific> specificList,
            int expectedRow,
            MySqlConnection connection,
            MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);
            p.Add("_SpecificList", JsonConvert.SerializeObject(specificList));
            p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("Admin_PromotionSpecific_Del", p, transaction: transaction,
                commandType: CommandType.StoredProcedure);

            var affectCount = p.Get<int?>("_AffectCount") == null ? 0 : p.Get<int>("_AffectCount");

            return affectCount == expectedRow;
        }

        /// <summary>
        /// 設定促銷回饋
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <param name="rebateList"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private bool SetPromotionRebate(string bonusCode, int siteId, List<PromotionRebate1> rebateList,
            MySqlConnection connection,
            MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);
            p.Add("_RebateList", JsonConvert.SerializeObject(rebateList));
            p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("Admin_PromotionRebateV2_Set", p, transaction: transaction,
                commandType: CommandType.StoredProcedure);

            var affectCount = p.Get<int?>("_AffectCount") == null ? 0 : p.Get<int>("_AffectCount");

            return affectCount > 0;
        }

        /// <summary>
        /// 刪除促銷回饋
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <param name="rebateList"></param>
        /// <param name="expectedRow"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private bool DeletePromotionRebate(string bonusCode, int siteId, List<PromotionRebate1> rebateList,
            int expectedRow,
            MySqlConnection connection,
            MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);
            p.Add("_RebateList", JsonConvert.SerializeObject(rebateList));
            p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("Admin_PromotionRebate_Del", p, transaction: transaction,
                commandType: CommandType.StoredProcedure);

            var affectCount = p.Get<int?>("_AffectCount") == null ? 0 : p.Get<int>("_AffectCount");

            return affectCount == expectedRow;
        }

        /// <summary>
        /// 設定促銷顯示條件
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <param name="conditionData"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private bool SetPromotionDisplayCondition(string bonusCode, int siteId,
            List<PromotionDisplayConditionModel> conditionData,
            MySqlConnection connection,
            MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);
            p.Add("_ConditionList", JsonConvert.SerializeObject(conditionData));
            p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("Admin_PromotionDisplayCondition_Set", p, transaction: transaction,
                commandType: CommandType.StoredProcedure);

            var affectCount = p.Get<int?>("_AffectCount") == null ? 0 : p.Get<int>("_AffectCount");

            return affectCount > 0;
        }

        /// <summary>
        /// 刪除促銷顯示條件
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <param name="conditionData"></param>
        /// <param name="expectedRow"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private bool DeletePromotionDisplayCondition(string bonusCode, int siteId,
            List<PromotionDisplayConditionModel> conditionData,
            int expectedRow,
            MySqlConnection connection,
            MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);
            p.Add("_ConditionList", JsonConvert.SerializeObject(conditionData));
            p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("Admin_PromotionDisplayCondition_Del", p, transaction: transaction,
                commandType: CommandType.StoredProcedure);

            var affectCount = p.Get<int?>("_AffectCount") == null ? 0 : p.Get<int>("_AffectCount");

            return affectCount == expectedRow;
        }

        /// <summary>
        /// 複製促銷基本資料
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <param name="newBonusCode"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private bool DuplicatePromotion(string bonusCode, int siteId, string newBonusCode, MySqlConnection connection,
            MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);
            p.Add("_NewBonusCode", newBonusCode);
            p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("Admin_PromotionDuplicateV2_Set", p, transaction: transaction,
                commandType: CommandType.StoredProcedure);

            var affectCount = p.Get<int?>("_AffectCount") == null ? 0 : p.Get<int>("_AffectCount");

            return affectCount == 1;
        }

        /// <summary>
        /// 複製促銷規則??
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <param name="newBonusCode"></param>
        /// <param name="expectedRow"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private bool DuplicatePromotionSpecific(string bonusCode, int siteId, string newBonusCode, int expectedRow,
            MySqlConnection connection,
            MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);
            p.Add("_NewBonusCode", newBonusCode);
            p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("Admin_PromotionSpecificDuplicate_Set", p, transaction: transaction,
                commandType: CommandType.StoredProcedure);

            var affectCount = p.Get<int?>("_AffectCount") == null ? 0 : p.Get<int>("_AffectCount");

            return affectCount == expectedRow;
        }

        /// <summary>
        /// 複製促銷回饋
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <param name="newBonusCode"></param>
        /// <param name="expectedRow"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private bool DuplicatePromotionRebate(string bonusCode, int siteId, string newBonusCode, int expectedRow,
            MySqlConnection connection,
            MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);
            p.Add("_NewBonusCode", newBonusCode);
            p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("Admin_PromotionSpecificDuplicate_Set", p, transaction: transaction,
                commandType: CommandType.StoredProcedure);

            var affectCount = p.Get<int?>("_AffectCount") == null ? 0 : p.Get<int>("_AffectCount");

            return affectCount == expectedRow;
        }

        /// <summary>
        /// 複製促銷顯示條件
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <param name="newBonusCode"></param>
        /// <param name="expectedRow"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private bool DuplicatePromotionDisplayCondition(string bonusCode, int siteId, string newBonusCode,
            int expectedRow, MySqlConnection connection,
            MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);
            p.Add("_NewBonusCode", newBonusCode);
            p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("Admin_PromotionDisplayConditionDuplicate_Set", p, transaction: transaction,
                commandType: CommandType.StoredProcedure);

            var affectCount = p.Get<int?>("_AffectCount") == null ? 0 : p.Get<int>("_AffectCount");

            return affectCount == expectedRow;
        }

        #endregion

        #region Refactory Old Code

        /// <summary>
        ///  Get PromotionList old flow.not use in temp.
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="filterPara"></param>
        /// <returns></returns>
        //public IEnumerable<PromotionInfo> GetPromotionList(string bonusCode, FilterPara filterPara)
        //{
        //    int siteidlength = filterPara.SiteidFilter == null ? 0 : filterPara.SiteidFilter.Length;
        //    int promotionTypelength =
        //        filterPara.PromotionTypeFilter == null ? 0 : filterPara.PromotionTypeFilter.Length;
        //    int[] siteIdFilterArray = new int[siteidlength];
        //    int[] promotionTypeFilterArray = new int[promotionTypelength];

        //    if (siteidlength > 0)
        //    {
        //        for (var i = 0; i < siteidlength; i++)
        //        {
        //            siteIdFilterArray[i] = filterPara.SiteidFilter[i].value;
        //        }
        //    }

        //    if (promotionTypelength > 0)
        //    {
        //        for (var i = 0; i < promotionTypelength; i++)
        //        {
        //            promotionTypeFilterArray[i] = filterPara.PromotionTypeFilter[i].value;
        //        }
        //    }

        //    string siteIdFilter = JsonConvert.SerializeObject(siteIdFilterArray);
        //    string promotionTypeFilter = JsonConvert.SerializeObject(promotionTypeFilterArray);
        //    string permissionSiteIds = JsonConvert.SerializeObject(filterPara.SiteArray);
        //    using (MySqlConnection conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString")))
        //    {
        //        int? siteId = null;
        //        return conn.Query<PromotionInfo>("Admin_PromotionListV2_Get", new
        //        {
        //            bonusCode,
        //            siteId,
        //            siteIdFilter,
        //            promotionTypeFilter,
        //            permissionSiteIds,
        //            Status = filterPara.Status,
        //            StartDate = filterPara.StartDate,
        //            EndDate = filterPara.EndDate,
        //            PromotionDeviceType = filterPara.Device
        //        }, commandType: CommandType.StoredProcedure);
        //    }
        //}

        /// <summary>
        ///  Get Promotion old flow.not use in temp.
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        //public PromotionInfo GetPromotion(string bonusCode, int siteId)
        //{
        //    string permissionSiteIds = null;
        //    using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
        //    conn.Open();
        //    using var trans = conn.BeginTransaction();

        //    var promotion = conn.Query<PromotionInfo>("Admin_Promotion_Get", new { bonusCode, siteId, permissionSiteIds },
        //        transaction: trans, commandType: CommandType.StoredProcedure).FirstOrDefault();

        //    if (promotion != null)
        //    {
        //        promotion.PromotionDisplayCondition = GetPromotionDisplayCondition(bonusCode, siteId, conn, trans);
        //        promotion.Specific = GetPromotionSpecific(bonusCode, siteId, conn, trans);

        //        if (promotion.Specific != null)
        //        {
        //            var rebateList = GetPromotionRebate(bonusCode, siteId, conn, trans);

        //            foreach (var s in promotion.Specific)
        //            {
        //                if (!string.IsNullOrEmpty(s.VN_AffIds))
        //                {
        //                    s.VN_AffIds = JsonConvert.DeserializeObject<string[]>(s.VN_AffIds) == null
        //                        ? null
        //                        : string.Join(',', JsonConvert.DeserializeObject<string[]>(s.VN_AffIds));
        //                }

        //                if (!string.IsNullOrEmpty(s.VN_AffCodes))
        //                {
        //                    s.VN_AffCodes = JsonConvert.DeserializeObject<string[]>(s.VN_AffCodes) == null
        //                        ? null
        //                        : string.Join(',', JsonConvert.DeserializeObject<string[]>(s.VN_AffCodes));
        //                }

        //                s.RebateList = rebateList.Where(x => x.CurrencyId == s.CurrencyId);
        //            }
        //        }
        //    }

        //    trans.Commit();

        //    return promotion;
        //}

        /// <summary>
        /// Set Promotion old flow.not use in temp.
        /// </summary>
        /// <param name="promotion"></param>
        /// <returns></returns>
        //public bool SetPromotion(PromotionInfo promotion)
        //{
        //    //var refPromotion = GetPromotion(promotion.BonusCode, promotion.SiteId);
        //    using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
        //    conn.Open();
        //    using var trans = conn.BeginTransaction();

        //    var p = new DynamicParameters();
        //    p.Add("_BonusCode", promotion.BonusCode);
        //    p.Add("_Description", promotion.Description);
        //    p.Add("_PromotionType", promotion.PromotionType);
        //    p.Add("_CategoryId", promotion.CategoryId);
        //    p.Add("_PeriodFrom", promotion.UnixPeriodFrom);
        //    p.Add("_PeriodTo", promotion.UnixPeriodTo);
        //    p.Add("_ExpiryDays", promotion.ExpiryDays);
        //    p.Add("_Sort", promotion.Sort);
        //    p.Add("_Status", promotion.Status);
        //    p.Add("_LastUpdateUser", promotion.LastUpdateUser);
        //    p.Add("_SiteId", promotion.SiteId);
        //    p.Add("_WalletType", promotion.WalletType);
        //    p.Add("_TpId", promotion.TpId);
        //    p.Add("_RedeemHours", promotion.RedeemHours);
        //    p.Add("_PeriodType", promotion.PeriodType);
        //    p.Add("_PeriodDay", promotion.PeriodDay);
        //    p.Add("_SpecialCode", promotion.SpecialCode);
        //    p.Add("_IsSpecial", promotion.IsSpecial);
        //    p.Add("_OnlyRegistered", promotion.OnlyRegistered);
        //    p.Add("_PromotionDeviceType", promotion.PromotionDeviceType);
        //    p.Add("_PromotionAction", promotion.PromotionAction);
        //    p.Add("_AutoExtend", promotion.AutoExtend);
        //    p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

        //    conn.Execute("Admin_PromotionV4_Set", p, transaction: trans, commandType: CommandType.StoredProcedure);
        //    var affectCount = p.Get<int>("_AffectCount");

        //    if (affectCount > 0)
        //    {
        //        List<PromotionSpecific> specificList = new List<PromotionSpecific>();

        //        foreach (var specific in promotion.Specific)
        //        {
        //            specificList.Add(new PromotionSpecific
        //            {
        //                CurrencyId = specific.CurrencyId,
        //                RollOver = specific.RollOver,
        //                Bonus = specific.Bonus,
        //                BonusType = specific.BonusType,
        //                MaxBonus = specific.BonusType == 2 ? specific.Bonus : specific.MaxBonus,
        //                Status = specific.Status,
        //                Usage = specific.Usage,
        //                Details = specific.Details,
        //                RequiredAmount = specific.RequiredAmount,
        //                LockedType = specific.LockedType,
        //                MinPayout = specific.MinPayout,
        //                VN_AffIds = !string.IsNullOrEmpty(specific.VN_AffIds)
        //                    ? JsonConvert.SerializeObject(specific.VN_AffIds.Split(","))
        //                    : null,
        //                VN_AffCodes = !string.IsNullOrEmpty(specific.VN_AffCodes)
        //                    ? JsonConvert.SerializeObject(specific.VN_AffCodes.Split(","))
        //                    : null,
        //                TotalUsageLimit = specific.TotalUsageLimit
        //            });
        //        }

        //        conn.Execute("Admin_PromotionSpecificV3_Set", new
        //        {
        //            promotion.BonusCode,
        //            promotion.SiteId,
        //            SpecificList = JsonConvert.SerializeObject(specificList)
        //        }, transaction: trans, commandType: CommandType.StoredProcedure);

        //        if (promotion.PromotionType == 6 || promotion.PromotionType == 7)
        //        {
        //            List<PromotionRebate1> rebateList = new List<PromotionRebate1>();
        //            foreach (var s in promotion.Specific)
        //            {
        //                foreach (var r in s.RebateList)
        //                {
        //                    rebateList.Add(new PromotionRebate1
        //                    {
        //                        CurrencyId = s.CurrencyId,
        //                        GameType = r.GameType,
        //                        Percentage = r.Percentage,
        //                        Status = r.Status,
        //                    });
        //                }
        //            }

        //            conn.Execute("Admin_PromotionRebate_Set", new
        //            {
        //                promotion.BonusCode,
        //                promotion.SiteId,
        //                RebateList = JsonConvert.SerializeObject(rebateList)
        //            }, transaction: trans, commandType: CommandType.StoredProcedure);
        //        }

        //        trans.Commit();
        //        return true;
        //    }
        //    else
        //    {
        //        trans.Rollback();
        //        return false;
        //    }
        //}

        /// <summary>
        /// Duplicate Promotion old flow.not use in temp.
        /// </summary>
        /// <param name="promotion"></param>
        /// <returns></returns>
        //public int DuplicatePromotion(string bonusCode, int siteId, string newBonusCode)
        //{
        //    int errorCode = 0;
        //    using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
        //    conn.Open();
        //    using var trans = conn.BeginTransaction();
        //    var result = conn.Execute("Admin_PromotionDuplicate_Set",
        //        new { BonusCode = bonusCode, SiteId = siteId, NewBonusCode = newBonusCode }, transaction: trans,
        //        commandType: CommandType.StoredProcedure);
        //    trans.Commit();

        //    if (result == 0)
        //    {
        //        errorCode = 1;
        //    }

        //    return errorCode;
        //}

        #endregion

        #region
        private int InsertAdjustmentInfoLog(PromotionInfo promotion, MySqlConnection connection,
            MySqlTransaction transaction,string Operator)
        {
            DynamicParameters propsLog = new DynamicParameters();

            var temppromotion = JsonConvert.SerializeObject(promotion);
            JObject jobject = JObject.Parse(temppromotion);
            JArray tokenselect = jobject.SelectToken("Specific") as JArray;
            if(tokenselect != null)
            {
                foreach (JObject item in tokenselect)
                {
                    item.Remove("Details");
                }
            }
            string DetailInfo = JsonConvert.SerializeObject(jobject);
            propsLog.Add("Menu", "BonusManagement");
            propsLog.Add("ActionType", 1);
            propsLog.Add("DetailInfo", DetailInfo);
            propsLog.Add("Operator", Operator);
            return connection.Execute("Admin_AdjustmentInfoLog_Set", propsLog, transaction: transaction,
                commandType: CommandType.StoredProcedure);
        }
        #endregion
    }
}