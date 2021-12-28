using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Controllers.Api;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.CustVipLevel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Member.Process
{
    [Authorize]
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

        public class VipUpgradeReviewReq
        {
            public int SiteId { get; set; }
            public string CalculateYearMonth { get; set; }
            public VipStatus Status { get; set; }
            public string UserName { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }
        
        public class UpdateVipUpgradeReviewReq
        {
            public IEnumerable<int> CustIdList { get; set; }
            public string CalculateYearMonth { get; set; }
            public int SiteId { get; set; }
            public VipStatus NewStatus { get; set; }
        }
        
        public class VipDowngradeReviewReq
        {
            public int SiteId { get; set; }
            public string CalculateYearMonth { get; set; }
            public VipStatus Status { get; set; }
            public string UserName { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
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
        /// VipLevelReview/GetVipUpgradeReviewList 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Vip Level Review > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<VipUpgradeReviewDetailModel> GetVipUpgradeReviewList(VipUpgradeReviewReq req)
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
        /// VipLevelReview/ExportVipUpgradeReviewList 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Vip Level Review > [Submit] > [Export]
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ExportVipUpgradeReviewList(ExportVipUpgradeReviewListReq req)
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
        /// VipLevelReview/UpdateVipUpgradeReview 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Vip Level Review 
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult UpdateVipUpgradeReview(UpdateVipUpgradeReviewReq req)
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
        /// VipLevelReview/GetVipDowngradeReviewList 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Vip Level Review  > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<VipDowngradeReviewDetailModel> GetVipDowngradeReviewList(VipDowngradeReviewReq req)
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
        /// VipLevelReview/ExportVipDowngradeReviewList 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Vip Level Review  > [Submit] > [Export]
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ExportVipDowngradeReviewList(ExportVipDowngradeReviewListReq req)
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
        /// VipLevelReview/UpdateVipDowngradeReview 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Vip Level Review 
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult UpdateVipDowngradeReview(UpdateVipUpgradeReviewReq req)
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