using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentBLL paymentBLL;
        private readonly CustInfoBLL custInfoBLL;
        private readonly TpBLL tpBLL;
        private readonly AuthorizeBLL authorizeBLL;
        private readonly int mainWalletType;
        private readonly Common common;
        private readonly ExcelBLL excelBLL;

        public PaymentController(IConfiguration configuration, PaymentBLL paymentBLL, CustInfoBLL custInfoBLL, TpBLL tpBLL, AuthorizeBLL authorizeBLL, Common common, ExcelBLL excelBLL)
        {
            this.paymentBLL = paymentBLL;
            this.custInfoBLL = custInfoBLL;
            this.tpBLL = tpBLL;
            this.authorizeBLL = authorizeBLL;
            mainWalletType = configuration.GetValue<int>("MainWalletType");
            this.common = common;
            this.excelBLL = excelBLL;
        }
        public class PaymentAdjustmentReq
        {
            public int CustId { get; set; }
            public int Option { get; set; }
            public decimal Amount { get; set; }
        }

        public class AdjustmentAndWithdrawalLimitReq : PaymentAdjustmentReq
        {
            public decimal WithdrawalLimitAmount { get; set; }
            public int WithdrawalLimitOption { get; set; }
            public string Memo { set; get; }
        }

        [HttpPost]
        public async Task<ApiResult> PaymentAdjustmentAsync(PaymentAdjustmentReq paymentAdjustmentReq)
        {
            ApiResult apiResult = new ApiResult();
            AdjustmentInfo adjustmentInfo = new AdjustmentInfo();
            var custInfo = custInfoBLL.GetCustInfoWithCache(paymentAdjustmentReq.CustId);
            if (custInfo != null)
            {
                adjustmentInfo.CustId = custInfo.CustId;
                adjustmentInfo.CurrencyId = custInfo.CurrencyId;
                adjustmentInfo.UserName = custInfo.UserName;
                adjustmentInfo.SiteId = custInfo.SiteId;
                adjustmentInfo.Amount = paymentAdjustmentReq.Amount;
                adjustmentInfo.AdjustType = paymentAdjustmentReq.Option;

                string transId = null;
                if (adjustmentInfo.AdjustType == 2)
                {
                    var balance = custInfoBLL.GetBalance(custInfo.CustId);
                    if (balance < adjustmentInfo.Amount)
                    {
                        if (mainWalletType == 2)
                        {
                            var deAmount = adjustmentInfo.Amount - balance;
                            TpTransfer inTpTransfer = new TpTransfer() { TpId = 1, Amount = deAmount, CustId = custInfo.CustId, direction = TpDirection.ToMain };

                            var acc = await tpBLL.TransferAsync(inTpTransfer);
                            if (acc.ErrorCode != 0 || acc.Data.Status != TransferStatus.Success)
                            {
                                apiResult.ErrorCode = 1001;
                                apiResult.Message = $"{balance.ToString("#,##0.##")}<{adjustmentInfo.Amount.ToString("#,##0.##")}";
                                return apiResult;
                            }

                            //balance = custInfoBLL.GetBalance(custInfo.CustId);
                            //if (balance < adjustmentInfo.Amount)
                            //{
                            //    apiResult.ErrorCode = 1001;
                            //    apiResult.Message = $"{balance.ToString("#,##0")}<{adjustmentInfo.Amount.ToString("#,##0")}";
                            //    return apiResult;
                            //}
                        }
                    }

                    balance = custInfoBLL.GetBalance(custInfo.CustId);

                    if (balance < adjustmentInfo.Amount)
                    {
                        apiResult.ErrorCode = 1002;
                        apiResult.Message = $"{balance.ToString("#,##0.##")} < {adjustmentInfo.Amount.ToString("#,##0.##")}";
                        return apiResult;
                    }
                }

                if (paymentBLL.PaymentAdjustment(adjustmentInfo, User.Identity.Name, out transId) > 0)
                {
                    if (mainWalletType == 2 && adjustmentInfo.AdjustType == 1)
                    {
                        var balance = custInfoBLL.GetBalance(custInfo.CustId);
                        TpTransfer inTpTransfer = new TpTransfer() { TpId = 1, Amount = balance, CustId = custInfo.CustId, direction = TpDirection.ToTp };

                        var acc = await tpBLL.TransferAsync(inTpTransfer);
                        if (acc.ErrorCode == 0 && acc.Data.Status == TransferStatus.Success)
                        {
                            apiResult.ErrorCode = 0;
                            apiResult.Message = $"Success. ID is {transId}";
                        }
                    }
                    else
                    {
                        apiResult.ErrorCode = 0;
                        apiResult.Message = $"Success. ID is {transId}";
                    }

                }
            }

            return apiResult;
        }

        public class GetFinancialLogReq
        {
            public int CustId { get; set; }
            public long? StartDate { get; set; }
            public long? EndDate { get; set; }
            public string RefTransId { get; set; }
            public int PageNumber { get; set; }
        }
        public ApiListResult<FinancialLog> GetFinancialLog(GetFinancialLogReq getFinancialLogReq)
        {
            int pageNumber = getFinancialLogReq.PageNumber;
            int totalPages = 0;
            int rowCount = 0;
            var apiListResult = new ApiListResult<FinancialLog>();
            apiListResult.Data = paymentBLL.GetFinancialLog(getFinancialLogReq.CustId, getFinancialLogReq.StartDate, getFinancialLogReq.EndDate, getFinancialLogReq.RefTransId, 10, ref pageNumber, out totalPages, out rowCount);
            apiListResult.ErrorCode = 0;
            apiListResult.PageNumber = pageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;

            return apiListResult;
        }

        public class GetBonusLogReq
        {
            public int CustId { get; set; }
            public int PromoCategoryId { get; set; }
            public long? StartDate { get; set; }
            public long? EndDate { get; set; }
            public string RefTransId { get; set; }
            public int PageNumber { get; set; }
        }
        public ApiListResult<BonusLog> GetBonusLog(GetBonusLogReq getBonusLogReq)
        {
            int pageNumber = getBonusLogReq.PageNumber;
            int totalPages = 0;
            int rowCount = 0;
            var apiListResult = new ApiListResult<BonusLog>();
            apiListResult.Data = paymentBLL.GetBonusLog(getBonusLogReq.CustId, getBonusLogReq.PromoCategoryId, getBonusLogReq.StartDate, getBonusLogReq.EndDate, getBonusLogReq.RefTransId, 10, ref pageNumber, out totalPages, out rowCount);
            apiListResult.ErrorCode = 0;
            apiListResult.PageNumber = pageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;

            return apiListResult;
        }
        public class GetRebateLogByCustReq
        {
            public int CustId { get; set; }
            public long? StartDate { get; set; }
            public long? EndDate { get; set; }
            public int PageNumber { get; set; }
        }
        public ApiListResult<RebateLog> GetRebateLogByCust(GetRebateLogByCustReq getRebateLogByCustReq)
        {
            int pageNumber = getRebateLogByCustReq.PageNumber;
            int totalPages = 0;
            int rowCount = 0;
            var apiListResult = new ApiListResult<RebateLog>();
            apiListResult.Data = paymentBLL.GetRebateLogByCust(getRebateLogByCustReq.CustId, getRebateLogByCustReq.StartDate, getRebateLogByCustReq.EndDate, 10, ref pageNumber, out totalPages, out rowCount);
            apiListResult.ErrorCode = 0;
            apiListResult.PageNumber = pageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;

            return apiListResult;
        }
        public ApiListResultCurrencyAmount<RebateLog> GetRebateLog([FromForm] IEnumerable<int> SelectSite, [FromForm] string UserName, [FromForm] int RebateType, [FromForm] long StartDate, [FromForm] long EndDate, [FromForm] int PageNumber, [FromForm] int CurrencyId, [FromForm] int BaseCurrencyId, [FromForm] decimal TotalAmountShow, [FromForm] int RebateCategoriesId)
        {
            var apiListResult = new ApiListResultCurrencyAmount<RebateLog>();
            int PageSize = 10;
            int TotalPages = 0;
            int RowCount = 0;
            List<string> CurrencyAmount = new List<string>();
            apiListResult.Data = paymentBLL.GetRebateLog(SelectSite, UserName, RebateType, StartDate, EndDate, PageSize, CurrencyId, BaseCurrencyId, TotalAmountShow, RebateCategoriesId, ref PageNumber, out TotalPages, out RowCount,ref CurrencyAmount);
            apiListResult.PageNumber = PageNumber;
            apiListResult.RowCount = RowCount;
            apiListResult.TotalPages = TotalPages;
            apiListResult.ErrorCode = 0;
            apiListResult.CurrencyAmount = CurrencyAmount;

            return apiListResult;
        }
        [HttpPost]
        public IActionResult ExportRebateLog(string SelectSite, string UserName, int RebateType, long StartDate, long EndDate, int CurrencyId, int BaseCurrencyId, decimal TotalAmountShow, int RebateCategoriesId)
        {
            var ExportData = paymentBLL.GetRebateLogExport(SelectSite, UserName, RebateType, StartDate, EndDate, CurrencyId, BaseCurrencyId, TotalAmountShow, RebateCategoriesId);
            var stream = excelBLL.Export(ExportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BonusInfo_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        public ApiListResult<RebateInfo> GetRebateInfoList([FromForm] IEnumerable<int> SelectSite, [FromForm] string UserName, [FromForm] int RebateType, [FromForm] long StartDate, [FromForm] long EndDate, [FromForm] int PageNumber)
        {
            var apiListResult = new ApiListResult<RebateInfo>();
            int PageSize = 10;
            int TotalPages = 0;
            int RowCount = 0;
            apiListResult.Data = paymentBLL.GetRebateInfoList(SelectSite, UserName, RebateType, StartDate, EndDate, PageSize, ref PageNumber, out TotalPages, out RowCount);
            apiListResult.PageNumber = PageNumber;
            apiListResult.RowCount = RowCount;
            apiListResult.TotalPages = TotalPages;
            apiListResult.ErrorCode = 0;

            return apiListResult;
        }
        [HttpPost]
        public IActionResult ExportRebateInfo(string SelectSite, string UserName, int RebateType, long StartDate, long EndDate)
        {
            var ExportData = paymentBLL.GetRebateInfoExport(SelectSite, UserName, RebateType, StartDate, EndDate);
            var stream = excelBLL.Export(ExportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BonusInfo_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }
        

        public ApiResult<IEnumerable<PromotionCategory>> GetPromotionCategory()
        {

            var apiResult = new ApiResult<IEnumerable<PromotionCategory>>();
            apiResult.ErrorCode = 0;
            apiResult.Data = paymentBLL.GetPromotionCategory();
            return apiResult;
        }
        public class CustTpBetRecordListOneWeekReq
        {
            public int CustId { get; set; }
        }
        [HttpPost]
        public ApiResult<IEnumerable<TpBetRecordListOneWeek>> GetCustTpBetRecordListOneWeek(CustTpBetRecordListOneWeekReq custTpBetRecordListOneWeekReq)
        {
            var apiResult = new ApiResult<IEnumerable<TpBetRecordListOneWeek>>();
            apiResult.ErrorCode = 0;
            apiResult.Data = paymentBLL.GetCustTpBetRecordListOneWeek(custTpBetRecordListOneWeekReq.CustId);
            return apiResult;
        }
        public class CustTpBetRecordListOneDayReq
        {
            public int CustId { get; set; }
            public string selDate { get; set; }
        }
        [HttpPost]
        public ApiResult<IEnumerable<TpBetRecordListOneDay>> GetCustTpBetRecordListOneDay(CustTpBetRecordListOneDayReq custTpBetRecordListOneDayReq)
        {
            var apiResult = new ApiResult<IEnumerable<TpBetRecordListOneDay>>();
            apiResult.ErrorCode = 0;
            apiResult.Data = paymentBLL.GetCustTpBetRecordListOneDay(custTpBetRecordListOneDayReq.CustId, custTpBetRecordListOneDayReq.selDate);
            return apiResult;
        }
        public class CustTpBetRecordListbyCategoryIdReq
        {
            public int CustId { get; set; }
            public string selDate { get; set; }
            public int CategoryId { get; set; }
        }
        [HttpPost]
        public ApiResult<IEnumerable<TpBetRecordbyCategoryId>> GetCustTpBetRecordListbyCategoryId(CustTpBetRecordListbyCategoryIdReq custTpBetRecordListbyCategoryIdReq)
        {
            var apiResult = new ApiResult<IEnumerable<TpBetRecordbyCategoryId>>();
            apiResult.ErrorCode = 0;
            apiResult.Data = paymentBLL.GetCustTpBetRecordListbyCategoryId(custTpBetRecordListbyCategoryIdReq.CustId, custTpBetRecordListbyCategoryIdReq.selDate, custTpBetRecordListbyCategoryIdReq.CategoryId);

            return apiResult;
        }
        public class CustTpBetRecordDetailListbyGameTypeReq
        {
            public int CustId { get; set; }
            public string selDate { get; set; }
            public int gameTypeId { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }

        }
        [HttpPost]
        public ApiListResult<TpBetRecordDetailbyGameType> GetCustTpBetRecordDetailListbyGameType(CustTpBetRecordDetailListbyGameTypeReq custTpBetRecordDetailListbyGameTypeReq)
        {
            var apiResult = new ApiListResult<TpBetRecordDetailbyGameType>();
            int PageSize = custTpBetRecordDetailListbyGameTypeReq.PageSize;
            int TotalPages = 0;
            int RowCount = 0;
            int PageNumber = custTpBetRecordDetailListbyGameTypeReq.PageNumber;

            apiResult.ErrorCode = 0;
            apiResult.Data = paymentBLL.GetCustTpBetRecordDetailListbyGameType(custTpBetRecordDetailListbyGameTypeReq.CustId, custTpBetRecordDetailListbyGameTypeReq.selDate, custTpBetRecordDetailListbyGameTypeReq.gameTypeId, PageSize, ref PageNumber, ref TotalPages, ref RowCount);
            apiResult.PageNumber = PageNumber;
            apiResult.RowCount = RowCount;
            apiResult.TotalPages = TotalPages;
            return apiResult;
        }
        [HttpPost]
        public async Task<ApiResult> AdjustmentAndWithdrawalLimitAsync(AdjustmentAndWithdrawalLimitReq adjustmentAndWithdrawalLimitReq)
        {
            ApiResult apiResult = new ApiResult();
            AdjustmentAndWithdrawalLimit adjustmentAndWithdrawalLimit = new AdjustmentAndWithdrawalLimit();
            try
            {
                var authData = authorizeBLL.GetAuthData();
                var custInfo = custInfoBLL.GetCustInfoWithCache(adjustmentAndWithdrawalLimitReq.CustId);
                if (custInfo != null)
                {
                    adjustmentAndWithdrawalLimit.CustId = custInfo.CustId;
                    adjustmentAndWithdrawalLimit.CurrencyId = custInfo.CurrencyId;
                    adjustmentAndWithdrawalLimit.UserName = custInfo.UserName;
                    adjustmentAndWithdrawalLimit.SiteId = custInfo.SiteId;
                    adjustmentAndWithdrawalLimit.Amount = adjustmentAndWithdrawalLimitReq.Amount;
                    adjustmentAndWithdrawalLimit.AdjustType = adjustmentAndWithdrawalLimitReq.Option;
                    adjustmentAndWithdrawalLimit.SiteName = custInfo.SiteName;
                    adjustmentAndWithdrawalLimit.Operator = authData.UserName;
                    adjustmentAndWithdrawalLimit.WithdrawalLimitAmount = adjustmentAndWithdrawalLimitReq.WithdrawalLimitAmount;
                    adjustmentAndWithdrawalLimit.WithdrawalLimitAmountAdjustType = adjustmentAndWithdrawalLimitReq.WithdrawalLimitOption;
                    adjustmentAndWithdrawalLimit.Memo = adjustmentAndWithdrawalLimitReq.Memo;

                    string transId = null;
                    if (adjustmentAndWithdrawalLimit.AdjustType == 2)
                    {
                        var balance = custInfoBLL.GetBalance(custInfo.CustId);
                        if (balance < adjustmentAndWithdrawalLimit.Amount)
                        {
                            if (mainWalletType == 2)
                            {
                                var deAmount = adjustmentAndWithdrawalLimit.Amount - balance;
                                TpTransfer inTpTransfer = new TpTransfer() { TpId = 1, Amount = deAmount, CustId = custInfo.CustId, direction = TpDirection.ToMain };

                                var acc = await tpBLL.TransferAsync(inTpTransfer);
                                if (acc.ErrorCode != 0 || acc.Data.Status != TransferStatus.Success)
                                {
                                    apiResult.ErrorCode = 1001;
                                    apiResult.Message = $"{balance.ToString("#,##0.##")}<{adjustmentAndWithdrawalLimit.Amount.ToString("#,##0.##")}";
                                    return apiResult;
                                }
                            }
                        }

                        balance = custInfoBLL.GetBalance(custInfo.CustId);

                        if (balance < adjustmentAndWithdrawalLimit.Amount)
                        {
                            apiResult.ErrorCode = 1002;
                            apiResult.Message = $"{balance.ToString("#,##0.##")} < {adjustmentAndWithdrawalLimit.Amount.ToString("#,##0.##")}";
                            return apiResult;
                        }
                    }

                    if (paymentBLL.AdjustmentAndWithdrawalLimit(adjustmentAndWithdrawalLimit, User.Identity.Name, out transId) > 0)
                    {
                        if (mainWalletType == 2 && adjustmentAndWithdrawalLimit.AdjustType == 1)
                        {
                            var balance = custInfoBLL.GetBalance(custInfo.CustId);
                            TpTransfer inTpTransfer = new TpTransfer() { TpId = 1, Amount = balance, CustId = custInfo.CustId, direction = TpDirection.ToTp };

                            var acc = await tpBLL.TransferAsync(inTpTransfer);
                            if (acc.ErrorCode == 0 && acc.Data.Status == TransferStatus.Success)
                            {
                                apiResult.ErrorCode = 0;
                                apiResult.Message = $"Success. ID is {transId}";
                            }
                        }
                        else
                        {
                            apiResult.ErrorCode = 0;
                            apiResult.Message = $"Success. ID is {transId}";
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.ToString();
            }

            return apiResult;
        }

        public class BonusCashBackInfoListDetail
        {
            public int errorCode { set; get; }
            public string custName { set; get; }
            public string massage { set; get; }
            public decimal WinlostAmountCountCalculation { set; get; }
            public decimal Percentage { set; get; }
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/BalanceAdjustmentBatch" })]
        public async Task<List<ApiResult>> BalanceAdjustmentBatch([FromForm] int option, [FromForm] int optSite, [FromForm] int withdrawalLimitOption, [FromForm] IFormFile excelUpload, [FromForm] decimal withdrawal)
        {
            var apiResult = new List<ApiResult>();
            var authData = authorizeBLL.GetAuthData();
            try
            {
                if (excelUpload.ContentType.Equals("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", StringComparison.OrdinalIgnoreCase) || excelUpload.ContentType.Equals("application/vnd.ms-excel", StringComparison.OrdinalIgnoreCase) || excelUpload.ContentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
                {
                    ExcelPackage excelPkg = new ExcelPackage(excelUpload.OpenReadStream());
                    ExcelWorksheet sheet = excelPkg.Workbook.Worksheets.First();//取得Sheet1
                    bool isLastRow = false;
                    int RowId = 2;   // 因為有標題列，所以從第2列開始讀起

                    do  // 讀取資料，直到讀到空白列為止
                    {
                        string custUsername = string.IsNullOrEmpty(sheet.Cells[RowId, 1].Text) ? null : sheet.Cells[RowId, 1].Text;

                        string sabaId = sheet.Cells[RowId, 2].Text.Trim();
                        var custId = (string.IsNullOrEmpty(sabaId)) ? null : common.TpIdToCustId(sabaId);

                        decimal.TryParse(sheet.Cells[RowId, 3].Text.Trim(), out decimal amount);
                        string memo = sheet.Cells[RowId, 4].Text.Trim();
                        if (string.IsNullOrEmpty(custUsername) && custId == null)
                        {
                            isLastRow = true;
                        }
                        else
                        {
                            var custInfoDetail = custInfoBLL.GetCustInfoDetails(custId, custUsername, optSite);
                            if (custInfoDetail == null)
                            {
                                apiResult.Add(new ApiResult() { ErrorCode = 1002, Message = $"Not fund. Username: {custUsername}. SabaId: {sabaId}." });
                            }
                            else if (custInfoDetail.Status == 1)
                            {
                                try
                                {
                                    decimal withdrawalLimitAmount = (withdrawalLimitOption == 1) ? amount * withdrawal : 0;
                                    AdjustmentAndWithdrawalLimit adjustmentAndWithdrawalLimit = new AdjustmentAndWithdrawalLimit
                                    {
                                        CustId = custInfoDetail.CustId,
                                        CurrencyId = custInfoDetail.CurrencyId,
                                        UserName = custInfoDetail.UserName,
                                        SiteId = custInfoDetail.SiteId,
                                        Amount = amount,
                                        AdjustType = option,
                                        SiteName = custInfoDetail.SiteName,
                                        Operator = authData.UserName,
                                        WithdrawalLimitAmount = withdrawalLimitAmount,
                                        WithdrawalLimitAmountAdjustType = withdrawalLimitOption,
                                        Memo = memo
                                    };

                                    string transId = null;
                                    if (adjustmentAndWithdrawalLimit.AdjustType == 2)
                                    {
                                        var balance = custInfoBLL.GetBalance(custInfoDetail.CustId);
                                        if (balance < adjustmentAndWithdrawalLimit.Amount)
                                        {
                                            if (mainWalletType == 2)
                                            {
                                                var deAmount = adjustmentAndWithdrawalLimit.Amount - balance;
                                                TpTransfer inTpTransfer = new TpTransfer() { TpId = 1, Amount = deAmount, CustId = custInfoDetail.CustId, direction = TpDirection.ToMain };

                                                var acc = await tpBLL.TransferAsync(inTpTransfer);
                                                if (acc.ErrorCode != 0 || acc.Data.Status != TransferStatus.Success)
                                                {
                                                    apiResult.Add(new ApiResult() { ErrorCode = 1001, Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. {balance.ToString("#,##0.##")} < {adjustmentAndWithdrawalLimit.Amount.ToString("#,##0.##")}" });
                                                    RowId += 1;
                                                    continue;
                                                }
                                            }
                                        }

                                        balance = custInfoBLL.GetBalance(custInfoDetail.CustId);

                                        if (balance < adjustmentAndWithdrawalLimit.Amount)
                                        {
                                            apiResult.Add(new ApiResult() { ErrorCode = 1002, Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. {balance.ToString("#,##0.##")} < {adjustmentAndWithdrawalLimit.Amount.ToString("#,##0.##")}" });
                                            RowId += 1;
                                            continue;
                                        }
                                    }

                                    if (paymentBLL.AdjustmentAndWithdrawalLimit(adjustmentAndWithdrawalLimit, authData.UserName, out transId) > 0)
                                    {
                                        if (mainWalletType == 2 && adjustmentAndWithdrawalLimit.AdjustType == 1)
                                        {
                                            var balance = custInfoBLL.GetBalance(custInfoDetail.CustId);
                                            TpTransfer inTpTransfer = new TpTransfer() { TpId = 1, Amount = balance, CustId = custInfoDetail.CustId, direction = TpDirection.ToTp };

                                            var acc = await tpBLL.TransferAsync(inTpTransfer);
                                            if (acc.ErrorCode == 0 && acc.Data.Status == TransferStatus.Success)
                                            {
                                                apiResult.Add(new ApiResult() { ErrorCode = 1, Message = $"Success. Username: {custUsername}. SabaId: {sabaId}. ID is {transId}." });
                                            }
                                        }
                                        else
                                        {
                                            apiResult.Add(new ApiResult() { ErrorCode = 1, Message = $"Success. Username: {custUsername}. SabaId: {sabaId}. ID is {transId}." });
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    apiResult.Add(new ApiResult() { ErrorCode = 999, Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. {ex}" });
                                }
                            }
                            else if (custInfoDetail.Status == 2)
                            {
                                apiResult.Add(new ApiResult() { ErrorCode = 1002, Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. Suspend Status." });
                            }
                            else if (custInfoDetail.Status == 4)
                            {
                                apiResult.Add(new ApiResult() { ErrorCode = 1002, Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. Lock Status." });
                            }
                            else
                            {
                                apiResult.Add(new ApiResult() { ErrorCode = 1002, Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. Closed Status." });
                            }
                            RowId += 1;
                        }
                    } while (!isLastRow);
                    return apiResult;
                }
                else
                {
                    apiResult.Add(new ApiResult() { ErrorCode = -1, Message = "File Type Error" });
                    return apiResult;
                }
            }
            catch (Exception ex)
            {
                apiResult.Add(new ApiResult() { ErrorCode = -1, Message = ex.ToString() });
                return apiResult;
            }
        }

        public class VipRebateInfoPara
        {
            public IEnumerable<int> SelectSite { get; set; }
            public int Currency { get; set; }
            public long StartDate { get; set; }
            public long EndDate { get; set; }
            public string User { get; set; }
            public int PageSize { get; set; }
            public int PageNumber { get; set; }
            public int CategoryId { get; set; }
            public int BaseCurrencyId { get; set; }
            public int RebateCategoriesId { get; set; }
            public string TotalAmountShow { get; set; }
        }
        [HttpPost]
        public ApiListResultCurrencyAmount<VipRebateInfo> GetVipRebateInfoList(VipRebateInfoPara para)
        {
            var apiResult = new ApiListResultCurrencyAmount<VipRebateInfo>();
            try
            {
                List<string> CurrencyAmount = new List<string>();
                var SearchResult = paymentBLL.GetVipRebateInfoList(para.SelectSite, para.Currency, para.StartDate, para.EndDate, para.User, para.PageSize, para.PageNumber,
                    para.CategoryId,para.BaseCurrencyId,para.RebateCategoriesId,para.TotalAmountShow, ref CurrencyAmount);
                apiResult.Data = SearchResult.VipRebateInfoList;
                apiResult.PageNumber = SearchResult.PageNumber;
                apiResult.TotalPages = SearchResult.TotalPages;
                apiResult.RowCount = SearchResult.RowCount;
                apiResult.ErrorCode = 0;
                apiResult.CurrencyAmount = CurrencyAmount;
            }
            catch(Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
        public class VipRebateInfoByCustIdPara
        {
            public int CustId { get; set; }
            public long? StartDate { get; set; }
            public long? EndDate { get; set; }
            public int PageNumber { get; set; }
        }
        [HttpPost]
        public ApiListResult<VipRebateInfo> GetVipRebateInfoListByCustId(VipRebateInfoByCustIdPara para)
        {
            var apiResult = new ApiListResult<VipRebateInfo>();
            try
            {
                var SearchResult = paymentBLL.GetVipRebateInfoListByCustId(para.CustId, para.StartDate, para.EndDate, 10, para.PageNumber);
                apiResult.Data = SearchResult.VipRebateInfoList;
                apiResult.PageNumber = SearchResult.PageNumber;
                apiResult.TotalPages = SearchResult.TotalPages;
                apiResult.RowCount = SearchResult.RowCount;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
        public class VipRebateInfoParaExport
        {
            public string SelectSite { get; set; }
            public int Currency { get; set; }
            public long StartDate { get; set; }
            public long EndDate { get; set; }
            public string User { get; set; }
            public int CategoryId { get; set; }
        }
        [HttpPost]
        public IActionResult GetVipRebateInfoListExport(string SelectSite, int Currency, long StartDate, long EndDate, string User, int CategoryId,int BaseCurrencyId,int RebateCategoriesId,decimal TotalAmountShow)
        {
            var SearchResult = paymentBLL.GetVipRebateInfoListExport(SelectSite, Currency, StartDate, EndDate, User, CategoryId, BaseCurrencyId, RebateCategoriesId, TotalAmountShow);
            var stream = excelBLL.Export(SearchResult.VipRebateInfoList);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"VipRebateReport_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }
        public ApiResult<IEnumerable<VipTPCategory>> GetTPCategoryListByVipRebate()
        {
            var apiResult = new ApiResult<IEnumerable<VipTPCategory>>();
            apiResult.ErrorCode = 0;
            apiResult.Data = paymentBLL.GetTPCategoryList();
            return apiResult;
        }
    }
}