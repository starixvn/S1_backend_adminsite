using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.Model.TpBetRecordDetail;
using Microsoft.Extensions.DependencyInjection;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL
{
    public class PaymentBLL
    {
        private readonly AdjustmentService adjustmentService;
        private readonly WalletService walletService;
        private readonly AdminService adminService;
        private readonly TpBetRecordService _tpBetRecordService;
        private readonly TpMaintainService _tpMaintainService;
        private readonly ThirdPartyService _thirdPartyService;

        public PaymentBLL(IServiceProvider provider)
        {
            adjustmentService = provider.GetRequiredService<AdjustmentService>();
            walletService = provider.GetRequiredService<WalletService>();
            adminService = provider.GetRequiredService<AdminService>();
            _tpBetRecordService = provider.GetRequiredService<TpBetRecordService>();
            _tpMaintainService = provider.GetRequiredService<TpMaintainService>();
            _thirdPartyService = provider.GetRequiredService<ThirdPartyService>();
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

        public async Task<BaseDetail> GetTpBetRecordDetailAsync(int gameType, string transId)
        {
            var gameTypeItem = _tpMaintainService.GetTpGameTypeList().FirstOrDefault(x => x.Type == gameType);

            var result = await _tpBetRecordService.GetTpBetRecordDetailAsync(gameTypeItem.TpId, gameType, transId);

            if (result.Any())
            {
                var data = result.FirstOrDefault();

                var tpCurrency = _thirdPartyService.GetTpCurrency(gameTypeItem.TpId, data.CurrencyId);

                return new DetailMapping().GetDetail(new GetDetailObject
                {
                    OriginalTpId = gameTypeItem.TpId,
                    OriginalGameType = gameType,
                    OriginalGameTypeName = gameTypeItem.TypeName,
                    OriginalTransId = transId,
                    TpCategoryId = gameTypeItem.CategoryId,
                    TpCurrency = await tpCurrency,
                    TpBetRecordDetail = data
                });
            }
            else
            {
                return new DetailMapping().GetDetail(new GetDetailObject
                {
                    OriginalTpId = gameTypeItem.TpId,
                    OriginalGameType = gameType,
                    OriginalGameTypeName = gameTypeItem.TypeName,
                    OriginalTransId = transId,
                    TpCategoryId = gameTypeItem.CategoryId,
                    TpCurrency = null,
                    TpBetRecordDetail = new TpBetRecordDetailModel()
                });
            }
        }
    }
}