using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class PaymentBLL
    {
        private readonly AdjustmentService adjustmentService;
        private readonly WalletService walletService;
        private readonly AdminUserService adminUserService;
        private readonly Tools tools;
        private readonly AdminService adminService;

        public PaymentBLL(AdjustmentService adjustmentService, WalletService walletService, AdminUserService adminUserService, Tools tools, AdminService adminService)
        {
            this.adjustmentService = adjustmentService;
            this.walletService = walletService;
            this.adminUserService = adminUserService;
            this.tools = tools;
            this.adminService = adminService;
        }
        public int PaymentAdjustment(AdjustmentInfo adjustmentInfo, string Operator, out string transId)
        {
            return adjustmentService.PaymentAdjustment(adjustmentInfo, Operator, out transId);
        }
        public IEnumerable<FinancialLog> GetFinancialLog(int CustId, long? startDate, long? endDate, string refTransId, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return walletService.GetFinancialLog(CustId, startDate, endDate, refTransId, pageSize, ref pageNumber, out totalPages, out rowCount);
        }
        public IEnumerable<BonusLog> GetBonusLog(int CustId, int promoCategoryId, long? startDate, long? endDate, string refTransId, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return walletService.GetBonusLog(CustId, promoCategoryId, startDate, endDate, refTransId, pageSize, ref pageNumber, out totalPages, out rowCount);
        }
        public IEnumerable<RebateLog> GetRebateLogByCust(int CustId, long? startDate, long? endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return walletService.GetRebateLogByCust(CustId, startDate, endDate, pageSize, ref pageNumber, out totalPages, out rowCount);
        }
        public IEnumerable<RebateLog> GetRebateLog(IEnumerable<int> selectSite, string userName, int rebateType, long startDate, long endDate, int pageSize,int currencyId,int baseCurrencyId,int rebateCategoriesId, ref int pageNumber, out int totalPages, out int rowCount, ref List<string> CurrencyAmount)
        {
            return walletService.GetRebateLog(selectSite, userName, rebateType, startDate, endDate, pageSize, currencyId, baseCurrencyId, rebateCategoriesId, ref pageNumber, out totalPages, out rowCount, ref CurrencyAmount);
        }
        public IEnumerable<RebateLog> GetRebateLogExport(string selectSite, string userName, int rebateType, long startDate, long endDate, int currencyId, int baseCurrencyId, int rebateCategoriesId)
        {
            return walletService.GetRebateLogExport(selectSite, userName, rebateType, startDate, endDate, currencyId, baseCurrencyId, rebateCategoriesId);
        }
        public IEnumerable<RebateInfo> GetRebateInfoByCust(int CustId)
        {
            return walletService.GetRebateInfoByCust(CustId);
        }
        public IEnumerable<RebateInfo> GetRebateInfoList(IEnumerable<int> selectSite, string userName, int rebateType, long startDate, long endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return walletService.GetRebateInfoList(selectSite, userName, rebateType, startDate, endDate, pageSize, ref pageNumber, out totalPages, out rowCount);
        }
        public IEnumerable<RebateInfo> GetRebateInfoExport(string selectSite, string userName, int rebateType, long startDate, long endDate)
        {
            return walletService.GetRebateInfoExport(selectSite, userName, rebateType, startDate, endDate);
        }
        public IEnumerable<PromotionCategory> GetPromotionCategory()
        {
            return walletService.GetPromotionCategory();
        }
        public int AdjustmentAndWithdrawalLimit(AdjustmentAndWithdrawalLimit adjustmentAndWithdrawalLimit, string Operator, out string transId)
        {
            return adjustmentService.SetAdjustmentAndWithdrawalLimit(adjustmentAndWithdrawalLimit, Operator, out transId);
        }
        public IEnumerable<TpBetRecordListOneWeek> GetCustTpBetRecordListOneWeek(int custId)
        {
            return walletService.GetCustTpBetRecordListOneWeek(custId);
        }
        public IEnumerable<TpBetRecordListOneDay> GetCustTpBetRecordListOneDay(int custId, string selDate)
        {
            return walletService.GetCustTpBetRecordListOneDay(custId, selDate);
        }
        public IEnumerable<TpBetRecordbyCategoryId> GetCustTpBetRecordListbyCategoryId(int custId, string selDate, int categoryId)
        {
            return walletService.GetCustTpBetRecordListbyCategoryId(custId, selDate, categoryId);
        }
        public IEnumerable<TpBetRecordDetailbyGameType> GetCustTpBetRecordDetailListbyGameType(int custId, string selDate, int gameTypeId, int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {
            return walletService.GetCustTpBetRecordDetailListbyGameType(custId, selDate, gameTypeId, PageSize, ref PageNumber, ref TotalPages, ref RowCount);
        }
        public VipRebateInfoData GetVipRebateInfoList(IEnumerable<int> selectSite, int currency, long startDate, long endDate, string User, int pageSize, int pageNumber,int CategoryId,int BaseCurrencyId,ref List<string> CurrencyAmount)
        {
            return walletService.GetVipRebateInfoList(selectSite, currency, startDate, endDate, User, pageSize, pageNumber, CategoryId, BaseCurrencyId, ref CurrencyAmount);
        }
        public VipRebateInfoData GetVipRebateInfoListByCustId(int custId, long? startDate, long? endDate, int pageSize, int pageNumber)
        {
            return walletService.GetVipRebateInfoListByCustId(custId, startDate, endDate, pageSize, pageNumber);
        }
        public VipRebateInfoData GetVipRebateInfoListExport(string selectSite, int currency, long startDate, long endDate, string User, int CategoryId, int BaseCurrencyId, int RebateCategoriesId)
        {
            return walletService.GetVipRebateInfoListExport(selectSite, currency, startDate, endDate, User, CategoryId, BaseCurrencyId, RebateCategoriesId);
        }
        public IEnumerable<VipTPCategory> GetTPCategoryList()
        {
            return walletService.GetTPCategoryList();
        }
        public AdjustmentLimtData GetAdjustmentData(int SiteId, int CurrencyId)
        {
            return adjustmentService.GetAdjustmentData(SiteId, CurrencyId);
        }
        public int AddAdjustmentLimitInfo(int SiteId, int CurrencyId, double AdjustmentMax,string Operator)
        {
            return adjustmentService.AddAdjustmentLimitInfo(SiteId, CurrencyId, AdjustmentMax,Operator);
        }
        public bool CheckPassWord(string password, int SiteId)
        {
            var result = adminService.GetSpecialPasswordData(SiteId);
            if(result == null)
            {
                return true;
            }
            else
            {
                return result.Password != password ? false : true;
            }
        }
        public IEnumerable<TpBetRecordListOneWeek> GetCustTpBetRecordList(int custId, long startDate, long endDate)
        {
            return walletService.GetCustTpBetRecordList(custId, startDate, endDate);
        }
    }
}
