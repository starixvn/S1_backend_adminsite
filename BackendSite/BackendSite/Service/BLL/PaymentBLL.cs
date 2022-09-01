using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Option;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class PaymentBLL
    {
        private readonly AdjustmentService adjustmentService;
        private readonly WalletService walletService;
        private readonly AdminService adminService;
        private readonly BonusManageService _bonusManageService;
        private readonly BetListBLL _betListBll;
        private readonly IEnumerable<TpGameTypeOption> _tpGameTypeList;
        private readonly BonusInfoBLL _bonusInfoBLL;
        private readonly PartitionService _partitionService;
        private readonly ICacheHelper _cacheHelper;

        public PaymentBLL(IServiceProvider provider, IConfiguration configuration)
        {
            adjustmentService = provider.GetRequiredService<AdjustmentService>();
            walletService = provider.GetRequiredService<WalletService>();
            adminService = provider.GetRequiredService<AdminService>();
            _bonusManageService = provider.GetRequiredService<BonusManageService>();
            _betListBll = provider.GetRequiredService<BetListBLL>();
            _tpGameTypeList = provider.GetRequiredService<IEnumerable<TpGameTypeOption>>();
            _bonusInfoBLL = provider.GetRequiredService<BonusInfoBLL>();
            _partitionService = provider.GetRequiredService<PartitionService>();
            _cacheHelper = provider.GetRequiredService<ICacheHelper>();
        }

        public int PaymentAdjustment(AdjustmentInfo adjustmentInfo, string Operator, out string transId)
        {
            return adjustmentService.PaymentAdjustment(adjustmentInfo, Operator, out transId);
        }

        public IEnumerable<FinancialLog> GetFinancialLog(int custId, DateTime startDate, DateTime endDate, string refTransId,
            int pageSize, int pageNumber, out IPageInfo pageInfo)
        {
            pageInfo = new PageInfo();
            var key = $"FinancialLog:{custId}:{startDate:yyyyMMddHHmmssfff}:{endDate:yyyyMMddHHmmssfff}:{refTransId}";
            var cacheTime = TimeSpan.FromMinutes(15);

            if (_cacheHelper.KeyExists(key))
            {
                //Redis Pagination
                var total = (int)_cacheHelper.GetListLength(key);
                var (startIndex, endIndex) = pageInfo.RedisPagination(total, pageNumber, pageSize);
                return _cacheHelper.GetListByRange<FinancialLog>(key, startIndex, endIndex);
            }

            //Search range table
            var parameters = new
            {
                CustId = custId,
                StartDate  = startDate,
                EndDate = endDate,
                RefTransId = string.IsNullOrEmpty(refTransId) ? null : refTransId
            };
            var data = _partitionService.MonthlyQuery<FinancialLog>(startDate, endDate, "Admin_FinancialLogList_Get_V4", parameters);
            var list = data.OrderByDescending(i => i.Id).ToList();

            // Add Cache by List      
            _ = Task.Run(() =>
            {
                _cacheHelper.InsertList(key, cacheTime, list);
            });

            var (index, count) = pageInfo.ListPagination(list.Count, pageNumber, pageSize);
            return list.GetRange(index, count);
        }

        public IEnumerable<BonusLog> GetBonusLog(int custId, int promoCategoryId, DateTime? startDate, DateTime? endDate,
            string refTransId, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return walletService.GetBonusLog(custId, promoCategoryId, startDate, endDate, refTransId, pageSize,
                ref pageNumber, out totalPages, out rowCount);
        }

        public IEnumerable<RebateLog> GetRebateLogByCust(int CustId, long? startDate, long? endDate, int pageSize,
            ref int pageNumber, out int totalPages, out int rowCount)
        {
            return walletService.GetRebateLogByCust(CustId, startDate, endDate, pageSize, ref pageNumber,
                out totalPages, out rowCount);
        }

        public IEnumerable<RebateLog> GetRebateLog(IEnumerable<int> selectSite, string userName, int rebateType,
            DateTime startDate, DateTime endDate, int pageSize, int currencyId, int baseCurrencyId, int rebateCategoriesId,
            ref int pageNumber, out int totalPages, out int rowCount, ref List<string> currencyAmount)
        {
            return walletService.GetRebateLog(selectSite, userName, rebateType, startDate, endDate, pageSize,
                currencyId, baseCurrencyId, rebateCategoriesId, ref pageNumber, out totalPages, out rowCount,
                ref currencyAmount);
        }

        public IEnumerable<RebateLog> GetRebateLogExport(string selectSite, string userName, int rebateType,
            DateTime startDate, DateTime endDate, int currencyId, int baseCurrencyId, int rebateCategoriesId)
        {
            return walletService.GetRebateLogExport(selectSite, userName, rebateType, startDate, endDate, currencyId,
                baseCurrencyId, rebateCategoriesId);
        }

        public IEnumerable<RebateInfo> GetRebateInfoByCust(int CustId)
        {
            return walletService.GetRebateInfoByCust(CustId);
        }

        public IEnumerable<RebateInfo> GetRebateInfoList(IEnumerable<int> selectSite, string userName, int rebateType,
            DateTime startDate, DateTime endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return walletService.GetRebateInfoList(selectSite, userName, rebateType, startDate, endDate, pageSize,
                ref pageNumber, out totalPages, out rowCount);
        }

        public IEnumerable<RebateInfo> GetRebateInfoExport(string selectSite, string userName, int rebateType,
            DateTime startDate, DateTime endDate)
        {
            return walletService.GetRebateInfoExport(selectSite, userName, rebateType, startDate, endDate);
        }

        public IEnumerable<PromotionCategory> GetPromotionCategory()
        {
            return walletService.GetPromotionCategory();
        }

        public int AdjustmentAndWithdrawalLimit(AdjustmentAndWithdrawalLimit adjustmentAndWithdrawalLimit,
            string Operator, out string transId)
        {
            return adjustmentService.SetAdjustmentAndWithdrawalLimit(adjustmentAndWithdrawalLimit, Operator,
                out transId);
        }

        public IEnumerable<TpBetRecordListOneWeek> GetCustTpBetRecordListOneWeek(int custId)
        {
            return walletService.GetCustTpBetRecordListOneWeek(custId);
        }

        public IEnumerable<TpBetRecordListOneDayRes> GetCustTpBetRecordListOneDay(int custId, DateTime selDate, DateTime startDate, DateTime endDate)
        {
            var dataList = _betListBll.GetTpBetRecord(custId, startDate, endDate);
            var result = dataList
                .Join(_tpGameTypeList,
                  tpBetRecord => tpBetRecord.GameType,
                  tpGameType => tpGameType.Type,
                  (tpBetRecord, tpGameType) => new TpBetRecordListOneDay
                  {
                      SettledDate = selDate,
                      CategoryName = tpGameType.CategoryName,
                      CategoryId = tpGameType.CategoryId,
                      TurnOver = tpBetRecord.Stake,
                      ValidBet = tpBetRecord.ValidBet ?? 0,
                      WinlostAmount = tpBetRecord.WinlostAmount,
                      SettlementType = tpBetRecord.SettlementType,   
                      BonusId = tpBetRecord.BonusId,
                  })
                .GroupBy(x => new
                {
                    x.SettledDate,
                    x.CategoryName,
                    x.CategoryId
                })
                .Select(x => new TpBetRecordListOneDayRes
                {
                    SettledDate = x.Key.SettledDate,
                    CategoryName = x.Key.CategoryName,
                    CategoryId = x.Key.CategoryId,
                    TotalTO = x.Where(y => y.SettlementType == (int)SettlementType.SettledForValid || y.SettlementType == (int)SettlementType.SettledForValidNotRollover).Sum(y => y.ValidBet.Equals(null) ? y.TurnOver : y.ValidBet),
                    ValidBet = x.Where(y => y.SettlementType == (int)SettlementType.SettledForValid).Sum(y => y.ValidBet.Equals(null) ? y.TurnOver : y.ValidBet),
                    InvalidBet = x.Where(y => y.SettlementType == (int)SettlementType.SettledForInvalid).Sum(y => y.ValidBet.Equals(null) ? y.TurnOver : y.ValidBet),
                    TurnOver = x.Where(y => y.SettlementType == (int)SettlementType.SettledForValidNotRollover).Sum(y => y.ValidBet.Equals(null) ? y.TurnOver : y.ValidBet),
                    RunningTickets = x.Where(y => y.SettlementType == (int)SettlementType.UnSettled).Sum(y => y.ValidBet.Equals(null) ? y.TurnOver : y.ValidBet),
                    WinLoss = x.Sum(y => y.WinlostAmount),
                    TotalTOExcludeBonus = x.Where(y => y.BonusId == 0 && (y.SettlementType == (int)SettlementType.SettledForValid || y.SettlementType == (int)SettlementType.SettledForValidNotRollover)).Sum(y => y.ValidBet.Equals(null) ? y.TurnOver : y.ValidBet),
                    WinLossExcludeBonus = x.Where(y => y.BonusId == 0).Sum(y => y.WinlostAmount),
                }).ToList();
            return result;
        }

        public IEnumerable<TpBetRecordbyCategoryIdRes> GetCustTpBetRecordListbyCategoryId(int custId, DateTime selDate,
            int categoryId, DateTime startDate, DateTime endDate)
        {
            var betList = _betListBll.GetTpBetRecord(custId, startDate, endDate);
            var result = betList
                .Join(_tpGameTypeList,
                  tpBetRecord => tpBetRecord.GameType,
                  tpGameType => tpGameType.Type,
                  (tpBetRecord, tpGameType) => new TpBetRecordbyCategoryId
                  {
                      SettledDate = selDate,
                      GameTypeName = tpGameType.TypeName,
                      GameTypeID = tpBetRecord.GameType,
                      TurnOver = tpBetRecord.Stake,
                      ValidBet = tpBetRecord.ValidBet,
                      WinlostAmount = tpBetRecord.WinlostAmount,
                      SettlementType = tpBetRecord.SettlementType,
                      CategoryId = tpGameType.CategoryId,
                  })
                .Where(i => i.CategoryId == categoryId)
                .GroupBy(x => new
                {
                    SettledDate = x.SettledDate,
                    GameTypeName = x.GameTypeName,
                    GameTypeID = x.GameTypeID
                })
                .Select(x => new TpBetRecordbyCategoryIdRes
                {
                    SettledDate = x.Key.SettledDate,
                    GameTypeName = x.Key.GameTypeName,
                    GameTypeID = x.Key.GameTypeID,
                    TotalTO = x.Where(y => y.SettlementType == (int)SettlementType.SettledForValid || y.SettlementType == (int)SettlementType.SettledForValidNotRollover).Sum(y => y.ValidBet ?? y.TurnOver),
                    ValidBet = x.Where(y => y.SettlementType == (int)SettlementType.SettledForValid).Sum(y => y.ValidBet ?? y.TurnOver),
                    InvalidBet = x.Where(y => y.SettlementType == (int)SettlementType.SettledForInvalid).Sum(y => y.ValidBet ?? y.TurnOver),
                    TurnOver = x.Where(y => y.SettlementType == (int)SettlementType.SettledForValidNotRollover).Sum(y => y.ValidBet ?? y.TurnOver),
                    RunningTickets = x.Where(y => y.SettlementType == (int)SettlementType.UnSettled).Sum(y => y.ValidBet ?? y.TurnOver),
                    WinLoss = x.Sum(y => y.WinlostAmount)
                })
                .ToList();
            return result;
        }

        public IEnumerable<TpBetRecordDetailbyGameType> GetCustTpBetRecordDetailListbyGameType(int custId,
            DateTime selDate, int gameTypeId, DateTime startDate, DateTime endDate, int pageSize, ref int pageNumber, ref int totalPages, ref int rowCount)
        {
            var dataList = _betListBll.GetTpBetRecord(custId, startDate, endDate)
                            .Where(i => i.GameType == gameTypeId);
            var bonusCodeList = _bonusInfoBLL.GetBonusCodeList(dataList.Select(i => i.BonusId));

            var result = dataList.Join(bonusCodeList,
                              tpBetRecord => tpBetRecord.BonusId,
                              bonusList => bonusList.BonusId,
                              (tpBetRecord, bonusList) => new TpBetRecordDetailbyGameType
                              {
                                  TransId = tpBetRecord.TransId,
                                  BonusId = tpBetRecord.BonusId,
                                  BonusCode = bonusList.BonusCode,
                                  TurnOver = tpBetRecord.ValidBet ?? tpBetRecord.Stake,
                                  ValidBet = tpBetRecord.ValidBet.Value,
                                  WinlostAmount = tpBetRecord.WinlostAmount,
                                  TransTime = tpBetRecord.TransTime.DateTime,
                                  SettledTime = tpBetRecord.SettledTime.DateTime,
                                  SettlementType = tpBetRecord.SettlementType,
                                  SettlementTypeName = Enum.GetName(typeof(SettlementType), tpBetRecord.SettlementType),
                                  TurnOver_ExcludeValidTO = tpBetRecord.SettlementType == (int)SettlementType.SettledForValidNotRollover ? tpBetRecord.ValidBet ?? tpBetRecord.Stake : 0,
                                  ValidBet_Valid = tpBetRecord.SettlementType == (int)SettlementType.SettledForValid ? tpBetRecord.ValidBet ?? tpBetRecord.Stake : 0,
                              }).ToList();

            IPageInfo pageInfo = new PageInfo();
            var (index, count) = pageInfo.ListPagination(result.Count, pageNumber, pageSize);
            pageNumber = pageInfo.PageNumber;
            totalPages = pageInfo.TotalPages;
            rowCount = pageInfo.RowCount;
            return result.GetRange(index, count);
        }

        public VipRebateInfoData GetVipRebateInfoList(IEnumerable<int> selectSite, int currency, DateTime startDate,
            DateTime endDate, string user, int pageSize, int pageNumber, int categoryId, int baseCurrencyId,
            ref List<string> currencyAmount)
        {
            return walletService.GetVipRebateInfoList(selectSite, currency, startDate, endDate, user, pageSize,
                pageNumber, categoryId, baseCurrencyId, ref currencyAmount);
        }

        public VipRebateInfoData GetVipRebateInfoListByCustId(int custId, DateTime? startDate, DateTime? endDate, int pageSize,
            int pageNumber)
        {
            return walletService.GetVipRebateInfoListByCustId(custId, startDate, endDate, pageSize, pageNumber);
        }

        public VipRebateInfoData GetVipRebateInfoListExport(string selectSite, int currency, DateTime startDate,
            DateTime endDate, string user, int categoryId, int baseCurrencyId)
        {
            return walletService.GetVipRebateInfoListExport(selectSite, currency, startDate, endDate, user, categoryId,
                baseCurrencyId);
        }

        public IEnumerable<VipTPCategory> GetTPCategoryList()
        {
            return walletService.GetTPCategoryList();
        }

        public IEnumerable<AdjustmentLimitData> GetAdjustmentData(IEnumerable<int> siteId, IEnumerable<int> currencyId,
            int pageNumber,int pageSize,out int totalPages,out int rowCount)
        {
            return adjustmentService.GetAdjustmentData(JsonConvert.SerializeObject(siteId),
                JsonConvert.SerializeObject(currencyId),
                (Math.Abs(pageNumber)-1)*Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages,
                out rowCount);
        }

        public bool SetAdjustmentLimitInfo(ParamsWithLog<AdjustmentLimit> input)
        {
            return adjustmentService.SetAdjustmentLimitInfo(input);
        }

        public bool CheckPassWord(string password, int siteId)
        {
            var result = adminService.GetSpecialPasswordData(siteId);
            if (result == null)
            {
                return true;
            }
            else
            {
                return result.Password == password;
            }
        }

        public IEnumerable<TpBetRecordByDate> GetCustTpBetRecordList(int custId, DateTime startDate, DateTime endDate)
        {
            var dataList = _betListBll.GetTpBetRecord(custId, startDate, endDate);
            var result = dataList
                .GroupBy(x => new { SettledDate = x.SettledTime.AddHours(8).Date })
                .Select(x => new TpBetRecordByDate
                {
                    SettledDate = x.Key.SettledDate,
                    TotalTO = x.Where(y => y.SettlementType == (int)SettlementType.SettledForValid || y.SettlementType == (int)SettlementType.SettledForValidNotRollover).Sum(y => y.ValidBet ?? y.Stake),               
                    ValidBet = x.Where(y => y.SettlementType == (int)SettlementType.SettledForValid).Sum(y => y.ValidBet ?? y.Stake),
                    InvalidBet = x.Where(y => y.SettlementType == (int)SettlementType.SettledForInvalid).Sum(y => y.ValidBet ?? y.Stake),
                    TurnOver = x.Where(y => y.SettlementType == (int)SettlementType.SettledForValidNotRollover).Sum(y => y.ValidBet ?? y.Stake),
                    RunningTickets = x.Where(y => y.SettlementType == (int)SettlementType.UnSettled).Sum(y => y.ValidBet ?? y.Stake),
                    WinLoss = x.Sum(y => y.WinlostAmount),
                    TotalTOExcludeBonus = x.Where(y => y.BonusId == 0 && (y.SettlementType == (int)SettlementType.SettledForValid || y.SettlementType == (int)SettlementType.SettledForValidNotRollover)).Sum(y => y.ValidBet ?? y.Stake),
                    WinLossExcludeBonus = x.Where(y => y.BonusId == 0).Sum(y => y.WinlostAmount),
                })
                .OrderBy(i => i.SettledDate)
                .ToList();
            return result;
        }

        public BalanceAdjustBonusCodeRep CheckBalanceAdjustBonusCode(string bonusCode,int promotionType, int siteId,int currencyId)
        {
            var result = new BalanceAdjustBonusCodeRep();
            var balanceAdjustBonusCodeData = adjustmentService.CheckBalanceAdjustBonusCode(bonusCode, promotionType, siteId, currencyId);

            if(balanceAdjustBonusCodeData != null)
            {
                var promoCategories = _bonusManageService.GetPromoCategories(balanceAdjustBonusCodeData.WalletType).Where(x => x.Id == balanceAdjustBonusCodeData.CategoryId && x.TpId == balanceAdjustBonusCodeData.TpId)?.FirstOrDefault();
                result.HasBonusCode = true;
                result.BonusCode = bonusCode;
                result.RollOver = balanceAdjustBonusCodeData.RollOver;
                result.Bonus = balanceAdjustBonusCodeData.Bonus;
                result.TpId = balanceAdjustBonusCodeData.TpId;
                result.CategoryId = balanceAdjustBonusCodeData.CategoryId;
                result.Name = promoCategories == null ? string.Empty : promoCategories.Name;
                result.WalletType = balanceAdjustBonusCodeData.WalletType;
            }
            else
            {
                result.HasBonusCode = false;
            }

            return result;
        }
        public BalanceAdjustBonusCodeBatchRep CheckBalanceAdjustBonusCodeBatch(string bonusCode, int promotionType, int siteId)
        {
            var result = new BalanceAdjustBonusCodeBatchRep();
            var detail = new List<BalanceAdjustBonusCodeBatchDetail>();
            var balanceAdjustBonusCodeBatchData = adjustmentService.CheckBalanceAdjustBonusCodeBatch(bonusCode, promotionType, siteId).ToList();

            if (balanceAdjustBonusCodeBatchData.Count != 0)
            {
                var promoCategories = _bonusManageService.GetPromoCategories(balanceAdjustBonusCodeBatchData.FirstOrDefault().WalletType)
                    .Where(x => x.Id == balanceAdjustBonusCodeBatchData.FirstOrDefault().CategoryId && x.TpId == balanceAdjustBonusCodeBatchData.FirstOrDefault().TpId)?.FirstOrDefault();

                balanceAdjustBonusCodeBatchData.ForEach(x =>
                {
                    detail.Add(new BalanceAdjustBonusCodeBatchDetail()
                    {
                        Bonus = x.Bonus,
                        RollOver = x.RollOver,
                        Currency = x.Currency
                    });
                });
                result.HasBonusCode = true;
                result.BonusCode = bonusCode;
                result.WalletType = balanceAdjustBonusCodeBatchData.FirstOrDefault().WalletType;
                result.Detail = detail;
                result.Name = promoCategories == null ? string.Empty : promoCategories.Name;
            }
            else
            {
                result.HasBonusCode = false;
            }

            return result;
        }

        public IEnumerable<CheckBalanceAdjustData> CheckRententionBonusAdjustmentLimit(string bonusCode, int siteId)
        {
            return  adjustmentService.CheckBalanceAdjustBonusCodeBatch(bonusCode, 9, siteId);
        }
        
        public AdjustmentDepositLog GetAdjustmentDepositLogByTransId(string transId)
        {
            return  adjustmentService.GetAdjustmentDepositLogByTransId(transId);
        }
    }
}