using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using BackendSite.ExceptionHandling;
using BackendSite.Service.Model.CustVipLevel;
using BackendSite.Service.Model.enums;
using System.Linq;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class VipLevelController : ControllerBase
    {
        #region Fields

        private readonly VipLevelBLL _vipLevelBLL;
        private readonly CustInfoBLL _custInfoBLL;
        private readonly AuthorizeBLL _authorizeBLL;
        private readonly ExcelBLL _excelBLL;

        #endregion

        #region Constructors

        public VipLevelController(VipLevelBLL vipLevelBLL,
            CustInfoBLL custInfoBLL,
            AuthorizeBLL authorizeBLL,
            ExcelBLL excelBLL)
        {
            _vipLevelBLL = vipLevelBLL;
            _custInfoBLL = custInfoBLL;
            _authorizeBLL = authorizeBLL;
            _excelBLL = excelBLL;
        }

        #endregion

        #region API Model

        public class CustIdReq
        {
            public int CustId { get; set; }
            public int VipLevel { get; set; }
            public int Manually { get; set; }
        }

        public class VipUpgradeReviewParameter
        {
            public int SiteId { get; set; }
            public string CalculateYearMonth { get; set; }
            public VipStatus Status { get; set; }
            public string UserName { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }

        }

        public class UpdateVipUpgradeReviewParameter
        {
            public IEnumerable<int> CustIdList { get; set; }
            public string CalculateYearMonth { get; set; }
            public int SiteId { get; set; }
            public VipStatus NewStatus { get; set; }
        }

        public class VipDowngradeReviewParameter
        {
            public int SiteId { get; set; }
            public string CalculateYearMonth { get; set; }
            public VipStatus Status { get; set; }
            public string UserName { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }

        public class UpdateVipDowngradeReviewParameter
        {
            public IEnumerable<int> CustIdList { get; set; }
            public string CalculateYearMonth { get; set; }
            public int SiteId { get; set; }
            public VipStatus NewStatus { get; set; }
        }

        #endregion

        #region Actions

        #region CustInfo > VipLevel

        /// <summary>
        /// VipLevel/GetVipLevelSetting 取得VIP等級設定資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > VIP Level > Get Vip Level Setting
        /// 
        /// </remarks>
        /// <param name="custIdReq"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<VipLevelSetting> GetVipLevelSetting(CustIdReq custIdReq)
        {
            var custInfo = _custInfoBLL.GetCustInfoWithCache(custIdReq.CustId);
            var apiListResult = new ApiListResult<VipLevelSetting>
            {
                Data = _vipLevelBLL.GetVipLevelSetting(custInfo.SiteId),
                ErrorCode = 0
            };
            return apiListResult;
        }

        /// <summary>
        /// VipLevel/GetVipLevelInfo 取得指定客戶VIP等級資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > VIP Level > Cust Level
        /// 
        /// </remarks>
        /// <param name="custIdReq"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<VipLevelInfo> GetVipLevelInfo(CustIdReq custIdReq)
        {
            var data = _vipLevelBLL.GetVipLevelInfo(custIdReq.CustId);

            data.QualifiedTime = new DateTimeOffset(data.QualifiedTime.DateTime.AddHours(8), TimeSpan.FromHours(8));

            var apiListResult = new ApiResult<VipLevelInfo>
            {
                Data = data,
                ErrorCode = 0
            };
            return apiListResult;
        }

        /// <summary>
        /// VipLevel/GetVipLevelLog 取得指定客戶VIP等級紀錄
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > VIP Level > Cust Level Log
        /// 
        /// </remarks>
        /// <param name="custIdReq"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<VipLevelLog> GetVipLevelLog(CustIdReq custIdReq)
        {
            var data = _vipLevelBLL.GetVipLevelLog(custIdReq.CustId);

            data.ToList().ForEach(s => s.UpdateTime = new DateTimeOffset(s.UpdateTime.DateTime.AddHours(8), TimeSpan.FromHours(8)));

            var apiListResult = new ApiListResult<VipLevelLog>
            {
                Data = data,
                ErrorCode = 0
            };
            return apiListResult;
        }

        /// <summary>
        /// VipLevel/SetVipLevel 設定指定客戶VIP等級
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > VIP Level > Cust Level > Click [Edit]
        /// 
        /// </remarks>
        /// <param name="custIdReq"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult SetVipLevel(CustIdReq custIdReq)
        {
            var apiResult = new ApiResult<dynamic>();

            try
            {
                var custInfo = _custInfoBLL.GetCustInfoWithCache(custIdReq.CustId);
                var authData = _authorizeBLL.GetAuthData();
                var result = _vipLevelBLL.SetVipLevel(custInfo.SiteId, custInfo.CustId, custInfo.UserName,
                    custIdReq.VipLevel, authData.UserName);
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

        /// <summary>
        /// VipLevel/SetVipLevelManually 設定VIP等級為手動
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > VIP Level > Cust Level > Click [Manually]
        /// 
        /// </remarks>
        /// <param name="custIdReq"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult SetVipLevelManually(CustIdReq custIdReq)
        {
            var apiResult = new ApiResult<dynamic>();

            try
            {
                var custInfo = _custInfoBLL.GetCustInfoWithCache(custIdReq.CustId);
                var authData = _authorizeBLL.GetAuthData();
                var result = _vipLevelBLL.SetVipLevelManually(custInfo.SiteId, custInfo.CustId, custInfo.UserName,
                    custIdReq.Manually, authData.UserName);
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

        #endregion

        #region VIP Review

        [HttpPost]
        public ApiListResult<VipUpgradeReviewDetailModel> GetVipUpgradeReviewList(VipUpgradeReviewParameter request)
        {
            var result = _vipLevelBLL.GetVipUpgradeReviewList(request.SiteId,
                request.CalculateYearMonth,
                request.Status,
                request.UserName,
                request.PageNumber,
                request.PageSize);

            return new ApiListResult<VipUpgradeReviewDetailModel>
            {
                Data = result.Data,
                ErrorCode = (int)ErrorCodeEnum.Success,
                PageNumber = result.PageNumber,
                TotalPages = result.TotalPages,
                RowCount = result.RowCount
            };
        }

        [HttpPost]
        public IActionResult ExportVipUpgradeReviewList(int SiteId,
            string YearMonth,
            int Status,
            string UserName)
        {
            var data = _vipLevelBLL.ExportVipUpgradeReviewList(SiteId,
                YearMonth,
                (VipStatus)Status,
                UserName);

            var stream = _excelBLL.Export(data);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"VipUpgrade_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        [HttpPost]
        public ApiResult UpdateVipUpgradeReview(UpdateVipUpgradeReviewParameter request)
        {
            if (_vipLevelBLL.UpdateVipUpgradeReview(request.CustIdList,
                request.CalculateYearMonth,
                request.SiteId,
                request.NewStatus,
                User.Identity.Name))
            {
                //success
                return new ApiResult()
                {
                    ErrorCode = 0,
                    Message = "Update Successful."
                };
            }
            else
            {
                //fail
                return new ApiResult()
                {
                    ErrorCode = -1,
                    Message = "Update Fail."
                };
            }
        }

        [HttpPost]
        public ApiListResult<VipDowngradeReviewDetailModel> GetVipDowngradeReviewList(VipDowngradeReviewParameter request)
        {
            var result = _vipLevelBLL.GetVipDowngradeReviewList(request.SiteId,
                request.CalculateYearMonth,
                request.Status,
                request.UserName,
                request.PageNumber,
                request.PageSize);

            return new ApiListResult<VipDowngradeReviewDetailModel>
            {
                Data = result.Data,
                ErrorCode = (int)ErrorCodeEnum.Success,
                PageNumber = result.PageNumber,
                TotalPages = result.TotalPages,
                RowCount = result.RowCount
            };

            //不應該用errorcode判斷.........這個條件要再確認
            // if (_vipLevelBLL.CanUpgradeVip(request.YearMonth))
            // {
            //     apiResult.ErrorCode = (int) GetVipUpgradeReviewErrorCode.CanUpgrade;
            // }
            // else
            // {
            //     apiResult.ErrorCode = (int) GetVipUpgradeReviewErrorCode.JustCanSearch;
            // }
            //
            // return apiResult;
        }

        [HttpPost]
        public IActionResult ExportVipDowngradeReviewList(int SiteId,
            string CalculateYearMonth,
            int Status,
            string UserName)
        {
            var exportData = _vipLevelBLL.ExportVipDowngradeReviewList(SiteId,
                CalculateYearMonth,
                (VipStatus)Status,
                UserName);

            var stream = _excelBLL.Export(exportData);

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"VipDowngrade_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        [HttpPost]
        public ApiResult UpdateVipDowngradeReview(UpdateVipDowngradeReviewParameter request)
        {
            if (_vipLevelBLL.UpdateVipDowngradeReview(request.CustIdList,
                request.CalculateYearMonth,
                request.SiteId,
                request.NewStatus,
                User.Identity.Name))
            {
                //success
                return new ApiResult()
                {
                    ErrorCode = 0,
                    Message = "Update Successful."
                };
            }
            else
            {
                //fail
                return new ApiResult()
                {
                    ErrorCode = -1,
                    Message = "Update Fail."
                };
            }
        }

        #endregion

        #endregion
    }
}