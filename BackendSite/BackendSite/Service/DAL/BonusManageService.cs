using System.Data;

using BackendSite.ExceptionHandling;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.PromotionDisplayCondition;
using BackendSite.Service.Model.TpMaintain;

using Dapper;

using MySql.Data.MySqlClient;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BackendSite.Service.DAL
{
    public class BonusManageService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        private readonly MySqlClient _reportbackupDbClient;

        public BonusManageService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
            _reportbackupDbClient = provider.GetServices<MySqlClient>().FirstOrDefault(p => p.DataBase == DataBase.ReportBackupDB);
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
            return _reportDbClient.Query<PromotionType>("Admin_PromotionType_Get", new { isPayout, isNew });
        }

        public IEnumerable<PromoCategory> GetPromoCategories()
        {
            return _reportDbClient.Query<PromoCategory>("Admin_PromotionCategory_Get_V4");
        }

        public IEnumerable<int> GetPromotionCategoryGameTypeList(int PromotionCategoryId)
        {
            return _reportDbClient.Query<int>("Admin_PromotionCategoryGameType_Get", new { PromotionCategoryId });
        }

        public IEnumerable<CashBackGame> GetCashBackGameList(string siteIds, int currencyId, int categoryId)
        {
            return _reportDbClient.Query<CashBackGame>("Admin_CashBackGameType_Get_V3", 
                new DynamicParameters(
                    new { 
                        _SiteIds = siteIds, 
                        _CurrencyId = currencyId,
                        _PromoCategoryId = categoryId }));
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

        /// <summary>
        /// 設定bonus允許名單ID
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        public int SetBonusCustUsingInfo(int siteId, string bonusCode, string creator)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId);
            p.Add("_BonusCode", bonusCode);
            p.Add("_Creator", creator);
            p.Add("_Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

            _mainDbClient.Execute("Admin_BonusCustUsingInfo_Set", p);
            var id = p.Get<int?>("_Id") == null ? 0 : p.Get<int>("_Id");

            return id;
        }

        /// <summary>
        /// 設定bonus允許名單
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public bool SetBonusCustUsingDetail(List<BonusCustUsing> list)
        {
            var result = _mainDbClient.Execute("Admin_BonusCustUsingDetail_Set", list);

            return result > 0;
        }

        /// <summary>
        /// 取得 bonus允許名單
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BonusCustUsingList> GetBonusCustUsingDetail(int siteId, string bonusCode)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId);
            p.Add("_BonusCode", bonusCode);
            var result = _reportDbClient.Query<BonusCustUsingList>("Service_BonusCustUsingDetail_Get", p);

            return result;
        }

        #region Public Method

        /// <summary>
        /// 取得促銷資料清單
        /// </summary>
        public IEnumerable<PromotionInfo> GetPromotionList(string siteIds,
            string currencyIds,
            string promotionTypes,
            int status,
            long startDate,
            long endDate,
            int device,
            string bonusCode,
            string permissionSiteIds,
            string scratchCardCode,
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
            p.Add("_ScratchCardCode", string.IsNullOrEmpty(scratchCardCode) ? null : scratchCardCode);
            p.Add("_IsSpecial", isSpecial ? 1 : 0);
            p.Add("_StartIndex", startIndex);
            p.Add("_PageSize", pageSize);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportbackupDbClient != null
            ? _reportbackupDbClient.RetryQuery<PromotionInfo>("Admin_PromotionList_Get_V7", p)
            : _reportDbClient.Query<PromotionInfo>("Admin_PromotionList_Get_V7", p);

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
            var promotion = GetPromotion(bonusCode, siteId);

            if (promotion != null)
            {
                promotion.PromotionDisplayCondition = GetPromotionDisplayCondition(bonusCode, siteId);
                promotion.Specific = GetPromotionSpecific(bonusCode, siteId);

                if (promotion.Specific != null)
                {
                    if (promotion.PromotionType == (int)PromotionTypeEnum.Cashback)
                    {
                        var cashBackList = GetPromotionCashBack(bonusCode, siteId);
                        var tempCashBackList = cashBackList
                            .GroupBy(x => new 
                            { 
                                x.CurrencyId, 
                                x.GameType, 
                                x.LossAmount, 
                                x.Percentage, 
                                x.Status, 
                                x.TpId 
                            })
                            .Select(x =>new PromotionCashBackCurrency
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
                            if (cashBack != null)
                            {
                                specific.CashBackSetting = new CashBackSetting
                                {
                                    CashBackTpInfos = specific.CashBackList.Select(i => new CashBackTpInfo
                                    {
                                        // SBSW特殊邏輯
                                        TpId = i.TpId == (int)TpEnum.SBSW ? i.GameType : i.TpId,
                                        CategoryId = i.TpId == (int)TpEnum.SBSW ? (int)PromotionCategoryEnum.SBSW : i.GameType
                                    }),
                                    Percentage = cashBack.Percentage,
                                    LossAmount = cashBack.LossAmount
                                };
                            }
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
                        
                        if (!string.IsNullOrEmpty(specific.TurnoverTpGameTypes))
                        {
                            specific.TurnoverTpGameTypeList = JsonConvert.DeserializeObject<IEnumerable<int>>(specific.TurnoverTpGameTypes);
                        }
                        
                    }
                }
                
                if (promotion.CategoryId.HasValue)
                {
                    promotion.PromotionCategoryGameTypeList = GetPromotionCategoryGameTypeList(promotion.CategoryId.Value);
                }               
            }
            return promotion;
        }

        /// <summary>
        /// 取得促銷基本資料
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public PromotionInfo GetPromotion(string bonusCode, int siteId)
        {
            var p = new DynamicParameters();
            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);

            return _reportDbClient.QueryFirst<PromotionInfo>("Admin_Promotion_Get_V4", p);
        }

        /// <summary>
        /// 取得指定促銷規則??
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public IEnumerable<PromotionSpecific> GetPromotionSpecific(string bonusCode, int siteId)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);

            return _reportDbClient.Query<PromotionSpecific>("Admin_PromotionSpecificV3_Get", p);
        }

        /// <summary>
        /// 取得促銷顯示條件
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public IEnumerable<PromotionDisplayConditionModel> GetPromotionDisplayCondition(string bonusCode, int siteId)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);

            return _reportDbClient.Query<PromotionDisplayConditionModel>("Admin_PromotionDisplayCondition_Get", p);
        }

        /// <summary>
        /// 設定促銷流程
        /// </summary>
        /// <param name="isAdd"></param>
        /// <param name="promotion"></param>
        /// <param name="vipLevelCondition"></param>
        /// <param name="Operator"></param>
        /// <returns></returns>
        public bool SetPromotionFlow(bool isAdd, PromotionInfoReq promotion, DisplayConditionAboutVipLevel vipLevelCondition, string Operator)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            
            #region Step1:Set Promotion
            
            var setAffectCount = SetPromotion(promotion, trans);

            if (setAffectCount == 0)
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                return false;
            }

            if (isAdd && setAffectCount != 1)
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                return false;
            }
            var isUpdate = setAffectCount == 2;
            
            #endregion
            
            #region Step2:inset AdjustmentinfoLog
            var insertCount = InsertAdjustmentInfoLog(promotion, trans, Operator);
            if (insertCount == 0)
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                return false;
            }
            #endregion
            
            #region Step2-1:Set Promotion Specific
            
            var specificList = new List<PromotionSpecific>();
            foreach (var specific in promotion.Specific)
            {
                var maxBonus = specific.BonusType == (int)BonusTypeEnum.Amount ? specific.Bonus : specific.MaxBonus;
                if (promotion.PromotionType == (int)PromotionTypeEnum.SpecialBonus)
                {
                    // SpecialBonus不判斷BonusType，直接使用前端傳入的MaxBonus
                    maxBonus = specific.MaxBonus;
                }

                specificList.Add(new PromotionSpecific
                {
                    CurrencyId = specific.CurrencyId,
                    RollOver = specific.RollOver,
                    WinOver = specific.WinOver,
                    FullFillType = specific.WinOver > 0 ? (int)FullFillTypeEnum.WinOver : (int)FullFillTypeEnum.RollOver,
                    Bonus = specific.Bonus,
                    BonusType = specific.BonusType,
                    MaxBonus = maxBonus,
                    Status = specific.Status,
                    Usage = specific.Usage,
                    Details = specific.Details,
                    RequiredAmount = specific.RequiredAmount,
                    DepositCountAchieved = specific.DepositCountAchieved,
                    DepositTermPeriodDays = specific.DepositTermPeriodDays,
                    DepositConditionStatus = specific.DepositConditionStatus,
                    LockedType = specific.LockedType,
                    MinPayout = specific.MinPayout,
                    VN_AffCodes = !string.IsNullOrEmpty(specific.VN_AffCodes)
                        ? JsonConvert.SerializeObject(specific.VN_AffCodes.Split(","))
                        : null,
                    TotalUsageLimit = specific.TotalUsageLimit,
                    TierAmount = specific.TierAmountList.Any() ? JsonConvert.SerializeObject(specific.TierAmountList) : "[]",
                    TurnoverTpGameTypes = specific.TurnoverTpGameTypeList is not null ? JsonConvert.SerializeObject(specific.TurnoverTpGameTypeList) : "[]",
                });
            }
            
            if (!SetPromotionSpecific(promotion.BonusCode, promotion.SiteId, specificList, trans))
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                return false;
            }
            
            #endregion

            #region Step2-2:If Update,Delete old Promotion Specific
            if (isUpdate)
            {
                DeletePromotionSpecific(promotion.BonusCode, promotion.SiteId, specificList, trans);
            }
            #endregion
            
            #region Step3-1:Set Promotion Display Condition
            
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
                if (!SetPromotionDisplayCondition(promotion.BonusCode, promotion.SiteId, promotionDisplayConditionList, trans))
                {
                    _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                    return false;
                }
            }
            #endregion

            #region Step3-2:If Update,Delete old Promotion Display Condition
            if (isUpdate)
            {
                DeletePromotionDisplayCondition(promotion.BonusCode, promotion.SiteId, promotionDisplayConditionList, trans);
            }
            #endregion
            
            if (promotion.PromotionType == (int)PromotionTypeEnum.Cashback)
            {
                var cashBackList = new List<PromotionCashBackSetting>();

                if (!vipLevelCondition.SelectedVipLevel.Any())
                {
                    vipLevelCondition.SelectedVipLevel = new List<int>
                    {
                        (int)PromotionCashBackSettingLevelEnum.All
                    };
                }

                foreach (var specific in promotion.Specific)
                {
                    foreach (var tpInfo in specific.CashBackSetting.CashBackTpInfos)
                    {
                        var tpCategoryId = 0;
                        
                        if (tpInfo.CategoryId == (int)PromotionCategoryEnum.SBSW)
                        {
                            // SBSW的遊戲類型放在TpId內
                            tpCategoryId = tpInfo.TpId;
                        }
                        else
                        {
                            tpCategoryId = tpInfo.CategoryId switch
                            {
                                (int)PromotionCategoryEnum.SlotsGame => (int)TpCategoryEnum.Casino,
                                (int)PromotionCategoryEnum.LiveCasino => (int)TpCategoryEnum.LiveCasino,
                                (int)PromotionCategoryEnum.Fish => (int)TpCategoryEnum.Fishing,
                                (int)PromotionCategoryEnum.Lottery => (int)TpCategoryEnum.Lottery,
                                (int)PromotionCategoryEnum.ESports => (int)TpCategoryEnum.ESports,
                                _ => 0
                            };
                        }

                        vipLevelCondition.SelectedVipLevel.ToList().ForEach(vipLevel =>
                        {
                            cashBackList.Add(new PromotionCashBackSetting
                            {
                                CurrencyId = specific.CurrencyId,
                                TpId = tpInfo.CategoryId == (int)PromotionCategoryEnum.SBSW ? (int)TpEnum.SBSW : tpInfo.TpId,
                                CategoryId = tpCategoryId,
                                Level = vipLevel,
                                Percentage = specific.CashBackSetting.Percentage,
                                LossAmount = specific.CashBackSetting.LossAmount,
                                Status = (int)PromotionCashBackSettingStatusEnum.Enable
                            });
                        });
                    }
                }

                if (isUpdate && !DeletePromotionCashBack(promotion.BonusCode, promotion.SiteId, trans))
                {
                    _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                    return false;
                }

                if (!InsertPromotionCashBack(promotion.BonusCode, promotion.SiteId, Operator, cashBackList,
                        trans))
                {
                    _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                    return false;
                }
            }

            _mainDbClient.CommitAndDisposeConnection(conn, trans);
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
            using var connection = _mainDbClient.GetMySqlConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            var promotionResult = DuplicatePromotion(bonusCode, siteId, newBonusCode, newSpecialCode, isSpecial, transaction);

            if (!promotionResult)
            {
                _mainDbClient.RollbackAndDisposeConnection(connection, transaction);
                return false;
            }
            var promotionSpecificResult = DuplicatePromotionSpecific(bonusCode, siteId, newBonusCode, expectedSpecificRow, transaction);
            if (!promotionSpecificResult)
            {
                _mainDbClient.RollbackAndDisposeConnection(connection, transaction);
                return false;
            }

            if (expectedConditionRow > 0)
            {
                var promotionConditionResult = DuplicatePromotionDisplayCondition(bonusCode, siteId, newBonusCode, expectedConditionRow, transaction);
                if (!promotionConditionResult)
                {
                    _mainDbClient.RollbackAndDisposeConnection(connection, transaction);
                    return false;
                }
            }

            if (expectedCashBackRow > 0)
            {
                var promotionCashBackResult = DuplicatePromotionCashBack(bonusCode, siteId, newBonusCode, expectedCashBackRow, transaction, Operator);
                if (!promotionCashBackResult)
                {
                    _mainDbClient.RollbackAndDisposeConnection(connection, transaction);
                    return false;
                }
            }

            _mainDbClient.CommitAndDisposeConnection(connection, transaction);
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
        /// <param name="transaction"></param>
        /// <returns></returns>
        private int SetPromotion(PromotionInfoReq promotion, MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", promotion.BonusCode);
            p.Add("_Description", promotion.Description);
            p.Add("_PromotionType", promotion.PromotionType);
            p.Add("_CategoryId", promotion.CategoryId);
            p.Add("_PeriodFrom", promotion.PeriodFromTime?.ToUnixTimeSeconds());
            p.Add("_PeriodTo", promotion.PeriodToTime?.ToUnixTimeSeconds());
            p.Add("_DisplayFrom", promotion.DisplayFromTime?.ToUnixTimeSeconds() ?? 0);
            p.Add("_DisplayTo", promotion.DisplayToTime?.ToUnixTimeSeconds() ?? 0);
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
            p.Add("_IsPopUp", promotion.IsPopUp);

            //啟用限制條件，可用次數下限為1
            var _sameDeviceIDAvailableCount = promotion.SameDeviceIDRestrict && promotion.SameDeviceIDAvailableCount < 1 ? 1 : promotion.SameDeviceIDAvailableCount;
            var _sameIPRestrictAvailableCountt = promotion.SameIPRestrict && promotion.SameIPRestrictAvailableCount < 1 ? 1 : promotion.SameIPRestrictAvailableCount;
            var _sameRealNameRestrictAvailableCount = promotion.SameRealNameRestrict && promotion.SameRealNameRestrictAvailableCount < 1 ? 1 : promotion.SameRealNameRestrictAvailableCount;

            p.Add("_SameDeviceIDRestrict", promotion.SameDeviceIDRestrict);
            p.Add("_SameDeviceIDAvailableCount", _sameDeviceIDAvailableCount);
            p.Add("_SameIPRestrict", promotion.SameIPRestrict);
            p.Add("_SameIPRestrictAvailableCount", _sameIPRestrictAvailableCountt);
            p.Add("_SameRealNameRestrict", promotion.SameRealNameRestrict);
            p.Add("_SameRealNameRestrictAvailableCount", _sameRealNameRestrictAvailableCount);
            p.Add("_BlockTagList", JsonConvert.SerializeObject(promotion.BlockTagList));
            p.Add("_HideForUnlogin", promotion.HideForUnlogin);
            p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            _mainDbClient.Execute("Admin_Promotion_Set_V8", transaction, p);
            var affectCount = p.Get<int?>("_AffectCount") == null ? 0 : p.Get<int>("_AffectCount");

            return affectCount;
        }


        /// <summary>
        /// 設定促銷規則??
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <param name="specificList"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private bool SetPromotionSpecific(string bonusCode, int siteId, List<PromotionSpecific> specificList,
            MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);
            p.Add("_SpecificList", JsonConvert.SerializeObject(specificList));
            p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            _mainDbClient.Execute("Admin_PromotionSpecific_Set_V9", transaction, p);

            var affectCount = p.Get<int?>("_AffectCount") == null ? 0 : p.Get<int>("_AffectCount");

            return affectCount > 0;
        }
        
        /// <summary>
        /// 刪除促銷規則??
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <param name="specificList"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private int DeletePromotionSpecific(string bonusCode, int siteId, List<PromotionSpecific> specificList, MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);
            p.Add("_SpecificList", JsonConvert.SerializeObject(specificList));
            p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            _mainDbClient.Execute("Admin_PromotionSpecific_Del", transaction, p);

            var affectCount = p.Get<int?>("_AffectCount") == null ? 0 : p.Get<int>("_AffectCount");

            return affectCount;
        }
        
        /// <summary>
        /// 設定促銷顯示條件
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <param name="conditionData"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private bool SetPromotionDisplayCondition(string bonusCode, int siteId,
            List<PromotionDisplayConditionModel> conditionData,
            MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);
            p.Add("_ConditionList", JsonConvert.SerializeObject(conditionData));
            p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            _mainDbClient.Execute("Admin_PromotionDisplayCondition_Set", transaction, p);

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
        /// <param name="transaction"></param>
        /// <returns></returns>
        private int DeletePromotionDisplayCondition(string bonusCode, int siteId,
            List<PromotionDisplayConditionModel> conditionData,
            MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);
            p.Add("_ConditionList", JsonConvert.SerializeObject(conditionData));
            p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            _mainDbClient.Execute("Admin_PromotionDisplayCondition_Del", transaction, p);

            var affectCount = p.Get<int?>("_AffectCount") == null ? 0 : p.Get<int>("_AffectCount");

            return affectCount;
        }

        /// <summary>
        /// 複製促銷基本資料
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="siteId"></param>
        /// <param name="newBonusCode"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private bool DuplicatePromotion(string bonusCode, int siteId, string newBonusCode, string newSpecialCode, bool isSpecial,
            MySqlTransaction transaction)
        {
            var p = new DynamicParameters();
            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);
            p.Add("_NewBonusCode", newBonusCode);
            p.Add("_NewSpecialCode", newSpecialCode);
            p.Add("_IsSpecial", isSpecial ? 1 : 0);
            p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            _mainDbClient.Execute("Admin_PromotionDuplicate_Set_V3", transaction, p);

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
        /// <param name="transaction"></param>
        /// <returns></returns>
        private bool DuplicatePromotionSpecific(string bonusCode, int siteId, string newBonusCode, int expectedRow,
            MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);
            p.Add("_NewBonusCode", newBonusCode);
            p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            _mainDbClient.Execute("Admin_PromotionSpecificDuplicate_Set_V2", transaction, p);

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
        /// <param name="transaction"></param>
        /// <returns></returns>
        private bool DuplicatePromotionDisplayCondition(string bonusCode, int siteId, string newBonusCode,
            int expectedRow, MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);
            p.Add("_NewBonusCode", newBonusCode);
            p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            _mainDbClient.Execute("Admin_PromotionDisplayConditionDuplicate_Set", transaction, p);

            var affectCount = p.Get<int?>("_AffectCount") == null ? 0 : p.Get<int>("_AffectCount");

            return affectCount == expectedRow;
        }

        #endregion

        #region
        private int InsertAdjustmentInfoLog(PromotionInfoReq promotion, MySqlTransaction transaction, string Operator)
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
            return _mainDbClient.Execute("Admin_AdjustmentInfoLog_Set", transaction, propsLog);
        }
        #endregion

        #region
        private bool DeletePromotionCashBack(string bonusCode, int siteId,
            MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);

            return _mainDbClient.Execute("Admin_PromotionCashBack_Del", transaction, p) > 0;
        }

        private bool InsertPromotionCashBack(string bonusCode, int siteId, string Operator, List<PromotionCashBackSetting> cashbackList,
            MySqlTransaction transaction)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);
            p.Add("_Operator", Operator);
            p.Add("_CashBackList", JsonConvert.SerializeObject(cashbackList));

            return _mainDbClient.Execute("Admin_PromotionCashBack_Set", transaction, p) > 0;
        }

        public IEnumerable<PromotionCashBackCurrency> GetPromotionCashBack(string bonusCode, int siteId)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);

            return _reportDbClient.Query<PromotionCashBackCurrency>("Admin_PromotionCashBack_Get", p);
        }

        private bool DuplicatePromotionCashBack(string bonusCode, int siteId, string newBonusCode,
            int expectedRow, MySqlTransaction transaction, string Operator)
        {
            var p = new DynamicParameters();

            p.Add("_BonusCode", bonusCode);
            p.Add("_SiteId", siteId);
            p.Add("_NewBonusCode", newBonusCode);
            p.Add("_Operator", Operator);
            p.Add("_AffectCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            _mainDbClient.Execute("Admin_PromotionCashBackDuplicate_Set", transaction, p);

            var affectCount = p.Get<int?>("_AffectCount") == null ? 0 : p.Get<int>("_AffectCount");

            return affectCount == expectedRow;
        }
        #endregion
    }
}
