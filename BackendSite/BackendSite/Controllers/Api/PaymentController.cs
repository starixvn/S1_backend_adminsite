using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class PaymentController : ControllerBase
    {
        #region Fields

        private readonly PaymentBLL _paymentBLL;
        private readonly CustInfoBLL _custInfoBLL;
        private readonly TpBLL _tpBLL;
        private readonly AuthorizeBLL _authorizeBLL;
        private readonly Common _common;
        private readonly ExcelBLL _excelBLL;
        private readonly SelLangBLL _selLangBLL;

        private readonly int _mainWalletType;
        private readonly string _bonusApiUrl;
        private readonly CourierAdapter courierAdapter;
        #endregion

        #region Constructors

        public PaymentController(IConfiguration configuration,
            PaymentBLL paymentBLL,
            CustInfoBLL custInfoBLL,
            TpBLL tpBLL,
            AuthorizeBLL authorizeBLL,
            Common common,
            ExcelBLL excelBLL,
            SelLangBLL selLangBLL,
            CourierAdapter courierAdapter)
        {
            _paymentBLL = paymentBLL;
            _custInfoBLL = custInfoBLL;
            _tpBLL = tpBLL;
            _authorizeBLL = authorizeBLL;
            _common = common;
            _excelBLL = excelBLL;
            _selLangBLL = selLangBLL;

            _mainWalletType = configuration.GetValue<int>("MainWalletType"); //1=主錢包
            _bonusApiUrl = configuration.GetValue<string>("BonusService");
            this.courierAdapter = courierAdapter;
        }

        #endregion

        #region API Model

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
            public string BounsCode { get; set; }
            public int RollOver { get; set; }
        }

        public class GetFinancialLogReq
        {
            public int CustId { get; set; }
            public long? StartDate { get; set; }
            public long? EndDate { get; set; }
            public string RefTransId { get; set; }
            public int PageNumber { get; set; }
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

        public class GetRebateLogByCustReq
        {
            public int CustId { get; set; }
            public long? StartDate { get; set; }
            public long? EndDate { get; set; }
            public int PageNumber { get; set; }
        }

        public class CustTpBetRecordListOneWeekReq
        {
            public int CustId { get; set; }
        }

        public class CustTpBetRecordListOneDayReq
        {
            public int CustId { get; set; }
            public string SelDate { get; set; }
            public long StartDate { get; set; }
            public long EndDate { get; set; }
        }

        public class CustTpBetRecordListbyCategoryIdReq
        {
            public int CustId { get; set; }
            public string SelDate { get; set; }
            public int CategoryId { get; set; }
            public long StartDate { get; set; }
            public long EndDate { get; set; }
        }

        public class CustTpBetRecordDetailListbyGameTypeReq
        {
            public int CustId { get; set; }
            public string SelDate { get; set; }
            public int GameTypeId { get; set; }
            public long StartDate { get; set; }
            public long EndDate { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }

        public class BonusCashBackInfoListDetail
        {
            public int errorCode { set; get; }
            public string custName { set; get; }
            public string massage { set; get; }
            public decimal WinlostAmountCountCalculation { set; get; }
            public decimal Percentage { set; get; }
        }

        public class VipRebateInfoByCustIdPara
        {
            public int CustId { get; set; }
            public long? StartDate { get; set; }
            public long? EndDate { get; set; }
            public int PageNumber { get; set; }
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

        public class CustTpBetRecordListReq
        {
            public int CustId { get; set; }
            public long StartDate { get; set; }
            public long EndDate { get; set; }
        }

        #endregion

        #region Actions

        [HttpPost]
        public async Task<ApiResult> PaymentAdjustmentAsync(PaymentAdjustmentReq paymentAdjustmentReq)
        {
            var apiResult = new ApiResult();
            var adjustmentInfo = new AdjustmentInfo();
            var custInfo = _custInfoBLL.GetCustInfoWithCache(paymentAdjustmentReq.CustId);
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
                    var balance = _custInfoBLL.GetBalance(custInfo.CustId);
                    if (balance < adjustmentInfo.Amount)
                    {
                        if (_mainWalletType == 2)
                        {
                            var deAmount = adjustmentInfo.Amount - balance;
                            var inTpTransfer = new TpTransfer()
                            { TpId = 1, Amount = deAmount, CustId = custInfo.CustId, direction = TpDirection.ToMain };

                            var acc = await _tpBLL.TransferAsync(inTpTransfer);
                            if (acc.ErrorCode != 0 || acc.Data.Status != TransferStatus.Success)
                            {
                                apiResult.ErrorCode = 1001;
                                apiResult.Message =
                                    $"{balance.ToString("#,##0.##")}<{adjustmentInfo.Amount.ToString("#,##0.##")}";
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

                    balance = _custInfoBLL.GetBalance(custInfo.CustId);

                    if (balance < adjustmentInfo.Amount)
                    {
                        apiResult.ErrorCode = 1002;
                        apiResult.Message =
                            $"{balance.ToString("#,##0.##")} < {adjustmentInfo.Amount.ToString("#,##0.##")}";
                        return apiResult;
                    }
                }

                if (_paymentBLL.PaymentAdjustment(adjustmentInfo, User.Identity.Name, out transId) > 0)
                {
                    if (_mainWalletType == 2 && adjustmentInfo.AdjustType == 1)
                    {
                        var balance = _custInfoBLL.GetBalance(custInfo.CustId);
                        var inTpTransfer = new TpTransfer()
                        { TpId = 1, Amount = balance, CustId = custInfo.CustId, direction = TpDirection.ToTp };

                        var acc = await _tpBLL.TransferAsync(inTpTransfer);
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

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Use：
        /// CSD > Customer Administration > CustInfo > Financial Log
        /// </remarks>
        /// <param name="getFinancialLogReq"></param>
        /// <returns></returns>
        public ApiListResult<FinancialLog> GetFinancialLog(GetFinancialLogReq getFinancialLogReq)
        {
            var pageNumber = getFinancialLogReq.PageNumber;
            var totalPages = 0;
            var rowCount = 0;
            var data = _paymentBLL.GetFinancialLog(getFinancialLogReq.CustId, getFinancialLogReq.StartDate,
                getFinancialLogReq.EndDate, getFinancialLogReq.RefTransId, 10, ref pageNumber, out totalPages,
                out rowCount);


            return new ApiListResult<FinancialLog>
            {
                Data = data,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                RowCount = rowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Use：
        /// CSD > Customer Administration > CustInfo > Bonus Log
        /// </remarks>
        /// <param name="getBonusLogReq"></param>
        /// <returns></returns>
        public ApiResult GetBonusLog(GetBonusLogReq getBonusLogReq)
        {
            var pageNumber = getBonusLogReq.PageNumber;
            var totalPages = 0;
            var rowCount = 0;
            var data = _paymentBLL.GetBonusLog(getBonusLogReq.CustId,
                getBonusLogReq.PromoCategoryId,
                getBonusLogReq.StartDate,
                getBonusLogReq.EndDate,
                getBonusLogReq.RefTransId,
                10,
                ref pageNumber,
                out totalPages,
                out rowCount);

            return new ApiListResult<BonusLog>
            {
                Data = data,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                RowCount = rowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        public ApiListResult<RebateLog> GetRebateLogByCust(GetRebateLogByCustReq getRebateLogByCustReq)
        {
            var pageNumber = getRebateLogByCustReq.PageNumber;
            var totalPages = 0;
            var rowCount = 0;
            var apiListResult = new ApiListResult<RebateLog>
            {
                Data = _paymentBLL.GetRebateLogByCust(getRebateLogByCustReq.CustId,
                    getRebateLogByCustReq.StartDate, getRebateLogByCustReq.EndDate, 10, ref pageNumber, out totalPages,
                    out rowCount),
                ErrorCode = 0,
                PageNumber = pageNumber,
                TotalPages = totalPages,
                RowCount = rowCount
            };

            return apiListResult;
        }

        public ApiListResultCurrencyAmount<RebateLog> GetRebateLog([FromForm] IEnumerable<int> SelectSite,
            [FromForm] string UserName, [FromForm] int RebateType, [FromForm] long StartDate, [FromForm] long EndDate,
            [FromForm] int PageNumber, [FromForm] int CurrencyId, [FromForm] int BaseCurrencyId,
            [FromForm] int RebateCategoriesId)
        {
            var apiListResult = new ApiListResultCurrencyAmount<RebateLog>();
            var PageSize = 10;
            var TotalPages = 0;
            var RowCount = 0;
            var CurrencyAmount = new List<string>();
            apiListResult.Data = _paymentBLL.GetRebateLog(SelectSite, UserName, RebateType, StartDate, EndDate,
                PageSize,
                CurrencyId, BaseCurrencyId, RebateCategoriesId, ref PageNumber, out TotalPages, out RowCount,
                ref CurrencyAmount);
            apiListResult.PageNumber = PageNumber;
            apiListResult.RowCount = RowCount;
            apiListResult.TotalPages = TotalPages;
            apiListResult.ErrorCode = 0;
            apiListResult.CurrencyAmount = CurrencyAmount;

            return apiListResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Use：
        /// CSD > Customer Administration > CustInfo > Bonus Log [Promotion Category]
        /// </remarks>
        /// <returns></returns>
        public ApiResult<IEnumerable<PromotionCategory>> GetPromotionCategory()
        {
            var data = _paymentBLL.GetPromotionCategory();
            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<PromotionCategory>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        [HttpPost]
        public IActionResult ExportRebateLog(string SelectSite, string UserName, int RebateType, long StartDate,
            long EndDate, int CurrencyId, int BaseCurrencyId, int RebateCategoriesId)
        {
            var ExportData = _paymentBLL.GetRebateLogExport(SelectSite, UserName, RebateType, StartDate, EndDate,
                CurrencyId, BaseCurrencyId, RebateCategoriesId);
            var stream = _excelBLL.Export(ExportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BonusInfo_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        public ApiListResult<RebateInfo> GetRebateInfoList([FromForm] IEnumerable<int> SelectSite,
            [FromForm] string UserName, [FromForm] int RebateType, [FromForm] long StartDate, [FromForm] long EndDate,
            [FromForm] int PageNumber)
        {
            var apiListResult = new ApiListResult<RebateInfo>();
            var PageSize = 10;
            var TotalPages = 0;
            var RowCount = 0;
            apiListResult.Data = _paymentBLL.GetRebateInfoList(SelectSite, UserName, RebateType, StartDate, EndDate,
                PageSize, ref PageNumber, out TotalPages, out RowCount);
            apiListResult.PageNumber = PageNumber;
            apiListResult.RowCount = RowCount;
            apiListResult.TotalPages = TotalPages;
            apiListResult.ErrorCode = 0;

            return apiListResult;
        }

        [HttpPost]
        public IActionResult ExportRebateInfo(string SelectSite, string UserName, int RebateType, long StartDate,
            long EndDate)
        {
            var ExportData = _paymentBLL.GetRebateInfoExport(SelectSite, UserName, RebateType, StartDate, EndDate);
            var stream = _excelBLL.Export(ExportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BonusInfo_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Use：
        /// CSD > Customer Administration > CustInfo > Statement
        /// </remarks>
        /// <param name="custTpBetRecordListOneWeekReq"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<TpBetRecordListOneWeek>> GetCustTpBetRecordListOneWeek(
            CustTpBetRecordListOneWeekReq custTpBetRecordListOneWeekReq)
        {
            var apiResult = new ApiResult<IEnumerable<TpBetRecordListOneWeek>>
            {
                ErrorCode = 0,
                Data = _paymentBLL.GetCustTpBetRecordListOneWeek(custTpBetRecordListOneWeekReq.CustId)
            };
            return apiResult;
        }

        [HttpPost]
        public ApiResult<IEnumerable<TpBetRecordByDate>> GetCustTpBetRecordList(
            CustTpBetRecordListReq custTpBetRecordListReq)
        {
            var apiResult = new ApiResult<IEnumerable<TpBetRecordByDate>>
            {
                Data = _paymentBLL.GetCustTpBetRecordList(custTpBetRecordListReq.CustId,
                    custTpBetRecordListReq.StartDate, custTpBetRecordListReq.EndDate),
                ErrorCode = 0
            };
            return apiResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Use：
        /// CSD > Customer Administration > CustInfo > Statement
        /// </remarks>
        /// <param name="custTpBetRecordListOneDayReq"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<TpBetRecordListOneDayRes>> GetCustTpBetRecordListOneDay(
            CustTpBetRecordListOneDayReq custTpBetRecordListOneDayReq)
        {
            var apiResult = new ApiResult<IEnumerable<TpBetRecordListOneDayRes>>
            {
                ErrorCode = 0,
                Data = _paymentBLL.GetCustTpBetRecordListOneDay(custTpBetRecordListOneDayReq.CustId,
                    custTpBetRecordListOneDayReq.SelDate, custTpBetRecordListOneDayReq.StartDate, custTpBetRecordListOneDayReq.EndDate)
            };
            return apiResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Use：
        /// CSD > Customer Administration > CustInfo > Statement
        /// </remarks>
        /// <param name="custTpBetRecordListbyCategoryIdReq"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<TpBetRecordbyCategoryIdRes>> GetCustTpBetRecordListbyCategoryId(
            CustTpBetRecordListbyCategoryIdReq custTpBetRecordListbyCategoryIdReq)
        {
            var apiResult = new ApiResult<IEnumerable<TpBetRecordbyCategoryIdRes>>
            {
                ErrorCode = 0,
                Data = _paymentBLL.GetCustTpBetRecordListbyCategoryId(custTpBetRecordListbyCategoryIdReq.CustId,
                    custTpBetRecordListbyCategoryIdReq.SelDate,
                    custTpBetRecordListbyCategoryIdReq.CategoryId, custTpBetRecordListbyCategoryIdReq.StartDate, custTpBetRecordListbyCategoryIdReq.EndDate)
            };
            return apiResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Use：
        /// CSD > Customer Administration > CustInfo > Statement
        /// </remarks>
        /// <param name="custTpBetRecordDetailListbyGameTypeReq"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<TpBetRecordDetailbyGameType> GetCustTpBetRecordDetailListbyGameType(
            CustTpBetRecordDetailListbyGameTypeReq custTpBetRecordDetailListbyGameTypeReq)
        {
            var apiResult = new ApiListResult<TpBetRecordDetailbyGameType>();
            var PageSize = custTpBetRecordDetailListbyGameTypeReq.PageSize;
            var TotalPages = 0;
            var RowCount = 0;
            var PageNumber = custTpBetRecordDetailListbyGameTypeReq.PageNumber;

            apiResult.ErrorCode = 0;
            apiResult.Data = _paymentBLL.GetCustTpBetRecordDetailListbyGameType(
                custTpBetRecordDetailListbyGameTypeReq.CustId,
                custTpBetRecordDetailListbyGameTypeReq.SelDate,
                custTpBetRecordDetailListbyGameTypeReq.GameTypeId,
                custTpBetRecordDetailListbyGameTypeReq.StartDate,
                custTpBetRecordDetailListbyGameTypeReq.EndDate,
                PageSize,
                ref PageNumber,
                ref TotalPages,
                ref RowCount);
            apiResult.PageNumber = PageNumber;
            apiResult.RowCount = RowCount;
            apiResult.TotalPages = TotalPages;
            return apiResult;
        }

        [HttpPost]
        public async Task<ApiResult> AdjustmentAndWithdrawalLimitAsync(
            AdjustmentAndWithdrawalLimitReq adjustmentAndWithdrawalLimitReq)
        {
            var apiResult = new ApiResult();
            var adjustmentAndWithdrawalLimit = new AdjustmentAndWithdrawalLimit();
            string bonusUrl = $"{_bonusApiUrl}SetAdjustBonus";
            try
            {
                var authData = _authorizeBLL.GetAuthData();
                var custInfo = _custInfoBLL.GetCustInfoWithCache(adjustmentAndWithdrawalLimitReq.CustId);
                var balance = _custInfoBLL.GetBalance(custInfo.CustId);

                if (custInfo != null)
                {
                    //非RetentionBonus,ManualBonus
                    if (adjustmentAndWithdrawalLimitReq.Option != 5 && adjustmentAndWithdrawalLimitReq.Option != 6)
                    {
                        adjustmentAndWithdrawalLimit.CustId = custInfo.CustId;
                        adjustmentAndWithdrawalLimit.CurrencyId = custInfo.CurrencyId;
                        adjustmentAndWithdrawalLimit.UserName = custInfo.UserName;
                        adjustmentAndWithdrawalLimit.SiteId = custInfo.SiteId;
                        adjustmentAndWithdrawalLimit.Amount = adjustmentAndWithdrawalLimitReq.Amount;
                        adjustmentAndWithdrawalLimit.AdjustType = adjustmentAndWithdrawalLimitReq.Option;
                        adjustmentAndWithdrawalLimit.SiteName = custInfo.SiteName;
                        adjustmentAndWithdrawalLimit.Operator = authData.UserName;
                        adjustmentAndWithdrawalLimit.WithdrawalLimitAmount =
                            adjustmentAndWithdrawalLimitReq.WithdrawalLimitAmount;
                        adjustmentAndWithdrawalLimit.WithdrawalLimitAmountAdjustType =
                            adjustmentAndWithdrawalLimitReq.WithdrawalLimitOption;
                        adjustmentAndWithdrawalLimit.Memo = adjustmentAndWithdrawalLimitReq.Memo;

                        string transId = null;

                        switch (adjustmentAndWithdrawalLimit.AdjustType)
                        {
                            case 2:  //Balance(-)
                                {
                                    if (balance < adjustmentAndWithdrawalLimit.Amount)
                                    {
                                        if (_mainWalletType == 2)
                                        {
                                            var deAmount = adjustmentAndWithdrawalLimit.Amount - balance;
                                            var inTpTransfer = new TpTransfer()
                                            {
                                                TpId = 1,
                                                Amount = deAmount,
                                                CustId = custInfo.CustId,
                                                direction = TpDirection.ToMain
                                            };

                                            var acc = await _tpBLL.TransferAsync(inTpTransfer);
                                            if (acc.ErrorCode != 0 || acc.Data.Status != TransferStatus.Success)
                                            {
                                                apiResult.ErrorCode = 1001;
                                                apiResult.Message =
                                                    $"{balance.ToString("#,##0.##")} < {adjustmentAndWithdrawalLimit.Amount.ToString("#,##0.##")}";
                                                return apiResult;
                                            }
                                        }
                                    }


                                    if (balance < adjustmentAndWithdrawalLimit.Amount)
                                    {
                                        apiResult.ErrorCode = 1002;
                                        apiResult.Message =
                                            $"{balance.ToString("#,##0.##")} < {adjustmentAndWithdrawalLimit.Amount.ToString("#,##0.##")}";
                                        return apiResult;
                                    }

                                    if (_mainWalletType == 1 && (balance - adjustmentAndWithdrawalLimit.Amount) < 0)
                                    {
                                        apiResult.ErrorCode = 1002;
                                        apiResult.Message = $"{balance.ToString("#,##0.##")} + {adjustmentAndWithdrawalLimit.Amount.ToString("#,##0.##")} < 0. Reject";
                                        return apiResult;
                                    }
                                }
                                break;
                            case 1:  //Balance(+)
                            case 3:  //Bonus(+)
                                {
                                    if (_mainWalletType == 1 && (balance + adjustmentAndWithdrawalLimit.Amount) < 0)
                                    {
                                        apiResult.ErrorCode = 1002;
                                        apiResult.Message = $"{balance.ToString("#,##0.##")} + {adjustmentAndWithdrawalLimit.Amount.ToString("#,##0.##")} < 0. Reject";
                                        return apiResult;
                                    }
                                }
                                break;
                            case 4:  //Bonus(-)
                                {
                                    if (_mainWalletType == 1 && (balance - adjustmentAndWithdrawalLimit.Amount) < 0)
                                    {
                                        apiResult.ErrorCode = 1002;
                                        apiResult.Message = $"{balance.ToString("#,##0.##")} - {adjustmentAndWithdrawalLimit.Amount.ToString("#,##0.##")} < 0. Reject";
                                        return apiResult;
                                    }
                                }
                                break;

                        }

                        if (_paymentBLL.AdjustmentAndWithdrawalLimit(adjustmentAndWithdrawalLimit, User.Identity.Name,
                            out transId) > 0)
                        {
                            if (_mainWalletType == 2 && adjustmentAndWithdrawalLimit.AdjustType == 1)
                            {
                                var inTpTransfer = new TpTransfer()
                                { TpId = 1, Amount = balance, CustId = custInfo.CustId, direction = TpDirection.ToTp };

                                var acc = await _tpBLL.TransferAsync(inTpTransfer);
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
                    else
                    {
                        var setAdjustBonusReq = new SetAdjustBonusReq()
                        {
                            CustId = custInfo.CustId,
                            BonusCode = adjustmentAndWithdrawalLimitReq.BounsCode,
                            Memo = adjustmentAndWithdrawalLimitReq.Memo,
                            Operator = authData.UserName,
                            Amount = adjustmentAndWithdrawalLimitReq.Amount,
                            RollOver = adjustmentAndWithdrawalLimitReq.RollOver,
                            PromotionType = adjustmentAndWithdrawalLimitReq.Option == 5 ?  9 : 8
                        };
                        var apiOnlineResult =
                            await courierAdapter.MyPostAsync<string, SetAdjustBonusReq>(bonusUrl, setAdjustBonusReq);
                        if (apiOnlineResult.ErrorCode == 0)
                        {
                            apiResult.ErrorCode = apiOnlineResult.ErrorCode;
                            apiResult.Message = $"Success. ID is {apiOnlineResult.Data}";
                        }
                        else
                        {
                            apiResult.ErrorCode = apiOnlineResult.ErrorCode;
                            apiResult.Message = apiOnlineResult.Message;
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

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/BalanceAdjustmentBatch" })]
        public async Task<List<ApiResult>> BalanceAdjustmentBatch([FromForm] int option, [FromForm] int optSite,
            [FromForm] int withdrawalLimitOption, [FromForm] IFormFile excelUpload, [FromForm] decimal withdrawal, [FromForm] string bonuscode)
        {
            var apiResult = new List<ApiResult>();
            var authData = _authorizeBLL.GetAuthData();
            string bonusUrl = $"{_bonusApiUrl}SetAdjustBonus";
            try
            {
                if (excelUpload.ContentType.Equals("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        StringComparison.OrdinalIgnoreCase) ||
                    excelUpload.ContentType.Equals("application/vnd.ms-excel", StringComparison.OrdinalIgnoreCase) ||
                    excelUpload.ContentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
                {
                    var excelPkg = new ExcelPackage(excelUpload.OpenReadStream());
                    var sheet = excelPkg.Workbook.Worksheets.First(); //取得Sheet1
                    var isLastRow = false;
                    var RowId = 2; // 因為有標題列，所以從第2列開始讀起

                    do // 讀取資料，直到讀到空白列為止
                    {
                        var custUsername = string.IsNullOrEmpty(sheet.Cells[RowId, 1].Text)
                            ? null
                            : sheet.Cells[RowId, 1].Text;

                        var sabaId = sheet.Cells[RowId, 2].Text.Trim();
                        var custId = (string.IsNullOrEmpty(sabaId)) ? null : _common.TpIdToCustId(sabaId);

                        decimal.TryParse(sheet.Cells[RowId, 3].Text.Trim(), out var amount);
                        int.TryParse(sheet.Cells[RowId, 4].Text.Trim(), out var rollOver);
                        var memo = sheet.Cells[RowId, 5].Text.Trim();
                        if (string.IsNullOrEmpty(custUsername) && custId == null)
                        {
                            isLastRow = true;
                        }
                        else
                        {
                            var custInfoDetail = _custInfoBLL.GetCustInfoDetails(custId, custUsername, optSite);
                            if (custInfoDetail == null)
                            {
                                apiResult.Add(new ApiResult()
                                {
                                    ErrorCode = 1002,
                                    Message = $"Not fund. Username: {custUsername}. SabaId: {sabaId}."
                                });
                            }
                            else if (custInfoDetail.Status == 1)
                            {
                                try
                                {
                                    //非Retention Bonus
                                    if (option != 5 && option != 6)
                                    {
                                        var withdrawalLimitAmount =
                                        (withdrawalLimitOption == 1) ? amount * withdrawal : 0;
                                        var adjustmentAndWithdrawalLimit =
                                            new AdjustmentAndWithdrawalLimit
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
                                            var balance = _custInfoBLL.GetBalance(custInfoDetail.CustId);
                                            if (balance < adjustmentAndWithdrawalLimit.Amount)
                                            {
                                                if (_mainWalletType == 2)
                                                {
                                                    var deAmount = adjustmentAndWithdrawalLimit.Amount - balance;
                                                    var inTpTransfer = new TpTransfer()
                                                    {
                                                        TpId = 1,
                                                        Amount = deAmount,
                                                        CustId = custInfoDetail.CustId,
                                                        direction = TpDirection.ToMain
                                                    };

                                                    var acc = await _tpBLL.TransferAsync(inTpTransfer);
                                                    if (acc.ErrorCode != 0 || acc.Data.Status != TransferStatus.Success)
                                                    {
                                                        apiResult.Add(new ApiResult()
                                                        {
                                                            ErrorCode = 1001,
                                                            Message =
                                                                $"Fail. Username: {custUsername}. SabaId: {sabaId}. {balance.ToString("#,##0.##")} < {adjustmentAndWithdrawalLimit.Amount.ToString("#,##0.##")}"
                                                        });
                                                        RowId += 1;
                                                        continue;
                                                    }
                                                }
                                            }

                                            balance = _custInfoBLL.GetBalance(custInfoDetail.CustId);

                                            if (balance < adjustmentAndWithdrawalLimit.Amount)
                                            {
                                                apiResult.Add(new ApiResult()
                                                {
                                                    ErrorCode = 1002,
                                                    Message =
                                                        $"Fail. Username: {custUsername}. SabaId: {sabaId}. {balance.ToString("#,##0.##")} < {adjustmentAndWithdrawalLimit.Amount.ToString("#,##0.##")}"
                                                });
                                                RowId += 1;
                                                continue;
                                            }
                                        }

                                        if (_paymentBLL.AdjustmentAndWithdrawalLimit(adjustmentAndWithdrawalLimit,
                                            authData.UserName, out transId) > 0)
                                        {
                                            if (_mainWalletType == 2 && adjustmentAndWithdrawalLimit.AdjustType == 1)
                                            {
                                                var balance = _custInfoBLL.GetBalance(custInfoDetail.CustId);
                                                var inTpTransfer = new TpTransfer()
                                                {
                                                    TpId = 1,
                                                    Amount = balance,
                                                    CustId = custInfoDetail.CustId,
                                                    direction = TpDirection.ToTp
                                                };

                                                var acc = await _tpBLL.TransferAsync(inTpTransfer);
                                                if (acc.ErrorCode == 0 && acc.Data.Status == TransferStatus.Success)
                                                {
                                                    apiResult.Add(new ApiResult()
                                                    {
                                                        ErrorCode = 1,
                                                        Message =
                                                            $"Success. Username: {custUsername}. SabaId: {sabaId}. ID is {transId}."
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                apiResult.Add(new ApiResult()
                                                {
                                                    ErrorCode = 1,
                                                    Message =
                                                        $"Success. Username: {custUsername}. SabaId: {sabaId}. ID is {transId}."
                                                });
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var setAdjustBonusReq = new SetAdjustBonusReq()
                                        {
                                            CustId = custInfoDetail.CustId,
                                            BonusCode = bonuscode,
                                            Memo = memo,
                                            Operator = authData.UserName,
                                            Amount = amount,
                                            RollOver = rollOver,
                                            PromotionType = option == 5 ? 9 : 8
                                        };
                                        var apiOnlineResult =
                                             await courierAdapter.MyPostAsync<string, SetAdjustBonusReq>(bonusUrl, setAdjustBonusReq);
                                        if (apiOnlineResult.ErrorCode == 0)
                                        {
                                            apiResult.Add(new ApiResult()
                                            {
                                                ErrorCode = 1,
                                                Message = $"Success. Username: {custUsername}. SabaId: {sabaId}. ID is {apiOnlineResult.Data}."
                                            });
                                        }
                                        else
                                        {
                                            apiResult.Add(new ApiResult()
                                            {
                                                ErrorCode = apiOnlineResult.ErrorCode,
                                                Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. {apiOnlineResult.Message}"
                                            });
                                        }
                                    }

                                }
                                catch (Exception ex)
                                {
                                    apiResult.Add(new ApiResult()
                                    {
                                        ErrorCode = 999,
                                        Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. {ex}"
                                    });
                                }
                            }
                            else if (custInfoDetail.Status == 2)
                            {
                                apiResult.Add(new ApiResult()
                                {
                                    ErrorCode = 1002,
                                    Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. Suspend Status."
                                });
                            }
                            else if (custInfoDetail.Status == 4)
                            {
                                apiResult.Add(new ApiResult()
                                {
                                    ErrorCode = 1002,
                                    Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. Lock Status."
                                });
                            }
                            else
                            {
                                apiResult.Add(new ApiResult()
                                {
                                    ErrorCode = 1002,
                                    Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. Closed Status."
                                });
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
        }

        [HttpPost]
        public ApiListResultCurrencyAmount<VipRebateInfo> GetVipRebateInfoList(VipRebateInfoPara para)
        {
            var apiResult = new ApiListResultCurrencyAmount<VipRebateInfo>();
            try
            {
                var CurrencyAmount = new List<string>();
                var SearchResult = _paymentBLL.GetVipRebateInfoList(para.SelectSite, para.Currency, para.StartDate,
                    para.EndDate, para.User, para.PageSize, para.PageNumber,
                    para.CategoryId, para.BaseCurrencyId, ref CurrencyAmount);
                apiResult.Data = SearchResult.VipRebateInfoList;
                apiResult.PageNumber = SearchResult.PageNumber;
                apiResult.TotalPages = SearchResult.TotalPages;
                apiResult.RowCount = SearchResult.RowCount;
                apiResult.ErrorCode = 0;
                apiResult.CurrencyAmount = CurrencyAmount;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Use：
        /// CSD > Customer Administration > CustInfo > Vip Rebate Log
        /// </remarks>
        /// <param name="para"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResultCurrencyAmount<VipRebateInfo> GetVipRebateInfoListByCustId(VipRebateInfoByCustIdPara para)
        {
            var SearchResult = _paymentBLL.GetVipRebateInfoListByCustId(para.CustId,
                para.StartDate,
                para.EndDate,
                10,
                para.PageNumber);

            return new ApiListResultCurrencyAmount<VipRebateInfo>
            {
                Data = SearchResult.VipRebateInfoList,
                TotalPages = SearchResult.TotalPages,
                PageNumber = SearchResult.PageNumber,
                RowCount = SearchResult.RowCount,
                CurrencyAmount = SearchResult.CurrencyAmount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        public IActionResult GetVipRebateInfoListExport(string SelectSite, int Currency, long StartDate, long EndDate,
            string User, int CategoryId, int BaseCurrencyId, int RebateCategoriesId)
        {
            var SearchResult = _paymentBLL.GetVipRebateInfoListExport(SelectSite, Currency, StartDate, EndDate, User,
                CategoryId, BaseCurrencyId, RebateCategoriesId);
            var stream = _excelBLL.Export(SearchResult.VipRebateInfoList);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"VipRebateReport_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        public ApiResult<IEnumerable<VipTPCategory>> GetTPCategoryListByVipRebate()
        {
            var apiResult = new ApiResult<IEnumerable<VipTPCategory>>
            {
                ErrorCode = 0,
                Data = _paymentBLL.GetTPCategoryList()
            };
            return apiResult;
        }

        #region Adjustment Limit

        public class GetAdjustmentPara
        {
            public int SiteId { get; set; }

            public int CurrencyId { get; set; }
        }

        [HttpPost]
        public ApiResult<AdjustmentLimtData> GetAdjustmentData(GetAdjustmentPara para)
        {
            var apiResult = new ApiResult<AdjustmentLimtData>
            {
                Data = _paymentBLL.GetAdjustmentData(para.SiteId, para.CurrencyId),
                ErrorCode = 0
            };
            return apiResult;
        }

        public class AddAdjustmentLimitPara
        {
            public int SiteId { get; set; }

            public int CurrencyId { get; set; }
            public double AdjustmentMax { get; set; }
        }

        [HttpPost]
        public ApiResult AddAdjustmentLimitInfo(AddAdjustmentLimitPara para)
        {
            var result =
                _paymentBLL.AddAdjustmentLimitInfo(para.SiteId, para.CurrencyId, para.AdjustmentMax,
                    User.Identity.Name);
            var apiResult = new ApiResult();
            if (result > 0)
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = _selLangBLL.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = 9;
                apiResult.Message = _selLangBLL.GetMsg("msg_UpdateFailed");
            }

            return apiResult;
        }

        public class CheckPassWordPara
        {
            public string PassWord { get; set; }
            public int SiteId { get; set; }
        }

        [HttpPost]
        public ApiResult CheckPassWord(CheckPassWordPara para)
        {
            var apiResult = new ApiResult();
            if (_paymentBLL.CheckPassWord(para.PassWord, para.SiteId))
            {
                apiResult.ErrorCode = 0;
            }
            else
            {
                apiResult.ErrorCode = 1004;
            }

            return apiResult;
        }

        public class CheckBalanceAdjustBonusCodePara
        {
            public string BonusCode { get; set; }
            public int PromotionType { get; set; }
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
        }
        [HttpPost]
        public ApiResult<BalanceAdjustBonusCodeRep> CheckBalanceAdjustBonusCode(CheckBalanceAdjustBonusCodePara para)
        {
            var apiResult = new ApiResult<BalanceAdjustBonusCodeRep>();
            var data = _paymentBLL.CheckBalanceAdjustBonusCode(para.BonusCode, para.PromotionType, para.SiteId, para.CurrencyId);
            apiResult.Data = data;
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        public class CheckBalanceAdjustBonusCodeBatchPara
        {
            public string BonusCode { get; set; }
            public int PromotionType { get; set; }
            public int SiteId { get; set; }
        }

        [HttpPost]
        public ApiResult<BalanceAdjustBonusCodeBatchRep> CheckBalanceAdjustBonusCodeBatch(CheckBalanceAdjustBonusCodeBatchPara para)
        {
            var apiResult = new ApiResult<BalanceAdjustBonusCodeBatchRep>();
            var data = _paymentBLL.CheckBalanceAdjustBonusCodeBatch(para.BonusCode, para.PromotionType, para.SiteId);
            apiResult.Data = data;
            apiResult.ErrorCode = 0;
            return apiResult;
        }
        public class CheckAdjustmentLimitPara
        {
            public string BonusCode { get; set; }
            public int SiteId { get; set; }
        }
        public ApiResult CheckRententionBonusAdjustmentLimit(CheckAdjustmentLimitPara para)
        {
            var apiResult = new ApiResult();
            List<CheckBalanceAdjustData> BonusCodeData = new List<CheckBalanceAdjustData>();
            apiResult.ErrorCode = 0;
            try
            {
                BonusCodeData = _paymentBLL.CheckRententionBonusAdjustmentLimit(para.BonusCode, para.SiteId).ToList();

                BonusCodeData.ForEach(x =>
                {
                    var adjustmentLimit = _paymentBLL.GetAdjustmentData(para.SiteId, x.CurrencyId);

                    if (adjustmentLimit != null)
                    {
                        decimal Max = Convert.ToDecimal(adjustmentLimit.AdjustmentMax);
                        if (Max != 0 && Max < x.Bonus)
                        {
                            apiResult.ErrorCode = 1000;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = -1;
                apiResult.Message = ex.ToString();
                return apiResult;
            }
            return apiResult;
        }
        #endregion

        #endregion
    }
}