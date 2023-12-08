using BackendSite.Extensions;
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
        private readonly BetListService _betListBll;
        private readonly IEnumerable<TpGameTypeOption> _tpGameTypeList;
        private readonly BonusInfoBLL _bonusInfoBLL;
        private readonly PartitionService _partitionService;
        private readonly ICacheHelper _cacheHelper;
        private readonly Common _common;

        public PaymentBLL(IServiceProvider provider, IConfiguration configuration)
        {
            adjustmentService = provider.GetRequiredService<AdjustmentService>();
            walletService = provider.GetRequiredService<WalletService>();
            adminService = provider.GetRequiredService<AdminService>();
            _bonusManageService = provider.GetRequiredService<BonusManageService>();
            _betListBll = provider.GetRequiredService<BetListService>();
            _tpGameTypeList = provider.GetRequiredService<IEnumerable<TpGameTypeOption>>();
            _bonusInfoBLL = provider.GetRequiredService<BonusInfoBLL>();
            _partitionService = provider.GetRequiredService<PartitionService>();
            _cacheHelper = provider.GetRequiredService<ICacheHelper>();
            _common = provider.GetRequiredService<Common>();
        }

        public int PaymentAdjustment(AdjustmentInfo adjustmentInfo, string Operator, out string transId)
        {
            return adjustmentService.PaymentAdjustment(adjustmentInfo, Operator, out transId);
        }

        public IEnumerable<FinancialLog> GetFinancialLog(int custId, DateTime startDate, DateTime endDate, string refTransId, int financialType,
            int pageSize, int pageNumber, out IPageInfo pageInfo, List<int> financialTypeList)
        {
            pageInfo = new PageInfo();
            var key = $"GetFinancialLog:{custId}:{startDate:yyyyMMddHHmmssfff}:{endDate:yyyyMMddHHmmssfff}";
            if (!string.IsNullOrWhiteSpace(refTransId))
            {
                key += $":{refTransId}";
            }

            if (financialType != 0 || financialTypeList.Any())
            {
                financialTypeList.Add(financialType);
                financialTypeList = financialTypeList.Distinct().ToList();
                key += $":{string.Join(":", financialTypeList.Select(i => i.ToString()))}";
            }

            var cacheTime = TimeSpan.FromMinutes(1);

            if (_cacheHelper.KeyExists(key, RedisDataBase.Shared))
            {
                //Export
                if (pageNumber == -1)
                {
                    return _cacheHelper.GetListByRange<FinancialLog>(key, 0, pageNumber, RedisDataBase.Shared);
                }

                //Redis Pagination
                var total = (int)_cacheHelper.GetListLength(key, RedisDataBase.Shared);
                var (startIndex, endIndex) = pageInfo.RedisPagination(total, pageNumber, pageSize);
                return _cacheHelper.GetListByRange<FinancialLog>(key, startIndex, endIndex, RedisDataBase.Shared);
            }

            //Search range table
            var parameters = new
            {
                CustId = custId,
                StartDate = startDate,
                EndDate = endDate,
                RefTransId = string.IsNullOrEmpty(refTransId) ? null : refTransId
            };
            var data = _partitionService.MonthlyQuery<FinancialLog>(startDate, endDate, "Service_FinancialLog_Get", parameters);

            if (financialTypeList.Any())
            {
                data = data.Where(i => financialTypeList.Contains((int)i.FinancialType));
            }

            var list = data.OrderByDescending(i => i.CreateTime).ThenByDescending(x => x.Id);

            SetTpTransferMemo(list);

            // Add Cache by List      
            _ = Task.Run(() =>
            {
                _cacheHelper.InsertList(key, cacheTime, list, RedisDataBase.Shared);
            });

            //Export
            if (pageNumber == -1 || !data.Any())
            {
                return list;
            }

            var (index, count) = pageInfo.ListPagination(list.Count(), pageNumber, pageSize);
            return list.GetRange(index, count);
        }

        private void SetTpTransferMemo(IEnumerable<FinancialLog> list)
        {
            foreach (var financialLog in list)
            {
                if (financialLog.FinancialType == FinancialType.TpTransfer)
                {
                    var tpId = financialLog.Type - 2000;
                    var tpName = _common.GetTpName(tpId);
                    financialLog.Memo = financialLog.Direction == DirectionEnum.Add ?
                        $"{tpName} Wallet -> Main Wallet" : $"Main Wallet -> {tpName} Wallet";
                }
            }
        }
        public IEnumerable<BonusLog> GetBonusLog(int custId, IEnumerable<int> promoCategoryIds, DateTime? startDate, DateTime? endDate,
            string refTransId, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return walletService.GetBonusLog(custId, JsonConvert.SerializeObject(promoCategoryIds), startDate, endDate, refTransId, pageSize,
                ref pageNumber, out totalPages, out rowCount);
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

        public IEnumerable<TpBetRecordListOneDayRes> GetCustTpBetRecordListOneDay(int custId, DateTime selDate, DateTime startDate, DateTime endDate, IEnumerable<int> gameTypes)
        {
            var dataList = _betListBll.GetTpBetRecord(custId, startDate, endDate);
            var result = dataList
                .Where(i => gameTypes.Contains(i.GameType))
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
                    SettledDate = x.Key.SettledDate.ToString("yyyy-MM-dd"),
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
            int categoryId, DateTime startDate, DateTime endDate, IEnumerable<int> gameTypes)
        {
            var betList = _betListBll.GetTpBetRecord(custId, startDate, endDate);
            var result = betList
                .Where(i => gameTypes.Contains(i.GameType))
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
                      BonusId = tpBetRecord.BonusId,
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
                    SettledDate = x.Key.SettledDate.ToString("yyyy-MM-dd"),
                    GameTypeName = x.Key.GameTypeName,
                    GameTypeID = x.Key.GameTypeID,
                    TotalTO = x.Where(y => y.SettlementType == (int)SettlementType.SettledForValid || y.SettlementType == (int)SettlementType.SettledForValidNotRollover).Sum(y => y.ValidBet ?? y.TurnOver),
                    ValidBet = x.Where(y => y.SettlementType == (int)SettlementType.SettledForValid).Sum(y => y.ValidBet ?? y.TurnOver),
                    InvalidBet = x.Where(y => y.SettlementType == (int)SettlementType.SettledForInvalid).Sum(y => y.ValidBet ?? y.TurnOver),
                    TurnOver = x.Where(y => y.SettlementType == (int)SettlementType.SettledForValidNotRollover).Sum(y => y.ValidBet ?? y.TurnOver),
                    RunningTickets = x.Where(y => y.SettlementType == (int)SettlementType.UnSettled).Sum(y => y.ValidBet ?? y.TurnOver),
                    WinLoss = x.Sum(y => y.WinlostAmount),
                    TotalTOExcludeBonus = x.Where(y => y.BonusId == 0 && (y.SettlementType == (int)SettlementType.SettledForValid || y.SettlementType == (int)SettlementType.SettledForValidNotRollover)).Sum(y => y.ValidBet ?? y.TurnOver),
                    WinLossExcludeBonus = x.Where(y => y.BonusId == 0).Sum(y => y.WinlostAmount),
                })
                .ToList();
            return result;
        }

        public IEnumerable<TpBetRecordDetailbyGameType> GetCustTpBetRecordDetailListbyGameType(int custId, int gameTypeId, DateTime startDate, DateTime endDate, int pageSize, ref int pageNumber, ref int totalPages, ref int rowCount, TimeSpan offset, bool isExport)
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
                                  TransTime = tpBetRecord.TransTime.ToOffset(offset),
                                  SettledTime = tpBetRecord.SettledTime.ToOffset(offset),
                                  SettlementType = tpBetRecord.SettlementType,
                                  SettlementTypeName = Enum.GetName(typeof(SettlementType), tpBetRecord.SettlementType),
                                  TurnOver_ExcludeValidTO = tpBetRecord.SettlementType == (int)SettlementType.SettledForValidNotRollover ? tpBetRecord.ValidBet ?? tpBetRecord.Stake : 0,
                                  ValidBet_Valid = tpBetRecord.SettlementType == (int)SettlementType.SettledForValid ? tpBetRecord.ValidBet ?? tpBetRecord.Stake : 0,
                              }).ToList();

            IPageInfo pageInfo = new PageInfo();
            if (isExport)
            {
                pageNumber = 1;
                pageSize = result.Count;
            }

            var (index, count) = pageInfo.ListPagination(result.Count, pageNumber, pageSize);
            pageNumber = pageInfo.PageNumber;
            totalPages = pageInfo.TotalPages;
            rowCount = pageInfo.RowCount;
            return result.GetRange(index, count);
        }

        public VipRebateReportResult GetVipRebateInfoList(IEnumerable<MultiSiteRequestDto> multiSiteRequest,
            DateTime startDate, DateTime endDate, string userName, IEnumerable<int> categoryIds, int baseCurrencyId, IEnumerable<VipRebatePayoutStatusEnum> payoutStatuses,
            int pageSize, int pageNumber, ref List<string> currencyAmount)
        {
            var spPayoutStatus = 2; //All
            if (payoutStatuses.Count() == 1)
                spPayoutStatus = (int)payoutStatuses.FirstOrDefault(); //reference:VipRebateStatusEnum
            return walletService.GetVipRebateInfoList(JsonConvert.SerializeObject(multiSiteRequest),
                startDate, endDate, userName, JsonConvert.SerializeObject(categoryIds),
                baseCurrencyId, spPayoutStatus,
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize), ref currencyAmount);
        }

        public VipRebateInfoData GetVipRebateInfoListByCustId(int custId, DateTime? startDate, DateTime? endDate, IEnumerable<VipRebatePayoutStatusEnum> payoutStatuses, int pageSize,
            int pageNumber)
        {
            var spPayoutStatus = 2; //All
            if (payoutStatuses.Count() == 1)
                spPayoutStatus = (int)payoutStatuses.FirstOrDefault(); //reference:VipRebateStatusEnum
            return walletService.GetVipRebateInfoListByCustId(custId, startDate, endDate, spPayoutStatus, pageSize, pageNumber);
        }

        public IEnumerable<VipRebateDetail> GetVipRebateDetail(int custId, int tpCategoryId, string refId)
        {
            return walletService.GetVipRebateDetail(custId, tpCategoryId, refId);
        }

        public VipRebateInfoData GetVipRebateInfoListExport(IEnumerable<MultiSiteRequestDto> multiSiteRequest,
            DateTime startDate, DateTime endDate, string userName, IEnumerable<int> categoryIds, int baseCurrencyId, IEnumerable<VipRebatePayoutStatusEnum> payoutStatuses)
        {
            var spPayoutStatus = 2; //All
            if (payoutStatuses.Count() == 1)
                spPayoutStatus = (int)payoutStatuses.FirstOrDefault(); //reference:VipRebateStatusEnum
            return walletService.GetVipRebateInfoListExport(JsonConvert.SerializeObject(multiSiteRequest),
                startDate, endDate, userName, JsonConvert.SerializeObject(categoryIds),
                baseCurrencyId, spPayoutStatus);
        }

        public IEnumerable<AdjustmentLimitData> GetAdjustmentData(IEnumerable<int> siteId, IEnumerable<int> currencyId,
            int pageNumber, int pageSize, out int totalPages, out int rowCount)
        {
            return adjustmentService.GetAdjustmentData(JsonConvert.SerializeObject(siteId),
                JsonConvert.SerializeObject(currencyId),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
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

        public IEnumerable<TpBetRecordByDate> GetCustTpBetRecordList(int custId, DateTime startDate, DateTime endDate, IEnumerable<int> gameTypes, TimeSpan offset)
        {
            var dataList = _betListBll.GetTpBetRecord(custId, startDate, endDate);
            var result = dataList
                .Where(i => gameTypes.Contains(i.GameType))
                .GroupBy(x => new { SettledDate = x.SettledTime.ToOffset(offset).Date })
                .Select(x => new TpBetRecordByDate
                {
                    SettledDate = x.Key.SettledDate.ToString("yyyy-MM-dd"),
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

        public BalanceAdjustBonusCodeRep CheckBalanceAdjustBonusCode(string bonusCode, int promotionType, int siteId, int currencyId)
        {
            var result = new BalanceAdjustBonusCodeRep();
            var balanceAdjustBonusCodeData = adjustmentService.CheckBalanceAdjustBonusCode(bonusCode, promotionType, siteId, currencyId);

            if (balanceAdjustBonusCodeData != null)
            {
                var promoCategories = _bonusManageService.GetPromoCategories().FirstOrDefault(x => x.Id == balanceAdjustBonusCodeData.CategoryId);
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
                var promoCategories = _bonusManageService.GetPromoCategories()
                    .FirstOrDefault(x => x.Id == balanceAdjustBonusCodeBatchData.FirstOrDefault()?.CategoryId);

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
            return adjustmentService.CheckBalanceAdjustBonusCodeBatch(bonusCode, 9, siteId);
        }

        public AdjustmentDepositLog GetAdjustmentDepositLogByTransId(string transId)
        {
            return adjustmentService.GetAdjustmentDepositLogByTransId(transId);
        }

        public CheckPayoutBonusCodeRep CheckPayoutBonusCode(string bonusCode, int siteId, int currencyId)
        {
            var checkData = adjustmentService.CheckPayoutBonusCode(bonusCode, siteId, currencyId);
            if (checkData == null)
            {
                return new CheckPayoutBonusCodeRep
                {
                    HasBonusCode = false
                };
            }

            var promoCategories = _bonusManageService.GetPromoCategories().FirstOrDefault(x => x.Id == checkData.CategoryId );
            return new CheckPayoutBonusCodeRep
            {
                HasBonusCode = true,
                BonusCode = bonusCode,
                RollOver = checkData.RollOver,
                WinOver = checkData.WinOver,
                Bonus = checkData.Bonus,
                TpId = checkData.TpId,
                CategoryId = checkData.CategoryId,
                Name = promoCategories?.Name ?? string.Empty,
                WalletType = checkData.WalletType,
                TierAmountList = checkData.TierAmountList,
                BonusType = (int)checkData.BonusType,
                PromotionType = checkData.PromotionType,
            };
        }
    }
}