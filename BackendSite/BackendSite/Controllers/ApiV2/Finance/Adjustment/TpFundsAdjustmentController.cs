using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Finance.Adjustment
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class TpFundsAdjustmentController : ControllerBase
    {
        #region Fields

        private readonly TpBLL _tpBll;
        private readonly Common _common;
        private readonly CustInfoBLL _custInfoBll;

        #endregion

        #region Constructors

        public TpFundsAdjustmentController(IServiceProvider provider)
        {
            _tpBll = provider.GetRequiredService<TpBLL>();
            _common = provider.GetRequiredService<Common>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
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
            public int TpId { get; set; }
            public string UserName { get; set; }
            public string UserTpId { get; set; }
            public int Opt { get; set; }
            public decimal Amount { get; set; }
            public string Memo { get; set; }
        }

        public class GetLogRequest : ApiRequestPageModel
        {
            public int SiteId { get; set; }
            public int TpId { get; set; }
            public int CustId { get; set; }
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/ThirdPartyAdjustment" })]
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/ThirdPartyAdjustment" })]
        [HttpPost]
        public async Task<ApiResult<dynamic>> GetTpCustInfo(GetTpCustInfoRequest request)
        {
            var apiResult = new ApiResult<dynamic>();

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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/ThirdPartyAdjustment" })]
        [HttpPost]
        public async Task<ApiResult> Submit(SubmitRequest request)
        {
            var apiResult = new ApiResult();
            var custId = (string.IsNullOrEmpty(request.UserTpId)) ? null : _common.TpIdToCustId(request.UserTpId);
            var custInfosDetail = _custInfoBll.GetCustInfoDetails(custId, request.UserName, request.SiteId);

            var inTpTransfer = new AdminTpTransfer
            {
                TpId = request.TpId,
                CustId = custInfosDetail.CustId,
                Amount = request.Amount,
                direction = (TpDirection)request.Opt,
                Operator = User.Identity.Name,
                Remarks = request.Memo
            };
            var result = await _tpBll.AdminAdjustmentTransferAsync(inTpTransfer);
            if (result.ErrorCode == (int)ErrorCodeEnum.Success)
            {
                var param = new
                {
                    request.TpId,
                    custInfosDetail.CustId,
                    request.Amount,
                    request.Opt,
                    Remarks = request.Memo
                };
                _tpBll.SetAdjustmentTpCustInfoLog("3RDFundsAdjustment", 1, JsonConvert.SerializeObject(param), User.Identity.Name);
            }
            apiResult.ErrorCode = result.ErrorCode;
            apiResult.Message = string.IsNullOrWhiteSpace(result.Message) ? result.RawMessage : result.Message;

            return apiResult;
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/ThirdPartyAdjustment" })]
        [HttpPost]
        public ApiListResult<AdminTpTransferInfo> GetLog(GetLogRequest request)
        {
            var totalPages = 0;
            var rowCount = 0;
            var pageNumber = request.PageNumber;
            var result = new ApiListResult<AdminTpTransferInfo>();
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