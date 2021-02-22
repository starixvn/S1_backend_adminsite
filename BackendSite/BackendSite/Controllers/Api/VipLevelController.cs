using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]

    public class VipLevelController : ControllerBase
    {
        private readonly VipLevelBLL vipLevelBLL;
        private readonly CustInfoBLL custInfoBLL;
        private readonly AuthorizeBLL authorizeBLL;

        public VipLevelController(VipLevelBLL vipLevelBLL, CustInfoBLL custInfoBLL, AuthorizeBLL authorizeBLL)
        {
            this.vipLevelBLL = vipLevelBLL;
            this.custInfoBLL = custInfoBLL;
            this.authorizeBLL = authorizeBLL;
        }
        public class CustIdReq
        {
            public int CustId { get; set; }
            public int VipLevel { get; set; }
            public int Manually { get; set; }
        }
        [HttpPost]
        public ApiListResult<VipLevelSetting> GetVipLevelSetting(CustIdReq custIdReq)
        {
            var custInfo = custInfoBLL.GetCustInfoWithCache(custIdReq.CustId);
            var apiListResult = new ApiListResult<VipLevelSetting>();
            apiListResult.Data = vipLevelBLL.GetVipLevelSetting(custInfo.SiteId);
            apiListResult.ErrorCode = 0;
            return apiListResult;
        }
        [HttpPost]
        public ApiResult<VipLevelInfo> GetVipLevelInfo(CustIdReq custIdReq)
        {
            var apiListResult = new ApiResult<VipLevelInfo>();
            apiListResult.Data = vipLevelBLL.GetVipLevelInfo(custIdReq.CustId);
            apiListResult.ErrorCode = 0;
            return apiListResult;
        }
        [HttpPost]
        public ApiListResult<VipLevelLog> GetVipLevelLog(CustIdReq custIdReq)
        {
            var apiListResult = new ApiListResult<VipLevelLog>();
            apiListResult.Data = vipLevelBLL.GetVipLevelLog(custIdReq.CustId);
            apiListResult.ErrorCode = 0;
            return apiListResult;
        }
        [HttpPost]
        public ApiResult SetVipLevel(CustIdReq custIdReq)
        {
            var apiResult = new ApiResult<dynamic>();

            try
            {
                var custInfo = custInfoBLL.GetCustInfoWithCache(custIdReq.CustId);
                var authData = authorizeBLL.GetAuthData();
                var result = vipLevelBLL.SetVipLevel(custInfo.SiteId, custInfo.CustId, custInfo.UserName, custIdReq.VipLevel, authData.UserName);
                apiResult.Data = result;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = 999;
            }

            return apiResult;
        }
        [HttpPost]
        public ApiResult SetVipLevelManually(CustIdReq custIdReq)
        {
            var apiResult = new ApiResult<dynamic>();

            try
            {
                var custInfo = custInfoBLL.GetCustInfoWithCache(custIdReq.CustId);
                var authData = authorizeBLL.GetAuthData();
                var result = vipLevelBLL.SetVipLevelManually(custInfo.SiteId, custInfo.CustId, custInfo.UserName, custIdReq.Manually, authData.UserName);
                apiResult.Data = result;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = 999;
            }

            return apiResult;
        }

    }
}
