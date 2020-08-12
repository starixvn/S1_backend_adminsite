using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.Library;
using BackendSite.Attributes;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TpController : ControllerBase
    {
        private readonly TpBLL tpBLL;
        private readonly Common common;
        private readonly CustInfoBLL custInfoBLL;

        public TpController(TpBLL tpBLL, Common common, CustInfoBLL custInfoBLL)
        {
            this.tpBLL = tpBLL;
            this.common = common;
            this.custInfoBLL = custInfoBLL;
        }

        public class TransferInfoReq
        {
            public int PageNumber { get; set; }
            public int CustId { get; set; }
            public string TransId { get; set; }
            public long startDate { get; set; }
            public long endDate { get; set; }
        }
        public ApiListResult<TpTransferInfo> GetTpTransferInfoByCust(TransferInfoReq transferInfoReq)
        {
            var apiListResult = new ApiListResult<TpTransferInfo>();
            int pageNumber = transferInfoReq.PageNumber;
            int totalPages = 0;
            int rowCount = 0;
            var transferInfoList = tpBLL.GetTpTransferInfoByCust(transferInfoReq.CustId, transferInfoReq.TransId, transferInfoReq.startDate, transferInfoReq.endDate, 10, ref pageNumber, out totalPages, out rowCount);
            apiListResult.Data = transferInfoList;
            apiListResult.ErrorCode = 0;
            apiListResult.PageNumber = pageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;

            return apiListResult;
        }
        public class FixNegativeReq
        {
            public int CustId { get; set; }
        }


        public async Task<ApiResult> FixNegativeAsync(FixNegativeReq fixNegativeReq)
        {
            TpInfo tpInfo = new TpInfo
            {
                CustId = fixNegativeReq.CustId,
                TpId = 1
            };

            var apiResult = await tpBLL.FixNegativeAsync(tpInfo);
            return apiResult;
        }
        [HttpPost]
        public ApiResult<IEnumerable<SiteTpListIfo>> GetTpBySite([FromForm] int SiteId)
        {
            var apiListResult = new ApiResult<IEnumerable<SiteTpListIfo>>();

            var SiteTpListInfo = tpBLL.GetSiteTpList(SiteId);
            apiListResult.Data = SiteTpListInfo;
            apiListResult.ErrorCode = 0;
            return apiListResult;
        }
        [HttpPost]
        public async Task<ApiResult<dynamic>> GetTpCustInfo([FromForm] int SiteId, [FromForm] int TpId, [FromForm] string UserName, [FromForm] string UserTpId)
        {
            var apiResult = new ApiResult<dynamic>();
            var custId = (string.IsNullOrEmpty(UserTpId)) ? null : common.TpIdToCustId(UserTpId);
            var custInfosDetail = custInfoBLL.GetCustInfoDetails(custId, UserName, SiteId);

            if (custInfosDetail != null)
            {
                if (custInfosDetail.Status == 1)
                {
                    var tpInfo = new TpInfo
                    {
                        TpId = TpId,
                        CustId = custInfosDetail.CustId,
                    };

                    var TpBalanceInfo = await tpBLL.GetBalanceAsync(tpInfo);

                    apiResult.Data = new
                    {
                        CustId = custInfosDetail.CustId,
                        SiteName = custInfosDetail.SiteName,
                        UserName = custInfosDetail.UserName,
                        Currency = custInfosDetail.CurrencyId,
                        balance = TpBalanceInfo.Data.balance,
                    };
                    apiResult.ErrorCode = 0;
                }
                else if (custInfosDetail.Status == 2)
                {
                    apiResult.Message = "Suspend Status.";
                    apiResult.ErrorCode = 999;
                }
                else
                {
                    apiResult.Message = "Closed Status.";
                    apiResult.ErrorCode = 999;
                }
            }
            else
            {
                apiResult.Message = "No find";
                apiResult.ErrorCode = 999;
            }

            return apiResult;
        }
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/ThirdPartyAdjustment" })]
        public async Task<ApiResult> SetAdjustmentTpCustInfo([FromForm] int SiteId, [FromForm] int TpId, [FromForm] string UserName, [FromForm] string UserTpId, [FromForm] int Opt, [FromForm] int Amount, [FromForm] string Memo)
        {
            var apiResult = new ApiResult();
            var custId = (string.IsNullOrEmpty(UserTpId)) ? null : common.TpIdToCustId(UserTpId);
            var custInfosDetail = custInfoBLL.GetCustInfoDetails(custId, UserName, SiteId);

            AdminTpTransfer inTpTransfer = new AdminTpTransfer() { TpId = TpId, CustId = custInfosDetail.CustId, Amount = Amount, direction = (TpDirection)Opt, Operator = User.Identity.Name, Remarks = Memo };

            var result = await tpBLL.AdminAdjustmentTransferAsync(inTpTransfer);
            apiResult.ErrorCode = result.ErrorCode;
            apiResult.Message = result.Data.ThirdId;

            return apiResult;
        }
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/ThirdPartyAdjustment" })]
        public ApiListResult<AdminTpTransferInfo> GetAdjustmentTpLog([FromForm] int SiteId, [FromForm] int TpId, [FromForm] int CustId, [FromForm] int PageNumber)
        {
            int PageSize = 10;
            int TotalPages = 0;
            int RowCount = 0;

            var result = new ApiListResult<AdminTpTransferInfo>();
            try
            {
                result.Data = tpBLL.GetAdjustmentTpLog(SiteId, TpId, CustId, PageSize, ref PageNumber, ref TotalPages, ref RowCount);

                result.PageNumber = PageNumber;
                result.RowCount = RowCount;
                result.TotalPages = TotalPages;
                result.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                result.ErrorCode = 999;
                result.RawMessage = ex.ToString();
            }

            return result;
        }
    }
}