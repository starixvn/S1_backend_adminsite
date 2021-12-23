using System;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        
        #endregion

        #region Constructors

        public BalanceAdjustmentController(IServiceProvider provider)
        {
            _paymentBll = provider.GetRequiredService<PaymentBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _tpBll = provider.GetRequiredService<TpBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
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

        #endregion
        
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
    }
}