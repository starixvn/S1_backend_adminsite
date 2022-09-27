using System.Data;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.PromotionDisplayCondition;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BackendSite.Service.DAL
{
    public class BonusManageService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public BonusManageService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public int SetPromotionStatus(string bonusCode, int siteId, int status, string lastUpdateUser)
        {
            int errorCode;

            var result = _mainDbClient.Execute("Admin_Promotion_SetStatus", new DynamicParameters(new { bonusCode, siteId, status, lastUpdateUser }));
            if (result > 0)
            {
                errorCode = (int)ErrorCodeEnum.Success;
            }
            else
            {
                errorCode = (int)ErrorCodeEnum.Fail;
            }

            return errorCode;
        }

        public IEnumerable<PromotionType> GetPromotionType(bool isPayout, bool isNew)
        {
            return _reportDbClient.Query<PromotionType>("Admin_PromotionType_Get",new { isPayout, isNew });
        }

        public IEnumerable<PromoCategory> GetPromoCategories(int walletType)
        {
            return _reportDbClient.Query<PromoCategory>("Admin_PromotionCategory_Get_V2", new DynamicParameters(new { _WalletType = walletType }));
        }

        public IEnumerable<TpGameType> GetTpGameList()
        {
            return _reportDbClient.Query<TpGameType>("Admin_TpGameType_Get");
        }

        public IEnumerable<CashBackGame> GetCashBackGameList(int? tpId, int? categoryId)
        {
            return _reportDbClient.Query<CashBackGame>("Admin_CashBackGameType_Get", new DynamicParameters(new { TpId = tpId, PromoCategoryId = categoryId }));
        }

        public async Task<BonusImportResult> BonusImportAsync(BonusImportPara bonusImportPara)
        {
            var result = new BonusImportResult()
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
                using var conn =
                    new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
                conn.Open();

                using var trans = conn.BeginTransaction();

                var p = new DynamicParameters();
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
                    var custBonusPara = new DynamicParameters();
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
                        var para = new DynamicParameters();
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

                        var transId = para.Get<string>("TransId");
                        int.TryParse(transId.Replace("BD", ""), out var bonusId);
                        var expiryTime = DateTimeOffset.Now.AddDays(bonusImportPara.ExpiryDay).ToUnixTimeSeconds();

                        var para2 = new DynamicParameters();
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
            var errorCode = 0;
            try
            {
                _mainDbClient.Execute("Admin_ScratchCardCode_set", new DynamicParameters(new { specialCode, scratchCardCode, lastUpdateUser }));
            }
            catch (Exception ex)
            {
                errorCode = (int)ErrorCodeEnum.SystemException;
            }

            return errorCode;
        }

        public IEnumerable<ScratchCard> GetScratchCardList(string specialCode,
            int statusType,
            int startIndex,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {
            var p = new DynamicParameters();
            p.Add("_SpecialCode", specialCode);
            p.Add("_StatusType", statusType);
            p.Add("_StartIndex", startIndex);
            p.Add("_PageSize", pageSize);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _mainDbClient.Query<ScratchCard>("Admin_ScratchCardCodeList_Get_V3", p);

            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public IEnumerable<ScratchCardExport> GetScratchCardListExport(string specialCode)
        {
            return _reportDbClient.Query<ScratchCardExport>("Admin_ScratchCardCodeListExport_Get_V2", new DynamicParameters(new { SpecialCode = specialCode }));
        }
        public IEnumerable<LeaderboardExtraPointList> GetLeaderboardExtraPointList(string selectSite,
            string categoryType,
            string currencyId,
            DateTime startDate,
            DateTime endDate,
            int startIndex,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {

            var p = new DynamicParameters();
            p.Add("_SiteIds", selectSite);
            p.Add("_CurrencyIds", currencyId);
            p.Add("_CategoryIds", categoryType);
            p.Add("_StartDate", startDate);
            p.Add("_EndDate", endDate.AddDays(1).AddSeconds(-1));
            p.Add("_StartIndex", startIndex);
            p.Add("_PageSize", pageSize);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<LeaderboardExtraPointList>("Admin_LeaderboardExtraPoint_Get_V3", p);

            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public int SetLeaderboardExtraPoint(long activeDate, int siteId, int categoryId, string userName, decimal extraPoint, string lastUpdateUser)
        {
            var p = new DynamicParameters(new { _ActiveDate = activeDate, _SiteId = siteId, _CategoryId = categoryId, _UserName = userName, _ExtraPoint = extraPoint, _LastUpdateUser = lastUpdateUser });
            p.Add("RTN", dbType: DbType.Int32, direction: ParameterDirection.Output);
            _mainDbClient.Execute("Admin_LeaderboardExtraPoint_Set", p);
            var rtn = p.Get<int?>("RTN") ?? -1;

            return rtn;
        }

        #region Public Method

        /// <summary>
        /// 取得促銷資料清單
        /// </summary>
        /// <param name="siteIds"></param>
        /// <param name="currencyIds"></param>
        /// <param name="promotionTypes"></param>
        /// <param name="status"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="device"></param>
        /// <param name="bonusCode"></param>
        /// <param name="permissionSiteIds"></param>
        /// <param name="isSpecial"></param>
        /// <param name="startIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalPages"></param>
        /// <param name="rowCount"></param>
        /// <returns></returns>
        public IEnumerable<PromotionInfo> GetPromotionList(string siteIds,
            string currencyIds,
            string promotionTypes,         
            int status,
            long startDate,
            long endDate,
            int device,
            string bonusCode,
            string permissionSiteIds,
            bool isSpecial,
            int startIndex,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {
            var p = new DynamicParameters();
            p.Add("_SiteIds", siteIds);
            p.Add("_CurrencyIds", currencyIds);
            p.Add("_PromotionTypes", promotionTypes);
            p.Add("_Status", status);
            p.Add("_StartDate", startDate);
            p.Add("_EndDate", endDate);
            p.Add("_Device", device);
            p.Add("_BonusCode", bonusCode);
            p.Add("_PermissionSiteIds", permissionSiteIds);
            p.Add("_IsSpecial", isSpecial ? 1 : 0);
            p.Add("_StartIndex", startIndex);
            p.Add("_PageSize", pageSize);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<PromotionInfo>("Admin_PromotionList_Get_V6", p);

            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        /// <summary>
        /// 取得指定促銷流程
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public PromotionInfo GetPromotionFlow(string bonusCode, int siteId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var promotion = GetPromotion(bonusCode, siteId, conn);

            if (promotion != null)
            {
                promotion.PromotionDisplayCondition = GetPromotionDisplayCondition(bonusCode, siteId, conn);
                promotion.Specific = GetPromotionSpecific(bonusCode, siteId, conn);

                if (promotion.Specific != null)
                {
                    if (promotion.PromotionType == (int)PromotionTypeEnum.Cashback)
                    {
                        var cashBackList = GetPromotionCashBack(bonusCode, siteId, conn);
                        var tempCashBackList = cashBackList
                            .GroupBy(x => new
                                { x.CurrencyId, x.GameType, x.LossAmount, x.Percentage, x.Status, x.TpId }).Select(x =>
                                new PromotionCashBack1
                                {
                                    CurrencyId = x.Key.CurrencyId,
                                    GameType = x.Key.GameType,
                                    LossAmount = x.Key.LossAmount,
                                    Percentage = x.Key.Percentage,
                                    Status = x.Key.Status,
                                    TpId = x.Key.TpId
                                }).ToList();

                        foreach (var specific in promotion.Specific)
                        {
                            specific.CashBackList = tempCashBackList.Where(x => x.CurrencyId == specific.CurrencyId);
                        
                            var cashBack = specific.CashBackList.FirstOrDefault();
                            specific.CashBackSetting = new CashBackSetting
                            {
                                CashBackTpInfos = specific.CashBackList.Select(i => new CashBackTpInfo{ TpId = i.TpId, CategoryId = i.GameType }),
                                Percentage = cashBack.Percentage,
                                LossAmount = cashBack.LossAmount
                            };
                        }
                    }

                    foreach (var specific in promotion.Specific)
                    {
                        if (!string.IsNullOrEmpty(specific.VN_AffCodes))
                        {
                            specific.VN_AffCodes = JsonConvert.DeserializeObject<string[]>(specific.VN_AffCodes) == null
                                ? null
                                : string.Join(',', JsonConvert.DeserializeObject<string[]>(specific.VN_AffCodes));
                        }

                        if (!string.IsNullOrEmpty(specific.TierAmount))
                        {
                            specific.TierAmountList = JsonConvert.DeserializeObject<IEnumerable<TierAmount>>(specific.TierAmount);
                        }
                    }
                }
            }
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
                return _reportDbClient.QueryFirst<PromotionInfo>("Admin_Promotion_Get_V3", p);
            }
            else
            {
                return connection.QueryFirst<PromotionInfo>("Admin_Promotion_Get_V3", p, transaction, commandType: CommandType.StoredProcedure);
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
                return _reportDbClient.Query<PromotionSpecific>("Admin_PromotionSpecificV3_Get", p);
            }
            else
            {
                return connection.Query<PromotionSpecific>("Admin_PromotionSpecificV3_Get", p, transaction, commandType: CommandType.StoredProcedure);
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
                return _reportDbClient.Query<PromotionDisplayConditionModel>("Admin_PromotionDisplayCondition_Get", p);
            }
            else
            {
                return connection.Query<PromotionDisplayConditionModel>("Admin_PromotionDisplayCondition_Get", p, transaction, commandType: CommandType.StoredProcedure);
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
        public bool SetPromotionFlow(bool isAdd, PromotionInfoReq promotion, DisplayConditionAboutVipLevel vipLevelCondition, 
            string Operator)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
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
            
            var isUpdate = setAffectCount == 2;

            //Setp:inset AdjustmentinfoLog
            #region
            var insertCount = InsertAdjustmentInfoLog(promotion, conn, trans, Operator);
            if (insertCount == 0)
            {
                trans.Rollback();
                return false;
            }
            #endregion


            var specificList = new List<PromotionSpecific>();

            foreach (var specific in promotion.Specific)
            {
                specificList.Add(new PromotionSpecific
                {
                    CurrencyId = specific.CurrencyId,
                    RollOver = specific.RollOver,
                    WinOver = specific.WinOver,
                    FullFillType = specific.WinOver > 0 ? (int)FullFillTypeEnum.WinOver : (int)FullFillTypeEnum.RollOver,
                    Bonus = specific.Bonus,
                    BonusType = specific.BonusType,
                    MaxBonus = specific.BonusType == (int)BonusTypeEnum.Amount ? specific.Bonus : specific.MaxBonus,
                    Status = specific.Status,
                    Usage = specific.Usage,
                    Details = specific.Details,
                    RequiredAmount = specific.RequiredAmount,
                    LockedType = specific.LockedType,
                    MinPayout = specific.MinPayout,
                    VN_AffCodes = !string.IsNullOrEmpty(specific.VN_AffCodes)
                        ? JsonConvert.SerializeObject(specific.VN_AffCodes.Split(","))
                        : null,
                    TotalUsageLimit = specific.TotalUsageLimit,
                    TierAmount = specific.TierAmountList.Any() ? JsonConvert.SerializeObject(specific.TierAmountList) : "[]"
                });
            }

            //Step2-1:Set Promotion Specific
            if (!SetPromotionSpecific(promotion.BonusCode, promotion.SiteId, specificList, conn, trans))
            {
                trans.Rollback();
                return false;
            }

            //Step2-2:If Update,Delete old Promotion Specific
            if (isUpdate)
            {
                DeletePromotionSpecific(promotion.BonusCode, promotion.SiteId, specificList,
                    conn, trans);
            }

            //Step3-1:Set Promotion Display Condition
            //Display Condition Logic Process
            var promotionDisplayConditionList = new List<PromotionDisplayConditionModel>();
            if (vipLevelCondition.IsForVipLevel)
            {
                promotionDisplayConditionList.Add(new PromotionDisplayConditionModel()
                {
                    ConditionType = PromotionDisplayConditionType.VipLevel,
                    Description = new VipLevelCondictionModel() { VipLevel = vipLevelCondition.SelectedVipLevel }
                });
            }
            
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
            if (isUpdate)
            {
                DeletePromotionDisplayCondition(promotion.BonusCode, promotion.SiteId, promotionDisplayConditionList,
                    conn, trans);
            }

            if (promotion.PromotionType == (int)PromotionTypeEnum.Cashback)
            {
                var cashBackList = new List<PromotionCashBackSetting>();
                var cashBackGameList = GetCashBackGameList(promotion.TpId, promotion.CategoryId).ToList();
                
                if (!vipLevelCondition.SelectedVipLevel.Any())
                {
                    vipLevelCondition.SelectedVipLevel = new List<int>
                    {
                        (int)PromotionCashBackSettingLevelEnum.All
                    };
                }

                foreach (var specific in promotion.Specific)
                {
                    if (specific.CashBackSetting.CashBackTpInfos != null)
                    {
                        foreach (var tpInfo in specific.CashBackSetting.CashBackTpInfos)
                        {
                            var tpCategoryId = 0;
                            if (promotion.WalletType == (int)PromotionWalletTypeEnum.Transfer || tpInfo.TpId == (int)TpEnum.SBSW)
                            {
                                tpCategoryId = tpInfo.CategoryId;
                            }
                            else
                            {
                                tpCategoryId = tpInfo.CategoryId switch
                                {
                                    (int)PromotionCategoryEnum.SlotsGame => (int)TpCategoryEnum.Casino,
                                    (int)PromotionCategoryEnum.LiveCasino => (int)TpCategoryEnum.LiveCasino,
                                    (int)PromotionCategoryEnum.Fish => (int)TpCategoryEnum.Fishing,
                                    (int)PromotionCategoryEnum.Lottery => (int)TpCategoryEnum.Lottery,
                                    _ => 0
                                };
                            }

                            vipLevelCondition.SelectedVipLevel.ToList().ForEach(vipLevel =>
                            {
                                cashBackList.Add(new PromotionCashBackSetting
                                {
                                    CurrencyId = specific.CurrencyId,
                                    TpId = tpInfo.TpId,
                                    CategoryId = tpCategoryId,
                                    Level = vipLevel,
                                    Percentage = specific.CashBackSetting.Percentage,
                                    LossAmount = specific.CashBackSetting.LossAmount,
                                    Status = (int)PromotionCashBackSettingStatusEnum.Enable
                                });
                            });
                        }
                    }
                    else
                    {
                        //V1 版本，V2 上線後可移除
                        foreach (var cashBack in specific.CashBackList.Where(i => i.LossAmount != 0 && i.Percentage != 0))
                        {
                            var tempCategoryId = promotion.TpId == 0 ? cashBackGameList.FirstOrDefault(x => x.TpId == cashBack.GameType).CategoryId : cashBack.GameType;
                            vipLevelCondition.SelectedVipLevel.ToList().ForEach(vipLevel =>
                            {
                                cashBackList.Add(new PromotionCashBackSetting
                                {
                                    CurrencyId = specific.CurrencyId,
                                    CategoryId = tempCategoryId,
                                    Level = vipLevel,
                                    Percentage = cashBack.Percentage,
                                    LossAmount = cashBack.LossAmount,
                                    Status = cashBack.Status,
                                    TpId = promotion.TpId == 0 ? cashBack.GameType : promotion.TpId
                                });
                            });
                        }
                    }
                }

                if (isUpdate && !DeletePromotionCashBack(promotion.BonusCode, promotion.SiteId, conn, trans))
                {
                    trans.Rollback();
                    return false;
                }

                if (!InsertPromotionCashBack(promotion.BonusCode, promotion.SiteId, Operator, cashBackList, conn,
                        trans))
                {
                    trans.Rollback();
                    return false;
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
        /// <param name="expectedCashBackRow"></param>
        /// <param name="Operator"></param>
        /// <param name="newSpecialCode"></param>
        /// <param name="isSpecial"></param>
        /// <returns></returns>
        public bool DuplicatePromotionFlow(string bonusCode, int siteId, string newBonusCode, int expectedSpecificRow,
            int expectedConditionRow, int expectedCashBackRow, string Operator, string newSpecialCode, bool isSpecial)
        {
            using var connection = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            connection.Open();
            using var transaction = connection.BeginTransaction();
            var promotionResult = DuplicatePromotion(bonusCode, siteId, newBonusCode, newSpecialCode, isSpecial, connection, transaction);

            if (!promotionResult)
            {
                transaction.Rollback();
                return false;
            }
            var promotionSpecificResult = DuplicatePromotionSpecific(bonusCode, siteId, newBonusCode, expectedSpecificRow, connection, transaction);
            if (!promotionSpecificResult)
            {
                transaction.Rollback();
                return false;
            }

            if (expectedConditionRow > 0)
            {
                var promotionConditionResult = DuplicatePromotionDisplayCondition(bonusCode, siteId, newBonusCode, expectedConditionRow, connection, transaction);
                if (!promotionConditionResult)
                {
                    transaction.Rollback();
                    return false;
                }
            }

            if (expectedCashBackRow > 0)
            {
                var promotionCashBackResult = DuplicatePromotionCashBack(bonusCode, siteId, newBonusCode, expectedCashBackRow, connection, transaction, Operator);
                if (!promotionCashBackResult)
                {
                    transaction.Rollback();
                    return false;
                }
            }

            transaction.Commit();
            return true;
        }

        public IEnumerable<int> CheckPromotionExists(string bonusCode, string specialCode)
        {
            var p = new DynamicParameters();
            p.Add("_BonusCode", bonusCode);
            p.Add("_SpecialCode", specialCode);

            return _reportDbClient.Query<int>("Admin_PromotionExists_Get", p);
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
        private int SetPromotion(PromotionInfoReq promotion, MySqlConnection connection,
            MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", promotion.BonusCode);
            p.Add("_Description", promotion.Description);
            p.Add("_PromotionType", promotion.PromotionType);
            p.Add("_CategoryId", promotion.CategoryId);
            p.Add("_PeriodFrom", promotion.PeriodFromTime?.ToUnixTimeSeconds());
            p.Add("_PeriodTo", promotion.PeriodToTime?.ToUnixTimeSeconds());
            p.Add("_DisplayFrom", promotion.DisplayFromTime?.ToUnixTimeSeconds() ?? promotion.PeriodFromTime?.ToUnixTimeSeconds());
            p.Add("_DisplayTo", promotion.DisplayToTime?.ToUnixTimeSeconds() ?? promotion.PeriodToTime?.ToUnixTimeSeconds());
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

            connection.Execute("Admin_PromotionV4_Set", p, transaction, commandType: CommandType.StoredProcedure);
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

            connection.Execute("Admin_PromotionSpecific_Set_V5", p, transaction: transaction,
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
        private int DeletePromotionSpecific(string bonusCode, int siteId, List<PromotionSpecific> specificList,
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

            return affectCount;
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
        private int DeletePromotionDisplayCondition(string bonusCode, int siteId,
            List<PromotionDisplayConditionModel> conditionData,
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

            return affectCount;
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
        private bool DuplicatePromotion(string bonusCode, int siteId, string newBonusCode, string newSpecialCode, bool isSpecial, MySqlConnection connection,
            MySqlTransaction transaction)
        {
            var p = new DynamicParameters();
            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);
            p.Add("_NewBonusCode", newBonusCode);
            p.Add("_NewSpecialCode", newSpecialCode);
            p.Add("_IsSpecial", isSpecial ? 1 : 0);
            p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            connection.Execute("Admin_PromotionDuplicate_Set_V3", p, transaction, commandType: CommandType.StoredProcedure);

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

            connection.Execute("Admin_PromotionSpecificDuplicate_Set_V2", p, transaction: transaction,
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

        #region
        private int InsertAdjustmentInfoLog(PromotionInfoReq promotion, MySqlConnection connection,
            MySqlTransaction transaction, string Operator)
        {
            var propsLog = new DynamicParameters();

            var tempPromotion = JsonConvert.SerializeObject(promotion);
            var jObject = JObject.Parse(tempPromotion);
            var tokenSelect = jObject.SelectToken("Specific") as JArray;
            if (tokenSelect != null)
            {
                foreach (var jToken in tokenSelect)
                {
                    var item = (JObject)jToken;
                    item.Remove("Details");
                }
            }
            var detailInfo = JsonConvert.SerializeObject(jObject);
            propsLog.Add("Menu", "BonusManagement");
            propsLog.Add("ActionType", 1);
            propsLog.Add("DetailInfo", detailInfo);
            propsLog.Add("Operator", Operator);
            return connection.Execute("Admin_AdjustmentInfoLog_Set", propsLog, transaction, commandType: CommandType.StoredProcedure);
        }
        #endregion

        #region
        private bool DeletePromotionCashBack(string bonusCode, int siteId,
            MySqlConnection connection,
            MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);

            return connection.Execute("Admin_PromotionCashBack_Del", p, transaction, commandType: CommandType.StoredProcedure) > 0;
        }

        private bool InsertPromotionCashBack(string bonusCode, int siteId, string Operator, List<PromotionCashBackSetting> cashbackList,
            MySqlConnection connection,
            MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);
            p.Add("_Operator", Operator);
            p.Add("_CashBackList", JsonConvert.SerializeObject(cashbackList));

            return connection.Execute("Admin_PromotionCashBack_Set", p, transaction, commandType: CommandType.StoredProcedure) > 0;
        }

        public IEnumerable<PromotionCashBack1> GetPromotionCashBack(string bonusCode, int siteId,
            MySqlConnection connection = null,
            MySqlTransaction transaction = null)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);

            if (connection == null)
            {

                return _reportDbClient.Query<PromotionCashBack1>("Admin_PromotionCashBack_Get", p);
            }
            else
            {
                return connection.Query<PromotionCashBack1>("Admin_PromotionCashBack_Get", p, transaction, commandType: CommandType.StoredProcedure);
            }
        }

        private bool DuplicatePromotionCashBack(string bonusCode, int siteId, string newBonusCode,
            int expectedRow, MySqlConnection connection,
            MySqlTransaction transaction, string Operator)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);
            p.Add("_NewBonusCode", newBonusCode);
            p.Add("_Operator", Operator);
            p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("Admin_PromotionCashBackDuplicate_Set", p, transaction: transaction,
                commandType: CommandType.StoredProcedure);

            var affectCount = p.Get<int?>("_AffectCount") == null ? 0 : p.Get<int>("_AffectCount");

            return affectCount == expectedRow;
        }
        #endregion
    }
}
