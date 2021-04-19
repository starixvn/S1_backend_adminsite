using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using BackendSite.Service.Model.enums;

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
        private readonly ExcelBLL excelBLL;

        public VipLevelController(VipLevelBLL vipLevelBLL, CustInfoBLL custInfoBLL, AuthorizeBLL authorizeBLL, ExcelBLL excelBLL)
        {
            this.vipLevelBLL = vipLevelBLL;
            this.custInfoBLL = custInfoBLL;
            this.authorizeBLL = authorizeBLL;
            this.excelBLL = excelBLL;
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

        public class VipUpgradeReviewPara
        {
            public int SiteId { get; set; }
            public string YearMonth { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public int Status { get; set; }
            public string UserName { get; set; }
            public VipUpgradeReviewPara()
            {
                //PageSize預設20
                if (PageSize == 0)
                    PageSize = 20;
            }
        }

        [HttpPost]
        public ApiListResult<VipUpgradeList> GetVipUpgradeReview(VipUpgradeReviewPara para)
        {
            var apiResult = new ApiListResult<VipUpgradeList>();
            try
            {
                var SearchResult = vipLevelBLL.GetVipUpgradeReview(para.YearMonth, para.SiteId, para.PageNumber, para.PageSize, para.Status, para.UserName);
                apiResult.Data = SearchResult.VipUpgradeList;
                apiResult.PageNumber = SearchResult.PageNumber;
                apiResult.TotalPages = SearchResult.TotalPages;
                apiResult.RowCount = SearchResult.RowCount;

                if (vipLevelBLL.CanUpgradeVip(para.YearMonth))
                {
                    apiResult.ErrorCode = (int)GetVipUpgradeReviewErrorCode.CanUpgrade;
                }
                else
                {
                    apiResult.ErrorCode = (int)GetVipUpgradeReviewErrorCode.JustCanSearch;
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }
            return apiResult;
        }
        public class UpdateVipUpgradeReviewPara
        {
            public string CustIds { get; set; }
            public string YearMonth { get; set; }
            public int ChangeStatus { get; set; }
        }
        [HttpPost]
        public ApiResult UpdateVipUpgradeReview(UpdateVipUpgradeReviewPara para)
        {
            var apiResult = new ApiResult();
            try
            {
                if (vipLevelBLL.CanUpgradeVip(para.YearMonth))
                {
                    vipLevelBLL.UpdateVipUpgradeReview(para.CustIds, para.YearMonth, User.Identity.Name, para.ChangeStatus);
                }
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        public IActionResult ExportVipUpgradedata(int SiteId, string YearMonth, int Status, string UserName)
        {
            var ExportData = vipLevelBLL.GetVipUpgradedata(SiteId, YearMonth, Status, UserName);
            var stream = excelBLL.Export(ExportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"VipUpgrade_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

    }
}
