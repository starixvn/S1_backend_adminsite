using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.CustVipLevel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Member.Process
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class VipLevelReviewController : ControllerBase
    {
        #region Fields

        private readonly VipLevelBLL _vipLevelBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructor

        public VipLevelReviewController(IServiceProvider provider)
        {
            _vipLevelBll = provider.GetRequiredService<VipLevelBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region API Model

        public class VipUpgradeReviewReq : ApiRequestPageModel
        {
            public int SiteId { get; set; }
            public string CalculateYearMonth { get; set; }
            public VipSettingStatusEnum VipType { get; set; }
            public VipStatus Status { get; set; }
            public string UserName { get; set; }
        }
        
        public class UpdateVipUpgradeReviewReq
        {
            public IEnumerable<int> CustIdList { get; set; }
            public string CalculateYearMonth { get; set; }
            public int SiteId { get; set; }
            public VipStatus NewStatus { get; set; }
        }
        
        public class VipDowngradeReviewReq : ApiRequestPageModel
        {
            public int SiteId { get; set; }
            public string CalculateYearMonth { get; set; }
            public VipStatus Status { get; set; }
            public string UserName { get; set; }
        }
        
        public class ExportVipUpgradeReviewListReq
        {
            public int SiteId { get; set; }
            public string YearMonth { get; set; }
            public int Status { get; set; }
            public string UserName { get; set; }
        }
        
        public class ExportVipDowngradeReviewListReq
        {
            public int SiteId { get; set; }
            public string CalculateYearMonth { get; set; }
            public int Status { get; set; }
            public string UserName { get; set; }
        }

        #endregion

        #region VIP Review

        /// <summary>
        /// VipLevelReview/GetUpgradeList 取得VIP升級檢視清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Vip Level Review > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VipLevelReview" })]
        [HttpPost]
        public ApiListResult<VipUpgradeReviewDetailModel> GetUpgradeList(VipUpgradeReviewReq req)
        {
            var result = _vipLevelBll.GetVipUpgradeReviewList(req.SiteId, req.CalculateYearMonth, req.Status, req.UserName, req.PageNumber, req.PageSize);

            foreach (var data in result.Data)
            {
                data.CalculateTime = new DateTimeOffset(data.CalculateTime.DateTime, TimeSpan.Zero);
                if (data.SettleTime != null)
                    data.SettleTime = new DateTimeOffset(data.SettleTime.Value.DateTime, TimeSpan.Zero);
                if (data.QualifiedTime != null)
                    data.QualifiedTime = new DateTimeOffset(data.QualifiedTime.Value.DateTime, TimeSpan.Zero);
            }
            
            return new ApiListResult<VipUpgradeReviewDetailModel>
            {
                Data = result.Data,
                ErrorCode = (int)ErrorCodeEnum.Success,
                PageNumber = result.PageNumber,
                TotalPages = result.TotalPages,
                RowCount = result.RowCount
            };
        }

        /// <summary>
        /// VipLevelReview/ExportUpgradeList 匯出VIP升級檢視清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Vip Level Review > [Submit] > [Export]
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VipLevelReview", "viplevelreview_export_excel" })]
        [HttpPost]
        public IActionResult ExportUpgradeList(ExportVipUpgradeReviewListReq req)
        {
            var data = _vipLevelBll.ExportVipUpgradeReviewList(req.SiteId, req.YearMonth, (VipStatus)req.Status, req.UserName);
            var exportList = data.Select(d => new ExportVipUpgradeReviewModel
            {
                SiteId = d.SiteId,
                SiteName = d.SiteName,
                CustId = d.CustId,
                UserName = d.UserName,
                CurrencyId = d.CurrencyId,
                IsoCode = d.IsoCode,
                CurrentVipLevel = d.CurrencyId,
                CurrentVipName = d.CurrentVipName,
                UpgradeVipLevel = d.UpgradeVipLevel,
                UpgradeVipName = d.UpgradeVipName,
                ThresholdDeposit = d.ThresholdDeposit,
                DepositAmount = d.DepositAmount,
                UpgradeBonus = d.UpgradeBonus,
                Status = d.Status,
                CalculateTime = new DateTimeOffset(d.CalculateTime.DateTime, TimeSpan.Zero ).DateTime.ToString(),
                SettleTime = d.SettleTime == null ? "" : new DateTimeOffset(d.SettleTime.Value.DateTime, TimeSpan.Zero ).DateTime.ToString(),
                QualifiedTime = d.QualifiedTime == null ? "First Time Upgrade" : new DateTimeOffset(d.QualifiedTime.Value.DateTime, TimeSpan.Zero ).DateTime.ToString(),
                LastUpdateUser = d.LastUpdateUser
            }).ToList();
            var stream = _excelBll.Export(exportList);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"VipUpgrade_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        /// <summary>
        /// VipLevelReview/UpdateUpgrade 更新升級檢視
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Vip Level Review > [Submit] > [Approved] / [All Approved]
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VipLevelReview", "viplevelreview_update" })]
        [HttpPost]
        public ApiResult UpdateUpgrade(UpdateVipUpgradeReviewReq req)
        {
            if (_vipLevelBll.UpdateVipUpgradeReview(req.CustIdList, req.CalculateYearMonth, req.SiteId, req.NewStatus, User.Identity.Name))
            {
                //success
                return new ApiResult()
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Successful."
                };
            }
            else
            {
                //fail
                return new ApiResult()
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Update Fail."
                };
            }
        }

        /// <summary>
        /// VipLevelReview/GetDowngradeList 取得VIP降級檢視清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Vip Level Review  > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VipLevelReview" })]
        [HttpPost]
        public ApiListResult<VipDowngradeReviewDetailModel> GetDowngradeList(VipDowngradeReviewReq req)
        {
            var result = _vipLevelBll.GetVipDowngradeReviewList(req.SiteId, req.CalculateYearMonth, req.Status, req.UserName, req.PageNumber, req.PageSize);
            foreach (var data in result.Data)
            {
                data.CalculateTime = new DateTimeOffset(data.CalculateTime.DateTime, TimeSpan.Zero);
                if (data.SettleTime != null)
                    data.SettleTime = new DateTimeOffset(data.SettleTime.Value.DateTime, TimeSpan.Zero);
                if (data.QualifiedTime != null)
                    data.QualifiedTime = new DateTimeOffset(data.QualifiedTime.Value.DateTime, TimeSpan.Zero);
            }
            
            return new ApiListResult<VipDowngradeReviewDetailModel>
            {
                Data = result.Data,
                ErrorCode = (int)ErrorCodeEnum.Success,
                PageNumber = result.PageNumber,
                TotalPages = result.TotalPages,
                RowCount = result.RowCount
            };
        }

        /// <summary>
        /// VipLevelReview/ExportDowngradeList 匯出VIP降級檢視清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Vip Level Review  > [Submit] > [Export]
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VipLevelReview", "viplevelreview_export_excel" })]
        [HttpPost]
        public IActionResult ExportDowngradeList(ExportVipDowngradeReviewListReq req)
        {
            var data = _vipLevelBll.ExportVipDowngradeReviewList(req.SiteId, req.CalculateYearMonth, (VipStatus)req.Status, req.UserName);
            var exportList = data.Select(d => new ExportVipDowngradeReviewDataModel
            {
                SiteId = d.SiteId,
                SiteName = d.SiteName,
                CustId = d.CustId,
                UserName = d.UserName,
                CurrencyId = d.CurrencyId,
                IsoCode = d.IsoCode,
                CurrentVipLevel = d.CurrencyId,
                CurrentVipName = d.CurrentVipName,
                DowngradeVipLevel = d.DowngradeVipLevel,
                DowngradeVipName = d.DowngradeVipName,
                ThresholdDeposit = d.ThresholdDeposit,
                DepositAmount = d.DepositAmount,
                ThresholdTurnOver = d.ThresholdTurnOver,
                TurnOverAmount = d.TurnOverAmount,
                Status = d.Status,
                CalculateTime = new DateTimeOffset(d.CalculateTime.DateTime, TimeSpan.Zero ).DateTime.ToString(),
                SettleTime = d.SettleTime == null ? "" : new DateTimeOffset(d.SettleTime.Value.DateTime, TimeSpan.Zero ).DateTime.ToString(),
                QualifiedTime = d.QualifiedTime == null ? "" : new DateTimeOffset(d.QualifiedTime.Value.DateTime, TimeSpan.Zero ).DateTime.ToString(),
                LastUpdateUser = d.LastUpdateUser
            }).ToList();
            
            var stream = _excelBll.Export(exportList);

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"VipDowngrade_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        /// <summary>
        /// VipLevelReview/UpdateDowngrade 更新降級檢視
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Vip Level Review > [Submit] > [Approved] / [All Approved]
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VipLevelReview", "viplevelreview_update" })]
        [HttpPost]
        public ApiResult UpdateDowngrade(UpdateVipUpgradeReviewReq req)
        {
            if (_vipLevelBll.UpdateVipDowngradeReview(req.CustIdList, req.CalculateYearMonth, req.SiteId, req.NewStatus, User.Identity.Name))
            {
                //success
                return new ApiResult()
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Successful."
                };
            }
            else
            {
                //fail
                return new ApiResult()
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Update Fail."
                };
            }
        }

        #endregion
    }
}