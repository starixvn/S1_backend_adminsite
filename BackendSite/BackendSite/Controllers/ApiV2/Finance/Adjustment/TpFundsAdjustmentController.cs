using System.Text.RegularExpressions;

using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using static BackendSite.Controllers.ApiV2.Finance.Adjustment.BalanceAdjustmentController;

namespace BackendSite.Controllers.ApiV2.Finance.Adjustment
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class TpFundsAdjustmentController : ControllerBase
    {
        #region Fields

        private readonly TpBLL _tpBll;
        private readonly Common _common;
        private readonly CustInfoBLL _custInfoBll;
        private readonly PaymentBLL _paymentBll;
        private readonly AdjustmentLogReportBLL _adjustmentLogReportBll;
        private readonly BankFinancialBLL _bankFinancialBll;

        #endregion

        #region Constructors

        public TpFundsAdjustmentController(IServiceProvider provider)
        {
            _tpBll = provider.GetRequiredService<TpBLL>();
            _common = provider.GetRequiredService<Common>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _adjustmentLogReportBll = provider.GetRequiredService<AdjustmentLogReportBLL>();
            _bankFinancialBll = provider.GetRequiredService<BankFinancialBLL>();
            _paymentBll = provider.GetRequiredService<PaymentBLL>();
        }

        #endregion

        #region Model

        public class GetTpBySiteIdRequest
        {
            public int SiteId { get; set; }
        }

        public class GetTpCustInfoRequest
        {
            public int SiteId { get; set; }
            public int TpId { get; set; }
            public string UserName { get; set; }
            public string UserTpId { get; set; }
        }

        public class SubmitRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int TpId { get; set; }
            public string TpName { get; set; }
            public int CustId { get; set; }
            public string UserName { get; set; }
            public TpAdjustmentOptionEnum Option { get; set; }
            public decimal Amount { get; set; }
            public string Memo { get; set; }
            public int ReasonId { get; set; }
            public string TransId { get; set; }
            public MethodTypeIDEnum MethodType { get; set; }
        }

        public class GetLogRequest : ApiRequestPageModel
        {
            public int SiteId { get; set; }
            public int TpId { get; set; }
            public int CustId { get; set; }
            public string UserName { get; set; }
        }

        #endregion

        #region Finance > 3RD Funds Adjustment 

        /// <summary>
        /// TpFundsAdjustment/GetTpBySiteId 取得指定品牌第三方
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > 3RD Funds Adjustment > Third Party Name
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiResult<IEnumerable<SiteTpListIfo>> GetTpBySiteId(GetTpBySiteIdRequest request)
        {
            var apiListResult = new ApiResult<IEnumerable<SiteTpListIfo>>();

            var siteTpListInfo = _tpBll.GetSiteTpList(request.SiteId);
            apiListResult.Data = siteTpListInfo;
            apiListResult.ErrorCode = (int)ErrorCodeEnum.Success;
            return apiListResult;
        }

        /// <summary>
        /// TpFundsAdjustment/GetTpCustInfo 取得指定第三放客戶資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > 3RD Funds Adjustment > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public async Task<ApiResult<object>> GetTpCustInfoAsync(GetTpCustInfoRequest request)
        {
            var apiResult = new ApiResult<object>();

            if (string.IsNullOrWhiteSpace(request.UserTpId))
                request.UserTpId = null;
            if (string.IsNullOrWhiteSpace(request.UserName))
                request.UserName = null;

            var custId = (string.IsNullOrEmpty(request.UserTpId)) ? null : _common.TpIdToCustId(request.UserTpId);
            var custInfosDetail = _custInfoBll.GetCustInfoDetails(custId, request.UserName, request.SiteId);

            if (custInfosDetail != null)
            {
                switch (custInfosDetail.Status)
                {
                    case 1:
                        {
                            var tpInfo = new TpInfo
                            {
                                TpId = request.TpId,
                                CustId = custInfosDetail.CustId,
                            };

                            var tpBalanceInfo = await _tpBll.GetBalanceAsync(tpInfo);

                            apiResult.Data = new
                            {
                                custInfosDetail.CustId,
                                custInfosDetail.SiteName,
                                custInfosDetail.UserName,
                                Currency = custInfosDetail.CurrencyId,
                                tpBalanceInfo.Data.balance,
                            };
                            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                            break;
                        }
                    case 2:
                        apiResult.Message = "Suspend Status.";
                        apiResult.ErrorCode = 999;
                        break;
                    default:
                        apiResult.Message = "Closed Status.";
                        apiResult.ErrorCode = 999;
                        break;
                }
            }
            else
            {
                apiResult.Message = "No find";
                apiResult.ErrorCode = (int)ErrorCodeEnum.CustNotFound;
            }

            return apiResult;
        }

        /// <summary>
        /// TpFundsAdjustment/Submit 提交調整資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > 3RD Funds Adjustment > [Submit] > add/minus
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public async Task<ApiResult> Submit(SubmitRequest request)
        {
            var user = User.Identity.Name;
            var isDeposit = (request.ReasonId == 1 && request.MethodType == MethodTypeIDEnum.ThirdWallet); // Methodtype為第3方且ReasonID=1(Deposit)
            var inTpTransfer = new AdminTpTransfer
            {
                TpId = request.TpId,
                CustId = request.CustId,
                Amount = request.Amount,
                direction = (TpDirection)(int)request.Option,
                Operator = user,
                Remarks = request.Memo,
                Category = request.ReasonId,
                IsDepositId = isDeposit
            };

            if (isDeposit && !string.IsNullOrWhiteSpace(request.TransId))
            {
                var m = Regex.Match(request.TransId, @"\d+");
                int.TryParse(m.Value, out var depositId);
                inTpTransfer.DepositId = depositId;
            }

            var result = await _tpBll.AdminAdjustmentTransferAsync(inTpTransfer);
            if (result.ErrorCode == (int)ErrorCodeEnum.Success)
            {
                var reasonList = _bankFinancialBll.GetReason("FunKey/3RDWallet");
                var reason = reasonList.FirstOrDefault(x => x.ReasonId == request.ReasonId)?.Description;
                var strDetail = string.Empty;
                if (isDeposit)
                {
                    var log = _paymentBll.GetAdjustmentDepositLogByTransId(result.Data.TransId);
                    strDetail = JsonConvert.SerializeObject(new
                    {
                        request.TpName,
                        request.UserName,
                        Option = request.Option.ToString(),
                        request.Amount,
                        request.Memo,
                        Reason = reason,
                        DepositTransId = request.TransId,
                        BeforeActualDeposit = log?.BeforeAmount,
                        AfterActualDeposit = log?.AfterAmount
                    });
                }
                else
                {
                    strDetail = JsonConvert.SerializeObject(new
                    {
                        request.TpName,
                        request.UserName,
                        Option = request.Option.ToString(),
                        request.Amount,
                        request.Memo,
                        Reason = reason
                    });
                }

                _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                {
                    SiteId = request.SiteId,
                    CurrencyId = request.CurrencyId,
                    Operator = user,
                    ActionType = ActionType.Create,
                    MenuEnum = AdminAdjustmentLogMenuEnum.ThirdPartyFundsAdjustment,
                    BeforeDetailInfo = null,
                    AfterDetailInfo = strDetail,
                    RefTransId = result.Data.TransId
                });
            }

            return new ApiResult
            {
                ErrorCode = result.ErrorCode,
                Message = string.IsNullOrWhiteSpace(result.Message)
                    ? result.RawMessage : result.Message
            };
        }

        /// <summary>
        /// TpFundsAdjustment/GetLog 取得第三方調整紀錄
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > 3RD Funds Adjustment > [Submit] > Show Log
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiListResult<AdminTpTransferInfo> GetLog(GetLogRequest request)
        {
            var totalPages = 0;
            var rowCount = 0;
            var pageNumber = request.PageNumber;
            var result = new ApiListResult<AdminTpTransferInfo>();

            if (request.CustId <= 0 && !string.IsNullOrEmpty(request.UserName))
            {
                var custInfo = _custInfoBll.GetCustInfoDetails(null, request.UserName, request.SiteId);
                if (custInfo == null)
                {
                    throw new AdminException(ErrorCodeEnum.CustNotFound);
                }

                request.CustId = custInfo.CustId;
            }

            try
            {
                result.Data = _tpBll.GetAdjustmentTpLog(request.SiteId, request.TpId, request.CustId, request.PageSize, ref pageNumber, ref totalPages, ref rowCount);
                foreach (var data in result.Data)
                {
                    data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                }

                result.PageNumber = request.PageNumber;
                result.RowCount = rowCount;
                result.TotalPages = totalPages;
                result.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                result.ErrorCode = (int)ErrorCodeEnum.SystemException;
                result.RawMessage = ex.ToString();
            }

            return result;
        }

        #endregion
    }
}
