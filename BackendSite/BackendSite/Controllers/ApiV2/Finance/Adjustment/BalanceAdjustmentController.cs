using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Attributes;
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
        private readonly int _mainWalletType;
        private readonly string _bonusApiUrl;
        private readonly CourierAdapter _courierAdapter;
        private readonly Common _common;
        
        #endregion

        #region Constructors

        public BalanceAdjustmentController(IServiceProvider provider)
        {
            _paymentBll = provider.GetRequiredService<PaymentBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _tpBll = provider.GetRequiredService<TpBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _common = provider.GetRequiredService<Common>();
            _mainWalletType = provider.GetRequiredService<IConfiguration>().GetValue<int>("MainWalletType"); //1=主錢包
            _bonusApiUrl = provider.GetRequiredService<IConfiguration>().GetValue<string>("BonusService");
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
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
            public string BonusCode { get; set; }
            public int RollOver { get; set; }
        }
        
        public class GetAdjustmentReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
        }
        
        public class CheckPassWordReq
        {
            public string PassWord { get; set; }
            public int SiteId { get; set; }
        }
        
        public class CheckBalanceAdjustBonusCodeReq
        {
            public string BonusCode { get; set; }
            public int PromotionType { get; set; }
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
        }

        public class CheckBalanceAdjustBonusCodeBatchReq
        {
            public string BonusCode { get; set; }
            public int PromotionType { get; set; }
            public int SiteId { get; set; }
        }
        
        public class CheckAdjustmentLimitReq
        {
            public string BonusCode { get; set; }
            public int SiteId { get; set; }
        }
        
        #endregion

        #region API

        #region Balance Adjustment
        
        /// <summary>
        /// BalanceAdjustment/AdjustmentAndWithdrawalLimitAsync 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Balance Adjustment > [Submit] > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public async Task<ApiResult> AdjustmentAndWithdrawalLimitAsync(AdjustmentAndWithdrawalLimitReq req)
        {
            var apiResult = new ApiResult();
            var adjustmentAndWithdrawalLimit = new AdjustmentAndWithdrawalLimit();
            var bonusUrl = $"{_bonusApiUrl}SetAdjustBonus";
            try
            {
                var authData = _authorizeBll.GetAuthData();
                var custInfo = _custInfoBll.GetCustInfoWithCache(req.CustId);
                
                if (custInfo != null)
                {
                    var balance = _custInfoBll.GetBalance(custInfo.CustId);
                    //非RetentionBonus,ManualBonus
                    if (req.Option != (int)AdjustmentInfoAdjustTypeEnum.RetentionBonus && req.Option != (int)AdjustmentInfoAdjustTypeEnum.ManualBonus)
                    {
                        adjustmentAndWithdrawalLimit.CustId = custInfo.CustId;
                        adjustmentAndWithdrawalLimit.CurrencyId = custInfo.CurrencyId;
                        adjustmentAndWithdrawalLimit.UserName = custInfo.UserName;
                        adjustmentAndWithdrawalLimit.SiteId = custInfo.SiteId;
                        adjustmentAndWithdrawalLimit.Amount = req.Amount;
                        adjustmentAndWithdrawalLimit.AdjustType = req.Option;
                        adjustmentAndWithdrawalLimit.SiteName = custInfo.SiteName;
                        adjustmentAndWithdrawalLimit.Operator = authData.UserName;
                        adjustmentAndWithdrawalLimit.WithdrawalLimitAmount = req.WithdrawalLimitAmount;
                        adjustmentAndWithdrawalLimit.WithdrawalLimitAmountAdjustType = req.WithdrawalLimitOption;
                        adjustmentAndWithdrawalLimit.Memo = req.Memo;

                        switch (adjustmentAndWithdrawalLimit.AdjustType)
                        {
                            case (int)AdjustmentInfoAdjustTypeEnum.BalanceMinus:  //Balance(-)
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

                                            var acc = await _tpBll.TransferAsync(inTpTransfer);
                                            if (acc.ErrorCode != (int)ErrorCodeEnum.Success || acc.Data.Status != TransferStatus.Success)
                                            {
                                                apiResult.ErrorCode = (int)ErrorCodeEnum.TransferError;
                                                apiResult.Message = $"{balance.ToString("#,##0.##")} < {adjustmentAndWithdrawalLimit.Amount.ToString("#,##0.##")}";
                                                return apiResult;
                                            }
                                        }
                                    }


                                    if (balance < adjustmentAndWithdrawalLimit.Amount)
                                    {
                                        apiResult.ErrorCode = (int)ErrorCodeEnum.BalanceAdjustmentError;
                                        apiResult.Message = $"{balance.ToString("#,##0.##")} < {adjustmentAndWithdrawalLimit.Amount.ToString("#,##0.##")}";
                                        return apiResult;
                                    }

                                    if (_mainWalletType == 1 && (balance - adjustmentAndWithdrawalLimit.Amount) < 0)
                                    {
                                        apiResult.ErrorCode = (int)ErrorCodeEnum.BalanceAdjustmentError;
                                        apiResult.Message = $"{balance.ToString("#,##0.##")} + {adjustmentAndWithdrawalLimit.Amount.ToString("#,##0.##")} < 0. Reject";
                                        return apiResult;
                                    }
                                }
                                break; 
                            
                            case (int)AdjustmentInfoAdjustTypeEnum.BalancePlus:  //Balance(+)
                            case (int)AdjustmentInfoAdjustTypeEnum.BonusPlus:  //Bonus(+)
                                {
                                    if (_mainWalletType == 1 && (balance + adjustmentAndWithdrawalLimit.Amount) < 0)
                                    {
                                        apiResult.ErrorCode = (int)ErrorCodeEnum.BalanceAdjustmentError;
                                        apiResult.Message = $"{balance.ToString("#,##0.##")} + {adjustmentAndWithdrawalLimit.Amount.ToString("#,##0.##")} < 0. Reject";
                                        return apiResult;
                                    }
                                }
                                break;
                            case (int)AdjustmentInfoAdjustTypeEnum.BonusMinus:  //Bonus(-)
                                {
                                    if (_mainWalletType == 1 && (balance - adjustmentAndWithdrawalLimit.Amount) < 0)
                                    {
                                        apiResult.ErrorCode = (int)ErrorCodeEnum.BalanceAdjustmentError;
                                        apiResult.Message = $"{balance.ToString("#,##0.##")} - {adjustmentAndWithdrawalLimit.Amount.ToString("#,##0.##")} < 0. Reject";
                                        return apiResult;
                                    }
                                }
                                break;

                        }

                        if (_paymentBll.AdjustmentAndWithdrawalLimit(adjustmentAndWithdrawalLimit, User.Identity.Name,
                            out var transId) > 0)
                        {
                            if (_mainWalletType == 2 && adjustmentAndWithdrawalLimit.AdjustType == (int)AdjustmentInfoAdjustTypeEnum.BalancePlus)
                            {
                                var inTpTransfer = new TpTransfer()
                                { TpId = 1, Amount = balance, CustId = custInfo.CustId, direction = TpDirection.ToTp };

                                var acc = await _tpBll.TransferAsync(inTpTransfer);
                                if (acc.ErrorCode == (int)ErrorCodeEnum.Success && acc.Data.Status == TransferStatus.Success)
                                {
                                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                                    apiResult.Message = $"Success. ID is {transId}";
                                }
                            }
                            else
                            {
                                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                                apiResult.Message = $"Success. ID is {transId}";
                            }
                        }
                    }
                    else
                    {
                        var setAdjustBonusReq = new SetAdjustBonusReq()
                        {
                            CustId = custInfo.CustId,
                            BonusCode = req.BonusCode,
                            Memo = req.Memo,
                            Operator = authData.UserName,
                            Amount = req.Amount,
                            PromotionType = req.Option == (int)AdjustmentInfoAdjustTypeEnum.RetentionBonus ?  9 : 8
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
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.ToString();
            }

            return apiResult;
        }
        
        /// <summary>
        /// BalanceAdjustment/GetAdjustmentLimitInfo 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Balance Adjustment 
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult<AdjustmentLimitData> GetAdjustmentLimitInfo(GetAdjustmentReq req)
        {
            var apiResult = new ApiResult<AdjustmentLimitData>
            {
                Data = _paymentBll.GetAdjustmentData(req.SiteId, req.CurrencyId),
                ErrorCode = (int) ErrorCodeEnum.Success
            };
            return apiResult;
        }
        
        /// <summary>
        /// BalanceAdjustment/CheckPassWord 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Balance Adjustment > [submit] > [submit] > [Popup]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult CheckPassWord(CheckPassWordReq req)
        {
            var apiResult = new ApiResult();
            if ( _paymentBll.CheckPassWord(req.PassWord, req.SiteId))
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            }
            else
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.PasswordError;
            }

            return apiResult;
        }
        
        /// <summary>
        /// BalanceAdjustment/CheckBalanceAdjustBonusCode 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Balance Adjustment 
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult<BalanceAdjustBonusCodeRep> CheckBalanceAdjustBonusCode(CheckBalanceAdjustBonusCodeReq req)
        {
            var apiResult = new ApiResult<BalanceAdjustBonusCodeRep>();
            var data = _paymentBll.CheckBalanceAdjustBonusCode(req.BonusCode, req.PromotionType, req.SiteId, req.CurrencyId);
            apiResult.Data = data;
            apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            return apiResult;
        }
        
        #endregion

        #region Balance Adjustment Batch

        /// <summary>
        /// BalanceAdjustment/BalanceAdjustmentBatch 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Balance Adjustment Batch 
        /// </remarks>
        /// <param name="option"></param>
        /// <param name="optSite"></param>
        /// <param name="withdrawalLimitOption"></param>
        /// <param name="excelUpload"></param>
        /// <param name="withdrawal"></param>
        /// <param name="bonusCode"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/BalanceAdjustmentBatch" })]
        public async Task<List<ApiResult>> BalanceAdjustmentBatch(
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
                                    if (option != (int)BalanceAdjustmentType.RetentionBouns && option != (int)BalanceAdjustmentType.ManualBonus)
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

                                        if (adjustmentAndWithdrawalLimit.AdjustType == (int)BalanceAdjustmentType.BalanceMinus)
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
                                                    if (acc.ErrorCode != (int) ErrorCodeEnum.Success || acc.Data.Status != TransferStatus.Success)
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
                                            if (_mainWalletType == 2 && adjustmentAndWithdrawalLimit.AdjustType == (int)BalanceAdjustmentType.BalancePlus)
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
                                                if (acc.ErrorCode == (int) ErrorCodeEnum.Success && acc.Data.Status == TransferStatus.Success)
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
                                            PromotionType = option == (int)BalanceAdjustmentType.RetentionBouns ? 9 : 8
                                        };
                                        var apiOnlineResult = await _courierAdapter.MyPostAsync<ApiResult, SetAdjustBonusReq>(bonusUrl, setAdjustBonusReq);
                                        if (apiOnlineResult.ErrorCode == (int) ErrorCodeEnum.Success )
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
                                    ErrorCode = (int)ErrorCodeEnum.BalanceAdjustmentError,
                                    Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. Suspend Status."
                                });
                            }
                            else if (custInfoDetail.Status == (int)CustomerStatusEnum.Lock)
                            {
                                apiResult.Add(new ApiResult()
                                {
                                    ErrorCode = (int)ErrorCodeEnum.BalanceAdjustmentError,
                                    Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. Lock Status."
                                });
                            }
                            else
                            {
                                apiResult.Add(new ApiResult()
                                {
                                    ErrorCode = (int)ErrorCodeEnum.BalanceAdjustmentError,
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
        /// BalanceAdjustment/CheckBalanceAdjustBonusCodeBatch 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Balance Adjustment Batch 
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult<BalanceAdjustBonusCodeBatchRep> CheckBalanceAdjustBonusCodeBatch(CheckBalanceAdjustBonusCodeBatchReq req)
        {
            var apiResult = new ApiResult<BalanceAdjustBonusCodeBatchRep>();
            var data = _paymentBll.CheckBalanceAdjustBonusCodeBatch(req.BonusCode, req.PromotionType, req.SiteId);
            apiResult.Data = data;
            apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            return apiResult;
        }
        
        /// <summary>
        /// BalanceAdjustment/CheckRententionBonusAdjustmentLimit 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Balance Adjustment Batch 
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult CheckRententionBonusAdjustmentLimit(CheckAdjustmentLimitReq req)
        {
            var apiResult = new ApiResult();
            try
            {
                var bonusCodeData = _paymentBll.CheckRententionBonusAdjustmentLimit(req.BonusCode, req.SiteId).ToList();
                bonusCodeData.ForEach(x =>
                {
                    var adjustmentLimit = _paymentBll.GetAdjustmentData(req.SiteId, x.CurrencyId);

                    if (adjustmentLimit != null)
                    {
                        var max = Convert.ToDecimal(adjustmentLimit.AdjustmentMax);
                        if (max != 0 && max < x.Bonus)
                        {
                            apiResult.ErrorCode = 1000;
                        }
                    }
                });
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.SystemException;
                apiResult.Message = ex.ToString();
                return apiResult;
            }
            return apiResult;
        }
        
        #endregion

        #endregion
    }
}