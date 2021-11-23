using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Finance.Adjustment
{
    [Authorize]
    [Route("api/[controller]/[action]")]
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
        
        public class GetTpBySiteReq
        {
            public int SiteId { get; set; }
        }

        public class GetTpCustInfoReq
        {
            public int SiteId { get; set; }
            public int TpId { get; set; } 
            public string UserName { get; set; }
            public string UserTpId { get; set; }
        }
        
        public class SetAdjustmentTpCustInfoReq
        {
            public int SiteId { get; set; }
            public int TpId { get; set; }
            public string UserName { get; set; }
            public string UserTpId { get; set; }
            public int Opt { get; set; }
            public decimal Amount { get; set; } 
            public string Memo { get; set; }
        }

        public class GetAdjustmentTpLogReq
        {
            public int SiteId { get; set; }
            public int TpId { get; set; } 
            public int CustId { get; set; } 
            public int PageNumber { get; set; }
        }
        
        #endregion

        #region Finance > 3RD Funds Adjustment 

        /// <summary>
        /// TpFundsAdjustment/GetTpBySite
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > 3RD Funds Adjustment > Third Party Name
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult<IEnumerable<SiteTpListIfo>> GetTpBySite(GetTpBySiteReq req)
        {
            var apiListResult = new ApiResult<IEnumerable<SiteTpListIfo>>();

            var siteTpListInfo = _tpBll.GetSiteTpList(req.SiteId);
            apiListResult.Data = siteTpListInfo;
            apiListResult.ErrorCode = (int) ErrorCodeEnum.Success;
            return apiListResult;
        }
        
        /// <summary>
        /// TpFundsAdjustment/GetTpCustInfo
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > 3RD Funds Adjustment > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public async Task<ApiResult<dynamic>> GetTpCustInfo(GetTpCustInfoReq req)
        {
            var apiResult = new ApiResult<dynamic>();
            var custId = (string.IsNullOrEmpty(req.UserTpId)) ? null : _common.TpIdToCustId(req.UserTpId);
            var custInfosDetail = _custInfoBll.GetCustInfoDetails(custId, req.UserName, req.SiteId);

            if (custInfosDetail != null)
            {
                switch (custInfosDetail.Status)
                {
                    case 1:
                    {
                        var tpInfo = new TpInfo
                        {
                            TpId = req.TpId,
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
                        apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
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
                apiResult.ErrorCode =(int) ErrorMessageCodeEnum.CustNotFound;
            }

            return apiResult;
        }
        
        /// <summary>
        /// TpFundsAdjustment/SetAdjustmentTpCustInfo
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > 3RD Funds Adjustment > [Submit] > add/minus
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/ThirdPartyAdjustment" })]
        public async Task<ApiResult> SetAdjustmentTpCustInfo(SetAdjustmentTpCustInfoReq req)
        {
            var apiResult = new ApiResult();
            var custId = (string.IsNullOrEmpty(req.UserTpId)) ? null : _common.TpIdToCustId(req.UserTpId);
            var custInfosDetail = _custInfoBll.GetCustInfoDetails(custId, req.UserName, req.SiteId);
            
            var inTpTransfer = new AdminTpTransfer
                {
                    TpId = req.TpId,
                    CustId = custInfosDetail.CustId,
                    Amount = req.Amount, 
                    direction = (TpDirection)req.Opt,
                    Operator = User.Identity.Name, 
                    Remarks = req.Memo 
                };
            var result = await _tpBll.AdminAdjustmentTransferAsync(inTpTransfer);
            if (result.ErrorCode == (int)ErrorCodeType.Success)
            {
                var param = new
                {
                    req.TpId,
                    custInfosDetail.CustId,
                    req.Amount,
                    req.Opt,
                    Remarks = req.Memo
                };
                _tpBll.SetAdjustmentTpCustInfoLog("3RDFundsAdjustment", 1, JsonConvert.SerializeObject(param), User.Identity.Name);
            }
            apiResult.ErrorCode = result.ErrorCode;
            apiResult.Message = string.IsNullOrWhiteSpace(result.Message) ? result.RawMessage : result.Message;

            return apiResult;
        }
        
        /// <summary>
        /// TpFundsAdjustment/GetAdjustmentTpLog
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > 3RD Funds Adjustment > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/ThirdPartyAdjustment" })]
        public ApiListResult<AdminTpTransferInfo> GetAdjustmentTpLog(GetAdjustmentTpLogReq req)
        {
            var pageSize = 10;
            var totalPages = 0;
            var rowCount = 0;
            var pageNumber = req.PageNumber;
            var result = new ApiListResult<AdminTpTransferInfo>();
            try
            {
                result.Data = _tpBll.GetAdjustmentTpLog(req.SiteId, req.TpId, req.CustId, pageSize, ref pageNumber, ref totalPages, ref rowCount);
                foreach (var data in result.Data)
                {
                    data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                }
                
                result.PageNumber = pageNumber;
                result.RowCount = rowCount;
                result.TotalPages = totalPages;
                result.ErrorCode = (int)ErrorCodeType.Success;
            }
            catch (Exception ex)
            {
                result.ErrorCode = (int) ErrorCodeEnum.SystemException;
                result.RawMessage = ex.ToString();
            }

            return result;
        }

        #endregion
    }
}