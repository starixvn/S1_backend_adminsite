using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TpController : ControllerBase
    {
        private readonly TpBLL tpBLL;

        public TpController(TpBLL tpBLL)
        {
            this.tpBLL = tpBLL;
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
    }
}