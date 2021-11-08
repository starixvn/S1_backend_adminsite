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

        public IEnumerable<FinancialLog> GetFinancialLog(int CustId, long? startDate, long? endDate, string refTransId,
            int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return walletService.GetFinancialLog(CustId, startDate, endDate, refTransId, pageSize, ref pageNumber,
                out totalPages, out rowCount);
        }

        public IEnumerable<BonusLog> GetBonusLog(int CustId, int promoCategoryId, long? startDate, long? endDate,
            string refTransId, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return walletService.GetBonusLog(CustId, promoCategoryId, startDate, endDate, refTransId, pageSize,
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

        public IEnumerable<TpBetRecordListOneDayRes> GetCustTpBetRecordListOneDay(int custId, string selDate, long startDate, long endDate)
        {
            var result = new List<TpBetRecordListOneDayRes>();
            result = walletService.GetCustTpBetRecordListOneDay(custId, selDate, startDate, endDate).ToList().GroupBy(x => new
            {
                SettledDate = x.SettledDate,
                CategoryName = x.CategoryName,
                CategoryId = x.CategoryId
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

        public IEnumerable<TpBetRecordbyCategoryIdRes> GetCustTpBetRecordListbyCategoryId(int custId, string selDate,
            int categoryId, long startDate, long endDate)
        {
            var result = new List<TpBetRecordbyCategoryIdRes>();
            result = walletService.GetCustTpBetRecordListbyCategoryId(custId, selDate, categoryId, startDate, endDate).GroupBy(x => new
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
            string selDate, int gameTypeId, long startDate, long endDate, int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {
            var result = walletService.GetCustTpBetRecordDetailListbyGameType(custId, selDate, gameTypeId, startDate, endDate, PageSize,
                ref PageNumber, ref TotalPages, ref RowCount);
            result.ToList().ForEach(x =>
            {
                x.SettlementTypeName = Enum.GetName(typeof(SettlementType), x.SettlementType);
                x.TurnOver_ExcludeValidTO = x.SettlementType == (int)SettlementType.SettledForValidNotRollover ? x.ValidBet.Equals(null) ? x.TurnOver : x.ValidBet : 0;
                x.ValidBet_Valid = x.SettlementType == (int)SettlementType.SettledForValid ? x.ValidBet.Equals(null) ? x.TurnOver : x.ValidBet : 0;
            });
            return result;
        }

        public VipRebateInfoData GetVipRebateInfoList(IEnumerable<int> selectSite, int currency, long startDate,
            long endDate, string User, int pageSize, int pageNumber, int CategoryId, int BaseCurrencyId,
            ref List<string> CurrencyAmount)
        {
            return walletService.GetVipRebateInfoList(selectSite, currency, startDate, endDate, User, pageSize,
                pageNumber, CategoryId, BaseCurrencyId, ref CurrencyAmount);
        }

        public VipRebateInfoData GetVipRebateInfoListByCustId(int custId, long? startDate, long? endDate, int pageSize,
            int pageNumber)
        {
            return walletService.GetVipRebateInfoListByCustId(custId, startDate, endDate, pageSize, pageNumber);
        }

        public VipRebateInfoData GetVipRebateInfoListExport(string selectSite, int currency, long startDate,
            long endDate, string User, int CategoryId, int BaseCurrencyId, int RebateCategoriesId)
        {
            return walletService.GetVipRebateInfoListExport(selectSite, currency, startDate, endDate, User, CategoryId,
                BaseCurrencyId, RebateCategoriesId);
        }

        public IEnumerable<VipTPCategory> GetTPCategoryList()
        {
            return walletService.GetTPCategoryList();
        }

        public AdjustmentLimtData GetAdjustmentData(int SiteId, int CurrencyId)
        {
            return adjustmentService.GetAdjustmentData(SiteId, CurrencyId);
        }

        public int AddAdjustmentLimitInfo(int SiteId, int CurrencyId, double AdjustmentMax, string Operator)
        {
            return adjustmentService.AddAdjustmentLimitInfo(SiteId, CurrencyId, AdjustmentMax, Operator);
        }

        public bool CheckPassWord(string password, int SiteId)
        {
            var result = adminService.GetSpecialPasswordData(SiteId);
            if (result == null)
            {
                return true;
            }
            else
            {
                return result.Password == password;
            }
        }

        public IEnumerable<TpBetRecordByDate> GetCustTpBetRecordList(int custId, long startDate, long endDate)
        {
            var result = new List<TpBetRecordByDate>();

            result = walletService.GetCustTpBetRecordList(custId, startDate, endDate).ToList().GroupBy(x => new
            {
                SettledDate = x.SettledDate
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
        public BalanceAdjustBonusCodeRep CheckBalanceAdjustBonusCode(string BonusCode,int PromotionType, int SiteId,int CurrencyId)
        {
            BalanceAdjustBonusCodeRep result = new BalanceAdjustBonusCodeRep();
            var BalanceAdjustBonusCodeData = adjustmentService.CheckBalanceAdjustBonusCode(BonusCode, PromotionType, SiteId, CurrencyId);

            if(BalanceAdjustBonusCodeData != null)
            {
                var promoCategories = _bonusManageService.GetPromoCategories(BalanceAdjustBonusCodeData.WalletType).Where(x => x.Id == BalanceAdjustBonusCodeData.CategoryId && x.TpId == BalanceAdjustBonusCodeData.TpId)?.FirstOrDefault();
                result.HasBonusCode = true;
                result.BonusCode = BonusCode;
                result.RollOver = BalanceAdjustBonusCodeData.RollOver;
                result.Bonus = BalanceAdjustBonusCodeData.Bonus;
                result.TpId = BalanceAdjustBonusCodeData.TpId;
                result.CategoryId = BalanceAdjustBonusCodeData.CategoryId;
                result.Name = promoCategories == null ? string.Empty : promoCategories.Name;
                result.WalletType = BalanceAdjustBonusCodeData.WalletType;
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