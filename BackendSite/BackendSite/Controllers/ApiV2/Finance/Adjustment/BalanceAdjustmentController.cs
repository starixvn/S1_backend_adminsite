using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;

namespace BackendSite.Controllers.ApiV2.Finance.Adjustment
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BalanceAdjustmentController : ControllerBase
    {
        #region Fields

        private readonly PaymentBLL _paymentBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly TpBLL _tpBll;
        private readonly AuthorizeBLL _authorizeBll;
        private readonly CourierAdapter _courierAdapter;
        private readonly Common _common;

        private readonly int _mainWalletType;
        private readonly string _bonusApiUrl;

        #endregion

        #region Constructors

        public BalanceAdjustmentController(IServiceProvider provider)
        {
            _paymentBll = provider.GetRequiredService<PaymentBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _tpBll = provider.GetRequiredService<TpBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _common = provider.GetRequiredService<Common>();
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();

            _mainWalletType = provider.GetRequiredService<IConfiguration>().GetValue<int>("MainWalletType"); //1=主錢包,2=體育錢包
            _bonusApiUrl = provider.GetRequiredService<IConfiguration>().GetValue<string>("BonusService");
        }

        #endregion

        #region API Model

        public class SubmitRequest
        {
            public int CustId { get; set; }
            public AdjustmentInfoAdjustTypeEnum Option { get; set; }
            public decimal Amount { get; set; }
            public decimal WithdrawalLimitAmount { get; set; }
            public int WithdrawalLimitOption { get; set; }
            public string Memo { set; get; }
            public string BonusCode { get; set; }
            public int RollOver { get; set; }
        }

        public class GetAdjustmentLimitInfoRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
        }

        public class CheckPassWordRequest
        {
            public int SiteId { get; set; }
            public string PassWord { get; set; }
        }

        public class CheckBonusCodeRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public string BonusCode { get; set; }
            public PromotionType PromotionType { get; set; }
        }

        public class CheckBonusCodeForBatchRequest
        {
            public int SiteId { get; set; }
            public PromotionType PromotionType { get; set; }
            public string BonusCode { get; set; }
        }

        public class CheckLimitForRententionBonusRequest
        {
            public int SiteId { get; set; }
            public string BonusCode { get; set; }
        }

        #endregion

        #region API

        #region Balance Adjustment

        /// <summary>
        /// BalanceAdjustment/GetAdjustmentLimitInfo 取得調整限額資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Balance Adjustment > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/PaymentAdjustment" })]
        public ApiResult<AdjustmentLimitData> GetAdjustmentLimitInfo(GetAdjustmentLimitInfoRequest request)
        {
            var apiResult = new ApiResult<AdjustmentLimitData>
            {
                Data = _paymentBll.GetAdjustmentData(request.SiteId, request.CurrencyId),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
            return apiResult;
        }

        /// <summary>
        /// BalanceAdjustment/CheckBonusCode 檢查獎金代碼
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Balance Adjustment > [Submit] > [Check Bonus Code]
        /// 
        /// Memo：當調整選項選擇為Retention Bonus或Manual Bonus，才需要有此步驟。
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/PaymentAdjustment" })]
        public ApiResult<BalanceAdjustBonusCodeRep> CheckBonusCode(CheckBonusCodeRequest request)
        {
            var apiResult = new ApiResult<BalanceAdjustBonusCodeRep>();
            var data = _paymentBll.CheckBalanceAdjustBonusCode(request.BonusCode, (int)request.PromotionType, request.SiteId, request.CurrencyId);
            apiResult.Data = data;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            return apiResult;
        }

        /// <summary>
        /// BalanceAdjustment/CheckPassWord 驗證調帳密碼
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Balance Adjustment > [Submit] > [Submit] > [OK]
        /// 
        /// Memo：當異動金額超過限制，則需要輸入密碼，限制金額請參考GetAdjustmentLimitInfo取的資訊
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/PaymentAdjustment" })]
        public ApiResult CheckPassWord(CheckPassWordRequest request)
        {
            var apiResult = new ApiResult();
            if (_paymentBll.CheckPassWord(request.PassWord, request.SiteId))
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.PasswordError;
            }

            return apiResult;
        }

        /// <summary>
        /// BalanceAdjustment/Submit 提交餘額調整資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Balance Adjustment > [Submit] > [Submit] > if have [PopOut] to enter password, Then need to press [OK]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/PaymentAdjustment" })]
        public async Task<ApiResult> SubmitAsync(SubmitRequest request)
        {
            var apiResult = new ApiResult();
            var bonusUrl = $"{_bonusApiUrl}SetAdjustBonus";

            var authData = _authorizeBll.GetAuthData();
            var custInfo = _custInfoBll.GetCustInfoWithCache(request.CustId);

            if (custInfo != null)
            {
                var balance = _custInfoBll.GetBalance(custInfo.CustId);

                //非RetentionBonus,ManualBonus
                if (request.Option != AdjustmentInfoAdjustTypeEnum.RetentionBonus
                    && request.Option != AdjustmentInfoAdjustTypeEnum.ManualBonus)
                {
                    switch (request.Option)
                    {
                        case AdjustmentInfoAdjustTypeEnum.BalanceMinus:  //Balance(-)
                            {

                                if (balance < request.Amount)
                                {
                                    if (_mainWalletType == 2) //體育錢包
                                    {
                                        var deAmount = request.Amount - balance;
                                        var inTpTransfer = new TpTransfer()
                                        {
                                            TpId = 1,
                                            Amount = deAmount,
                                            CustId = custInfo.CustId,
                                            direction = TpDirection.ToMain
                                        };

                                        var acc = await _tpBll.TransferAsync(inTpTransfer);
                                        if (acc.ErrorCode != (int)ErrorCodeEnum.Success || acc.Data.Status != TransferStatus.Success)
                                        {
                                            throw new AdminException(ErrorCodeEnum.TransferError,
                                        $"{balance.ToString("#,##0.##")} < {request.Amount.ToString("#,##0.##")}. Balance(-)：SportBook Wallet Fail.");
                                        }
                                    }
                                    else
                                    {
                                        //主錢包
                                        throw new AdminException(ErrorCodeEnum.BalanceAdjustmentError,
                                            $"{balance.ToString("#,##0.##")} < {request.Amount.ToString("#,##0.##")}. Balance(-)：Main Wallet Fail.");
                                    }
                                }

                                if (_mainWalletType == 1 && (balance - request.Amount) < 0)
                                {
                                    throw new AdminException(ErrorCodeEnum.BalanceAdjustmentError,
                                        $"{balance.ToString("#,##0.##")} + {request.Amount.ToString("#,##0.##")} < 0. Balance(-)：Main Wallet Reject.");
                                }
                            }
                            break;

                        case AdjustmentInfoAdjustTypeEnum.BalancePlus:
                        case AdjustmentInfoAdjustTypeEnum.BonusPlus:
                            {
                                if (_mainWalletType == 1 && (balance + request.Amount) < 0)
                                {
                                    throw new AdminException(ErrorCodeEnum.BalanceAdjustmentError,
                                     $"{balance.ToString("#,##0.##")} + {request.Amount.ToString("#,##0.##")} < 0. Balance(+) or Bonus(+)：Main Wallet Reject");
                                }
                            }
                            break;

                        case AdjustmentInfoAdjustTypeEnum.BonusMinus:
                            {
                                if (balance < request.Amount)
                                {
                                    throw new AdminException(ErrorCodeEnum.BalanceAdjustmentError,
                                     $"{balance.ToString("#,##0.##")} < {request.Amount.ToString("#,##0.##")}. Bonus(-)：Fail.");
                                }

                                if (_mainWalletType == 1 && (balance - request.Amount) < 0)
                                {
                                    throw new AdminException(ErrorCodeEnum.BalanceAdjustmentError,
                                    $"{balance.ToString("#,##0.##")} - {request.Amount.ToString("#,##0.##")} < 0.Bonus(-)：Main Wallet Reject");
                                }
                            }
                            break;
                    }

                    var adjustmentAndWithdrawalLimit = new AdjustmentAndWithdrawalLimit()
                    {
                        CustId = custInfo.CustId,
                        CurrencyId = custInfo.CurrencyId,
                        UserName = custInfo.UserName,
                        SiteId = custInfo.SiteId,
                        Amount = request.Amount,
                        AdjustType = (int)request.Option,
                        SiteName = custInfo.SiteName,
                        Operator = authData.UserName,
                        WithdrawalLimitAmount = request.WithdrawalLimitAmount,
                        WithdrawalLimitAmountAdjustType = request.WithdrawalLimitOption,
                        Memo = request.Memo
                    };

                    if (_paymentBll.AdjustmentAndWithdrawalLimit(adjustmentAndWithdrawalLimit, User.Identity.Name, out var transId) > 0)
                    {
                        if (_mainWalletType == 2 && adjustmentAndWithdrawalLimit.AdjustType == (int)AdjustmentInfoAdjustTypeEnum.BalancePlus)
                        {
                            var inTpTransfer = new TpTransfer()
                            {
                                TpId = 1,
                                Amount = balance,
                                CustId = custInfo.CustId,
                                direction = TpDirection.ToTp
                            };

                            var acc = await _tpBll.TransferAsync(inTpTransfer);
                            if (acc.ErrorCode == (int)ErrorCodeEnum.Success && acc.Data.Status == TransferStatus.Success)
                            {
                                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                                apiResult.Message = $"Success. ID is {transId}";
                            }
                            else
                            {
                                throw new AdminException(ErrorCodeEnum.TransferError,
                                      "Balance(+)：SportBook Wallet Fail.");
                            }
                        }
                        else
                        {
                            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                            apiResult.Message = $"Success. ID is {transId}";
                        }
                    }
                    else
                    {
                        throw new AdminException(ErrorCodeEnum.Fail,
                                      "AdjustmentAndWithdrawalLimit Fail.");
                    }
                }
                else
                {
                    var setAdjustBonusReq = new SetAdjustBonusReq()
                    {
                        CustId = custInfo.CustId,
                        BonusCode = request.BonusCode,
                        Memo = request.Memo,
                        Operator = authData.UserName,
                        Amount = request.Amount,
                        PromotionType = request.Option == AdjustmentInfoAdjustTypeEnum.RetentionBonus ? 9 : 8
                    };

                    var apiOnlineResult =
                        await _courierAdapter.MyPostAsync<ApiResult, SetAdjustBonusReq>(bonusUrl, setAdjustBonusReq);

                    if (apiOnlineResult.ErrorCode == (int)ErrorCodeEnum.Success)
                    {
                        apiResult.ErrorCode = apiOnlineResult.ErrorCode;
                        apiResult.Message = $"Success.";
                    }
                    else
                    {
                        apiResult.ErrorCode = apiOnlineResult.ErrorCode;
                        apiResult.Message = apiOnlineResult.Message;
                    }
                }
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.CustNotFound);
            }

            return apiResult;
        }

        #endregion

        #region Balance Adjustment Batch

        /// <summary>
        /// BalanceAdjustment/SubmitBatch 批次提交餘額調整資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Balance Adjustment Batch > [Submit]
        /// </remarks>
        /// <param name="option"></param>
        /// <param name="optSite"></param>
        /// <param name="withdrawalLimitOption"></param>
        /// <param name="excelUpload"></param>
        /// <param name="withdrawal"></param>
        /// <param name="bonusCode"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/BalanceAdjustmentBatch" })]
        public async Task<List<ApiResult>> SubmitBatchAsync(
            [FromForm] int option,
            [FromForm] int optSite,
            [FromForm] int withdrawalLimitOption,
            [FromForm] IFormFile excelUpload,
            [FromForm] decimal withdrawal,
            [FromForm] string bonusCode)
        {
            var apiResult = new List<ApiResult>();
            var authData = _authorizeBll.GetAuthData();
            var bonusUrl = $"{_bonusApiUrl}SetAdjustBonus";
            try
            {
                if (excelUpload.ContentType.Equals("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", StringComparison.OrdinalIgnoreCase) ||
                    excelUpload.ContentType.Equals("application/vnd.ms-excel", StringComparison.OrdinalIgnoreCase) ||
                    excelUpload.ContentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
                {
                    var excelPkg = new ExcelPackage(excelUpload.OpenReadStream());
                    var sheet = excelPkg.Workbook.Worksheets.First(); //取得Sheet1
                    var isLastRow = false;
                    var rowId = 2; // 因為有標題列，所以從第2列開始讀起

                    do // 讀取資料，直到讀到空白列為止
                    {
                        var custUsername = string.IsNullOrEmpty(sheet.Cells[rowId, 1].Text) ? null : sheet.Cells[rowId, 1].Text;
                        var sabaId = sheet.Cells[rowId, 2].Text.Trim();
                        var custId = (string.IsNullOrEmpty(sabaId)) ? null : _common.TpIdToCustId(sabaId);

                        decimal.TryParse(sheet.Cells[rowId, 3].Text.Trim(), out var amount);
                        var memo = sheet.Cells[rowId, 4].Text.Trim();
                        if (string.IsNullOrEmpty(custUsername) && custId == null)
                        {
                            isLastRow = true;
                        }
                        else
                        {
                            var custInfoDetail = _custInfoBll.GetCustInfoDetails(custId, custUsername, optSite);
                            if (custInfoDetail == null)
                            {
                                apiResult.Add(new ApiResult()
                                {
                                    ErrorCode = (int)ErrorCodeEnum.BalanceAdjustmentError,
                                    Message = $"Not fund. Username: {custUsername}. SabaId: {sabaId}."
                                });
                            }
                            else if (custInfoDetail.Status == (int)CustomerStatusEnum.Running)
                            {
                                try
                                {
                                    //非Retention Bonus
                                    if (option != (int)AdjustmentInfoAdjustTypeEnum.RetentionBonus
                                        && option != (int)AdjustmentInfoAdjustTypeEnum.ManualBonus)
                                    {
                                        var withdrawalLimitAmount = (withdrawalLimitOption == 1) ? amount * withdrawal : 0;
                                        var adjustmentAndWithdrawalLimit = new AdjustmentAndWithdrawalLimit
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

                                        if (adjustmentAndWithdrawalLimit.AdjustType == (int)AdjustmentInfoAdjustTypeEnum.BalanceMinus)
                                        {
                                            var balance = _custInfoBll.GetBalance(custInfoDetail.CustId);
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

                                                    var acc = await _tpBll.TransferAsync(inTpTransfer);
                                                    if (acc.ErrorCode != (int)ErrorCodeEnum.Success || acc.Data.Status != TransferStatus.Success)
                                                    {
                                                        apiResult.Add(new ApiResult()
                                                        {
                                                            ErrorCode = (int)ErrorCodeEnum.TransferError,
                                                            Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. {balance.ToString("#,##0.##")} < {adjustmentAndWithdrawalLimit.Amount.ToString("#,##0.##")}"
                                                        });
                                                        rowId += 1;
                                                        continue;
                                                    }
                                                }
                                            }

                                            balance = _custInfoBll.GetBalance(custInfoDetail.CustId);

                                            if (balance < adjustmentAndWithdrawalLimit.Amount)
                                            {
                                                apiResult.Add(new ApiResult()
                                                {
                                                    ErrorCode = (int)ErrorCodeEnum.BalanceAdjustmentError,
                                                    Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. {balance.ToString("#,##0.##")} < {adjustmentAndWithdrawalLimit.Amount.ToString("#,##0.##")}"
                                                });
                                                rowId += 1;
                                                continue;
                                            }
                                        }

                                        if (_paymentBll.AdjustmentAndWithdrawalLimit(adjustmentAndWithdrawalLimit, authData.UserName, out var transId) > 0)
                                        {
                                            if (_mainWalletType == 2 && adjustmentAndWithdrawalLimit.AdjustType == (int)AdjustmentInfoAdjustTypeEnum.BalancePlus)
                                            {
                                                var balance = _custInfoBll.GetBalance(custInfoDetail.CustId);
                                                var inTpTransfer = new TpTransfer()
                                                {
                                                    TpId = 1,
                                                    Amount = balance,
                                                    CustId = custInfoDetail.CustId,
                                                    direction = TpDirection.ToTp
                                                };

                                                var acc = await _tpBll.TransferAsync(inTpTransfer);
                                                if (acc.ErrorCode == (int)ErrorCodeEnum.Success && acc.Data.Status == TransferStatus.Success)
                                                {
                                                    apiResult.Add(new ApiResult()
                                                    {
                                                        ErrorCode = (int)ErrorCodeEnum.Success,
                                                        Message = $"Success. Username: {custUsername}. SabaId: {sabaId}. ID is {transId}."
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                apiResult.Add(new ApiResult()
                                                {
                                                    ErrorCode = (int)ErrorCodeEnum.Success,
                                                    Message = $"Success. Username: {custUsername}. SabaId: {sabaId}. ID is {transId}."
                                                });
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var setAdjustBonusReq = new SetAdjustBonusReq()
                                        {
                                            CustId = custInfoDetail.CustId,
                                            BonusCode = bonusCode,
                                            Memo = memo,
                                            Operator = authData.UserName,
                                            Amount = amount,
                                            PromotionType = option == (int)AdjustmentInfoAdjustTypeEnum.RetentionBonus ? 9 : 8
                                        };
                                        var apiOnlineResult = await _courierAdapter.MyPostAsync<ApiResult, SetAdjustBonusReq>(bonusUrl, setAdjustBonusReq);
                                        if (apiOnlineResult.ErrorCode == (int)ErrorCodeEnum.Success)
                                        {
                                            apiResult.Add(new ApiResult()
                                            {
                                                ErrorCode = apiOnlineResult.ErrorCode,
                                                Message = $"Success. Username: {custUsername}. SabaId: {sabaId}."
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
                                        ErrorCode = (int)ErrorCodeEnum.SystemException,
                                        Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. {ex}"
                                    });
                                }
                            }
                            else if (custInfoDetail.Status == (int)CustomerStatusEnum.Suspend)
                            {
                                apiResult.Add(new ApiResult()
                                {
                                    ErrorCode = (int)ErrorCodeEnum.CustStatusIsSuspend,
                                    Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. Suspend Status."
                                });
                            }
                            else if (custInfoDetail.Status == (int)CustomerStatusEnum.Lock)
                            {
                                apiResult.Add(new ApiResult()
                                {
                                    ErrorCode = (int)ErrorCodeEnum.CustStatusIsLocked,
                                    Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. Lock Status."
                                });
                            }
                            else
                            {
                                apiResult.Add(new ApiResult()
                                {
                                    ErrorCode = (int)ErrorCodeEnum.CustStatusIsClosed,
                                    Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. Closed Status."
                                });
                            }

                            rowId += 1;
                        }
                    } while (!isLastRow);

                    return apiResult;
                }
                else
                {
                    apiResult.Add(new ApiResult() { ErrorCode = (int)ErrorCodeEnum.FileTypeError, Message = "File Type Error" });
                    return apiResult;
                }
            }
            catch (Exception ex)
            {
                apiResult.Add(new ApiResult() { ErrorCode = (int)ErrorCodeEnum.SystemException, Message = ex.ToString() });
                return apiResult;
            }
        }

        /// <summary>
        /// BalanceAdjustment/CheckBonusCodeForBatch 檢查批次的獎金代碼
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Balance Adjustment Batch > [Check Bonus Code]
        /// 
        /// Memo：當調整選項選擇為Retention Bonus或Manual Bonus，才需要有此步驟。
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/BalanceAdjustmentBatch" })]
        public ApiResult<BalanceAdjustBonusCodeBatchRep> CheckBonusCodeForBatch(CheckBonusCodeForBatchRequest request)
        {
            var apiResult = new ApiResult<BalanceAdjustBonusCodeBatchRep>();
            var data = _paymentBll.CheckBalanceAdjustBonusCodeBatch(request.BonusCode, (int)request.PromotionType, request.SiteId);
            apiResult.Data = data;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            return apiResult;
        }

        /// <summary>
        /// BalanceAdjustment/CheckLimitForRententionBonus  檢查保留獎金的限制
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Balance Adjustment Batch > [Submit]
        /// 
        /// Memo：當調整選項選擇為Retention Bonus，才需要有此步驟。
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/BalanceAdjustmentBatch" })]
        public ApiResult CheckLimitForRententionBonus(CheckLimitForRententionBonusRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                var bonusCodeData = _paymentBll.CheckRententionBonusAdjustmentLimit(request.BonusCode, request.SiteId).ToList();
                bonusCodeData.ForEach(x =>
                {
                    var adjustmentLimit = _paymentBll.GetAdjustmentData(request.SiteId, x.CurrencyId);

                    if (adjustmentLimit != null)
                    {
                        var max = Convert.ToDecimal(adjustmentLimit.AdjustmentMax);
                        if (max != 0 && max < x.Bonus)
                        {
                            apiResult.ErrorCode = 1000;
                        }
                    }
                });
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.ToString();
                return apiResult;
            }
            return apiResult;
        }

        #endregion

        #endregion
    }
}