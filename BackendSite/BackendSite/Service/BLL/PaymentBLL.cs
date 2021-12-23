using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.Configuration;
using BackendSite.Service.Library;

namespace BackendSite.Service.BLL
{
    public class PaymentBLL
    {
        private readonly AdjustmentService adjustmentService;
        private readonly WalletService walletService;
        private readonly AdminService adminService;
        private readonly TpMaintainService _tpMaintainService;
        private readonly ThirdPartyService _thirdPartyService;
        private readonly BonusManageService _bonusManageService;

        public PaymentBLL(IServiceProvider provider, IConfiguration configuration)
        {
            adjustmentService = provider.GetRequiredService<AdjustmentService>();
            walletService = provider.GetRequiredService<WalletService>();
            adminService = provider.GetRequiredService<AdminService>();
            _tpMaintainService = provider.GetRequiredService<TpMaintainService>();
            _thirdPartyService = provider.GetRequiredService<ThirdPartyService>();
            _bonusManageService = provider.GetRequiredService<BonusManageService>();
        }

        public int PaymentAdjustment(AdjustmentInfo adjustmentInfo, string Operator, out string transId)
        {
            return adjustmentService.PaymentAdjustment(adjustmentInfo, Operator, out transId);
        }

        public IEnumerable<FinancialLog> GetFinancialLog(int custId, DateTime? startDate, DateTime? endDate, string refTransId,
            int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return walletService.GetFinancialLog(custId, startDate, endDate, refTransId, pageSize, ref pageNumber,
                out totalPages, out rowCount);
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
            long startDate, long endDate, int pageSize, int currencyId, int baseCurrencyId, int rebateCategoriesId,
            ref int pageNumber, out int totalPages, out int rowCount, ref List<string> CurrencyAmount)
        {
            return walletService.GetRebateLog(selectSite, userName, rebateType, startDate, endDate, pageSize,
                currencyId, baseCurrencyId, rebateCategoriesId, ref pageNumber, out totalPages, out rowCount,
                ref CurrencyAmount);
        }

        public IEnumerable<RebateLog> GetRebateLogExport(string selectSite, string userName, int rebateType,
            long startDate, long endDate, int currencyId, int baseCurrencyId, int rebateCategoriesId)
        {
            return walletService.GetRebateLogExport(selectSite, userName, rebateType, startDate, endDate, currencyId,
                baseCurrencyId, rebateCategoriesId);
        }

        public IEnumerable<RebateInfo> GetRebateInfoByCust(int CustId)
        {
            return walletService.GetRebateInfoByCust(CustId);
        }

        public IEnumerable<RebateInfo> GetRebateInfoList(IEnumerable<int> selectSite, string userName, int rebateType,
            long startDate, long endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return walletService.GetRebateInfoList(selectSite, userName, rebateType, startDate, endDate, pageSize,
                ref pageNumber, out totalPages, out rowCount);
        }

        public IEnumerable<RebateInfo> GetRebateInfoExport(string selectSite, string userName, int rebateType,
            long startDate, long endDate)
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
            var result = walletService.GetCustTpBetRecordListOneDay(custId, selDate, startDate, endDate).ToList().GroupBy(x => new
            {
                x.SettledDate,
                x.CategoryName,
                x.CategoryId
            }).Select(x => new TpBetRecordListOneDayRes
            {
                SettledDate = x.Key.SettledDate,
                CategoryName = x.Key.CategoryName,
                CategoryId = x.Key.CategoryId,
                TotalTO = x.Where(y => (y.SettlementType == (int)SettlementType.SettledForValid || y.SettlementType == (int)SettlementType.SettledForValidNotRollover)).Sum(y => (y.ValidBet.Equals(null) ? y.TurnOver : y.ValidBet)),
                ValidBet = x.Where(y => y.SettlementType == (int)SettlementType.SettledForValid).Sum(y => (y.ValidBet.Equals(null) ? y.TurnOver : y.ValidBet)),
                InvalidBet = x.Where(y => y.SettlementType == (int)SettlementType.SettledForInvalid).Sum(y => (y.ValidBet.Equals(null) ? y.TurnOver : y.ValidBet)),
                TurnOver = x.Where(y => y.SettlementType == (int)SettlementType.SettledForValidNotRollover).Sum(y => (y.ValidBet.Equals(null) ? y.TurnOver : y.ValidBet)),
                RunningTickets = x.Where(y => y.SettlementType == (int)SettlementType.UnSettled).Sum(y => (y.ValidBet.Equals(null) ? y.TurnOver : y.ValidBet)),
                WinLoss = x.Sum(y => y.WinlostAmount)
            }).ToList();
            return result;
        }

        public IEnumerable<TpBetRecordbyCategoryIdRes> GetCustTpBetRecordListbyCategoryId(int custId, DateTime selDate,
            int categoryId, DateTime startDate, DateTime endDate)
        {
            var result = walletService.GetCustTpBetRecordListbyCategoryId(custId, selDate, categoryId, startDate, endDate).GroupBy(x => new
            {
                SettledDate = x.SettledDate,
                GameTypeName = x.GameTypeName,
                GameTypeID = x.GameTypeID
            }).Select(x => new TpBetRecordbyCategoryIdRes
            {
                SettledDate = x.Key.SettledDate,
                GameTypeName = x.Key.GameTypeName,
                GameTypeID = x.Key.GameTypeID,
                TotalTO = x.Where(y => (y.SettlementType == (int)SettlementType.SettledForValid || y.SettlementType == (int)SettlementType.SettledForValidNotRollover)).Sum(y => (y.ValidBet.Equals(null) ? y.TurnOver : y.ValidBet)),
                ValidBet = x.Where(y => y.SettlementType == (int)SettlementType.SettledForValid).Sum(y => (y.ValidBet.Equals(null) ? y.TurnOver : y.ValidBet)),
                InvalidBet = x.Where(y => y.SettlementType == (int)SettlementType.SettledForInvalid).Sum(y => (y.ValidBet.Equals(null) ? y.TurnOver : y.ValidBet)),
                TurnOver = x.Where(y => y.SettlementType == (int)SettlementType.SettledForValidNotRollover).Sum(y => (y.ValidBet.Equals(null) ? y.TurnOver : y.ValidBet)),
                RunningTickets = x.Where(y => y.SettlementType == (int)SettlementType.UnSettled).Sum(y => (y.ValidBet.Equals(null) ? y.TurnOver : y.ValidBet)),
                WinLoss = x.Sum(y => y.WinlostAmount)
            }).ToList();
            return result;
        }

        public IEnumerable<TpBetRecordDetailbyGameType> GetCustTpBetRecordDetailListbyGameType(int custId,
            DateTime selDate, int gameTypeId, DateTime startDate, DateTime endDate, int pageSize, ref int pageNumber, ref int totalPages, ref int rowCount)
        {
            var result = walletService.GetCustTpBetRecordDetailListbyGameType(custId, selDate, gameTypeId, startDate, endDate, pageSize,
                ref pageNumber, ref totalPages, ref rowCount);
            result.ToList().ForEach(x =>
            {
                x.SettlementTypeName = Enum.GetName(typeof(SettlementType), x.SettlementType);
                x.TurnOver_ExcludeValidTO = x.SettlementType == (int)SettlementType.SettledForValidNotRollover ? x.ValidBet.Equals(null) ? x.TurnOver : x.ValidBet : 0;
                x.ValidBet_Valid = x.SettlementType == (int)SettlementType.SettledForValid ? x.ValidBet.Equals(null) ? x.TurnOver : x.ValidBet : 0;
            });
            return result;
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
            DateTime endDate, string user, int categoryId, int baseCurrencyId, int rebateCategoriesId)
        {
            return walletService.GetVipRebateInfoListExport(selectSite, currency, startDate, endDate, user, categoryId,
                baseCurrencyId, rebateCategoriesId);
        }

        public IEnumerable<VipTPCategory> GetTPCategoryList()
        {
            return walletService.GetTPCategoryList();
        }

        public AdjustmentLimitData GetAdjustmentData(int siteId, int currencyId)
        {
            return adjustmentService.GetAdjustmentData(siteId, currencyId);
        }

        public int AddAdjustmentLimitInfo(int SiteId, int CurrencyId, decimal AdjustmentMax, string Operator)
        {
            return adjustmentService.AddAdjustmentLimitInfo(SiteId, CurrencyId, AdjustmentMax, Operator);
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
            var result = walletService.GetCustTpBetRecordList(custId, startDate, endDate).ToList().GroupBy(x => new
            {
                SettledDate = x.SettledDate.AddHours(8).Date
            }).Select(x => new TpBetRecordByDate
            {
                SettledDate = x.Key.SettledDate,
                TotalTO = x.Where(y => (y.SettlementType == (int)SettlementType.SettledForValid || y.SettlementType == (int)SettlementType.SettledForValidNotRollover)).Sum(y => (y.ValidBet.Equals(null) ? y.Stake : y.ValidBet)),
                ValidBet = x.Where(y => y.SettlementType == (int)SettlementType.SettledForValid).Sum(y => (y.ValidBet.Equals(null) ? y.Stake : y.ValidBet)),
                InvalidBet = x.Where(y => y.SettlementType == (int)SettlementType.SettledForInvalid).Sum(y => (y.ValidBet.Equals(null) ? y.Stake : y.ValidBet)),
                TurnOver = x.Where(y => y.SettlementType == (int)SettlementType.SettledForValidNotRollover).Sum(y => (y.ValidBet.Equals(null) ? y.Stake : y.ValidBet)),
                RunningTickets = x.Where(y => y.SettlementType == (int)SettlementType.UnSettled).Sum(y => (y.ValidBet.Equals(null) ? y.Stake : y.ValidBet)),
                WinLoss = x.Sum(y => y.WinlostAmount)
            }).OrderBy(i => i.SettledDate).ToList();

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
        public BalanceAdjustBonusCodeBatchRep CheckBalanceAdjustBonusCodeBatch(string BonusCode, int PromotionType, int SiteId)
        {
            BalanceAdjustBonusCodeBatchRep result = new BalanceAdjustBonusCodeBatchRep();
            List<BalanceAdjustBonusCodeBatchDetail> detail = new List<BalanceAdjustBonusCodeBatchDetail>();
            var BalanceAdjustBonusCodeBatchData = adjustmentService.CheckBalanceAdjustBonusCodeBatch(BonusCode, PromotionType, SiteId).ToList();

            if (BalanceAdjustBonusCodeBatchData.Count != 0)
            {
                var promoCategories = _bonusManageService.GetPromoCategories(BalanceAdjustBonusCodeBatchData.FirstOrDefault().WalletType)
                    .Where(x => x.Id == BalanceAdjustBonusCodeBatchData.FirstOrDefault().CategoryId && x.TpId == BalanceAdjustBonusCodeBatchData.FirstOrDefault().TpId)?.FirstOrDefault();

                BalanceAdjustBonusCodeBatchData.ForEach(x =>
                {
                    detail.Add(new BalanceAdjustBonusCodeBatchDetail()
                    {
                        Bonus = x.Bonus,
                        RollOver = x.RollOver,
                        Currency = x.Currency
                    });
                });
                result.HasBonusCode = true;
                result.BonusCode = BonusCode;
                result.WalletType = BalanceAdjustBonusCodeBatchData.FirstOrDefault().WalletType;
                result.Detail = detail;
                result.Name = promoCategories == null ? string.Empty : promoCategories.Name;
            }
            else
            {
                result.HasBonusCode = false;
            }

            return result;
        }

        public IEnumerable<CheckBalanceAdjustData> CheckRententionBonusAdjustmentLimit(string BonusCode, int SiteId)
        {
            return  adjustmentService.CheckBalanceAdjustBonusCodeBatch(BonusCode, 9, SiteId);
        }
    }
}