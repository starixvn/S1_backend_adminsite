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
    public class CustManageController : ControllerBase
    {
        private readonly CustInfoBLL custInfoBLL;

        public CustManageController(CustInfoBLL custInfoBLL)
        {
            this.custInfoBLL = custInfoBLL;
        }
        public class CustInfoListReq
        {
            public string UserName { get; set; }
            public string TransId { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }

        [HttpPost]
        public ApiListResult<dynamic> GetCustInfoList(CustInfoListReq custInfoListReq)
        {
            var apiResult = new ApiListResult<dynamic>();
            int totalPages = 0;
            int pageNumber = custInfoListReq.PageNumber;

            apiResult.Data = custInfoBLL.GetCustInfoList(custInfoListReq.UserName, custInfoListReq.TransId, custInfoListReq.PageSize, ref pageNumber, out totalPages);
            apiResult.TotalPages = totalPages;
            apiResult.PageNumber = pageNumber;
            apiResult.ErrorCode = 0;

            return apiResult;
        }

        public class CustInfoReq
        {
            public int CustId { get; set; }
        }

        [HttpPost]
        public ApiResult<dynamic> GetCustInfo(CustInfoReq custInfoReq)
        {
            var apiResult = new ApiResult<dynamic>();

            apiResult.Data = custInfoBLL.GetCustInfo(custInfoReq.CustId);
            apiResult.ErrorCode = 0;

            return apiResult;
        }
    }
}