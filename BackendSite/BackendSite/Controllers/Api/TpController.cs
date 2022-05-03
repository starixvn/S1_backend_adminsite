using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.Library;
using BackendSite.Attributes;
using BackendSite.Extensions;
using Newtonsoft.Json;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class TpController : ControllerBase
    {
        private readonly TpBLL tpBLL;
        private readonly Common common;
        private readonly CustInfoBLL custInfoBLL;
        private readonly SelLangBLL _selLangBLL;

        public TpController(IServiceProvider provider)
        {
            this.tpBLL = provider.GetRequiredService<TpBLL>();
            this.common = provider.GetRequiredService<Common>();
            this.custInfoBLL = provider.GetRequiredService<CustInfoBLL>();
            this._selLangBLL = provider.GetRequiredService<SelLangBLL>();
        }

        public class TransferInfoReq
        {
            public int PageNumber { get; set; }
            public int CustId { get; set; }
            public string TransId { get; set; }
            public long startDate { get; set; }
            public long endDate { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Use：
        /// CSD > Customer Administration > CustInfo > Transfer Info
        /// </remarks>
        /// <param name="transferInfoReq"></param>
        /// <returns></returns>
        public ApiListResult<TpTransferInfo> GetTpTransferInfoByCust(TransferInfoReq transferInfoReq)
        {
            var pageNumber = transferInfoReq.PageNumber;
            var totalPages = 0;
            var rowCount = 0;

            if (transferInfoReq.startDate > 2147483647)
                transferInfoReq.startDate = 2147483647;

            if (transferInfoReq.endDate > 2147483647)
                transferInfoReq.endDate = 2147483647;

            var startDate = DateTimeExtensions.UnixToDateTime(transferInfoReq.startDate).AddHours(-8); ;
            var endDate = DateTimeExtensions.UnixToDateTime(transferInfoReq.endDate).AddHours(-8); ;

            var data = tpBLL.GetTpTransferInfoByCust(transferInfoReq.CustId,
                                                     transferInfoReq.TransId,
                                                     startDate,
                                                     endDate,
                                                     10,
                                                     transferInfoReq.PageNumber,
                                                     out totalPages,
                                                     out rowCount);

            return new ApiListResult<TpTransferInfo>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success,
                TotalPages = totalPages,
                PageNumber = transferInfoReq.PageNumber,
                RowCount = rowCount
            };
        }

        public class FixNegativeReq
        {
            public int CustId { get; set; }
        }


        public async Task<ApiResult> FixNegativeAsync(FixNegativeReq fixNegativeReq)
        {
            var tpInfo = new TpInfo
            {
                CustId = fixNegativeReq.CustId,
                TpId = 1
            };

            var apiResult = await tpBLL.FixNegativeAsync(tpInfo);
            apiResult.Message = string.IsNullOrWhiteSpace(apiResult.Message) ? apiResult.RawMessage : apiResult.Message;

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
        public async Task<ApiResult> SetAdjustmentTpCustInfo([FromForm] int SiteId, [FromForm] int TpId, [FromForm] string UserName, [FromForm] string UserTpId, [FromForm] int Opt, [FromForm] decimal Amount, [FromForm] string Memo, [FromForm] int Category)
        {
            var apiResult = new ApiResult();
            var custId = (string.IsNullOrEmpty(UserTpId)) ? null : common.TpIdToCustId(UserTpId);
            var custInfosDetail = custInfoBLL.GetCustInfoDetails(custId, UserName, SiteId);
            
            var inTpTransfer = new AdminTpTransfer()
            {
                TpId = TpId,
                CustId = custInfosDetail.CustId,
                Amount = Amount,
                direction = (TpDirection)Opt,
                Operator = User.Identity.Name,
                Remarks = Memo,
                Category = Category
            };

            var result = await tpBLL.AdminAdjustmentTransferAsync(inTpTransfer);
            if (result.ErrorCode == (int)ErrorCodeEnum.Success)
            {
                var param = new
                {
                    TpId,
                    custInfosDetail.CustId,
                    Amount,
                    Opt,
                    Remarks = Memo
                };
                tpBLL.SetAdjustmentTpCustInfoLog("3RDFundsAdjustment", 1, JsonConvert.SerializeObject(param), User.Identity.Name);
            }
            apiResult.ErrorCode = result.ErrorCode;
            apiResult.Message = string.IsNullOrWhiteSpace(result.Message) ? result.RawMessage : result.Message;

            return apiResult;
        }
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/ThirdPartyAdjustment" })]
        public ApiListResult<AdminTpTransferInfo> GetAdjustmentTpLog([FromForm] int SiteId, [FromForm] int TpId, [FromForm] int CustId, [FromForm] int PageNumber)
        {
            var PageSize = 10;
            var TotalPages = 0;
            var RowCount = 0;

            var result = new ApiListResult<AdminTpTransferInfo>();
            try
            {
                result.Data = tpBLL.GetAdjustmentTpLog(SiteId, TpId, CustId, PageSize, ref PageNumber, ref TotalPages, ref RowCount);

                foreach (var data in result.Data)
                {
                    data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
                }
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
        public class TpDetailPara
        {
            public int Site { get; set; }
            public int Currency { get; set; }
        }
        [HttpPost]
        public ApiResult<IEnumerable<SiteTpListIfo>> GetTpBySiteAndCurrency(TpDetailPara TpDetailPara)
        {
            var apiListResult = new ApiResult<IEnumerable<SiteTpListIfo>>();

            var TPList = tpBLL.GetTpBySiteAndCurrency(TpDetailPara);
            apiListResult.Data = TPList;
            apiListResult.ErrorCode = 0;
            return apiListResult;
        }

        [HttpPost]
        public async Task<ApiResult<dynamic>> GetTpBetRecordDetailAsync(GetTpBetRecordDetailRequest request)
        {
            return await tpBLL.GetBetDetail(request);
        }
    }
}