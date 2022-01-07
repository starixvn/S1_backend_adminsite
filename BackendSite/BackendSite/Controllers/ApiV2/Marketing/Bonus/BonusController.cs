using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.Controllers.Api;
using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace BackendSite.Controllers.ApiV2.Marketing.Bonus
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/BonusManage" })]
    public class BonusController : ControllerBase
    {
        #region Fields

        private readonly BonusManageBLL _bonusManageBll;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly AuthorizeBLL _authorizeBll;
        private readonly ExcelBLL _excelBll;
        private readonly ILogger<BonusManageController> _logger;
        #endregion

        #region Constructors

        public BonusController(IServiceProvider provider)
        {
            _bonusManageBll = provider.GetRequiredService<BonusManageBLL>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _logger = provider.GetRequiredService<ILogger<BonusManageController>>();
        }

        #endregion

        #region API Model

        public class GetListRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> PromotionTypes { get; set; }
            public int Status { get; set; }
            public DateTimeOffset StartDate
            {
                get => _startDate;
                set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public DateTimeOffset EndDate
            {
                get => _endDate;
                set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public int Device { get; set; }
            public string BonusCode { get; set; }
            public bool IsSpecial { get; set; }
        }
        public class GetByBonusCodeRequest
        {
            public int SiteId { get; set; }
            public string BonusCode { get; set; }
        }
        public class GetSiteListByLoginUserReq
        {
            public int? SiteId { get; set; }
        }
        public class SetRequest
        {
            public bool IsAdd { get; set; }
            public PromotionInfoReq PromotionInfo { get; set; }
            public DisplayConditionAboutVipLevel DisplayConditionAboutVipLevel { get; set; }
        }
        public class DuplicateRequest
        {
            public int SiteId { get; set; }
            public string BonusCode { get; set; }
            public string NewBonusCode { get; set; }
            public string NewSpecialCode { get; set; }
            public bool IsSpecial { get; set; }
        }

        public class UpdatePromotionStatusReq
        {
            public int SiteId { get; set; }
            public string BonusCode { get; set; }
            /// <summary>
            /// 狀態(1=開啟,2=關閉)
            /// </summary>
            public int Status { get; set; }
        }

        #region ScratchCard

        public class GetScratchCardListRequest : ApiRequestPageModel
        {
            public string SpecialCode { get; set; }
            public ScratchCardStatusEnum Status { get; set; }
        }

        public class ExportScratchCardListRequest
        {
            public string SpecialCode { get; set; }
            public ScratchCardStatusEnum Status { get; set; }
        }

        public class GenScratchCardCodeRequest
        {
            public string SpecialCode { get; set; }
            public int Num { get; set; }
        }

        #endregion

        #endregion

        #region API

        /// <summary>
        /// BonusManagement/GetList 取得促銷獎勵清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Bonus Management > [Search] 
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiListResult<PromotionInfo> GetList(GetListRequest request)
        {
            var apiResult = new ApiListResult<PromotionInfo>();

            try
            {
                apiResult.Data = _bonusManageBll.GetPromotionList(request.SiteIds,
                    request.PromotionTypes,
                    request.Status,
                    request.StartDate.ToUnixTimeSeconds(),
                    request.EndDate.ToUnixTimeSeconds(),
                    request.Device,
                    request.BonusCode,
                    request.IsSpecial,
                    request.PageNumber,
                    request.PageSize,
                    out int totalPages,
                    out int rowCount);

                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.TotalPages = totalPages;
                apiResult.RowCount = rowCount;
                apiResult.PageNumber = request.PageNumber;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }

        /// <summary>
        /// BonusManagement/GetByBonusCode 取得指定促銷獎勵
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Bonus Management > [Search] > [edit]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiResult<PromotionInfo> GetByBonusCode(GetByBonusCodeRequest request)
        {
            var apiResult = new ApiResult<PromotionInfo>();
            try
            {
                apiResult.Data = _bonusManageBll.GetPromotion(request.BonusCode, request.SiteId);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }
            return apiResult;
        }

        /// <summary>
        /// BonusManagement/GetSiteListByLoginUser 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Bonus Management > [Search] > [edit]
        /// </remarks>
        /// <param name="req"></param>
        //[HttpPost]
        //public ApiResult<IEnumerable<dynamic>> GetSiteListByLoginUser(GetSiteListByLoginUserReq req)
        //{
        //    var apiResult = new ApiResult<IEnumerable<dynamic>>();
        //    try
        //    {
        //        apiResult.Data = _serverInfoBll.GetSiteListByLoginUser()?.Select(x =>
        //            new { x.SiteName, x.SiteId, CurrencyList = x.ICurrencyList, LangList = x.ILangList });
        //        if (req.SiteId != null && apiResult.Data != null && apiResult.Data.Any())
        //        {
        //            apiResult.Data = apiResult.Data.Where(s => s.SiteId == req.SiteId);
        //        }

        //        apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, MethodExtension.GetCurrentMethod());
        //        apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
        //        apiResult.Message = "something went wrong.";
        //        apiResult.RawMessage = ex.Message;
        //    }

        //    return apiResult;
        //}

        /// <summary>
        /// BonusManagement/UpdateStatus 更新促銷獎勵狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Bonus Management > [Search] > [Update Status] 
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult UpdateStatus(UpdatePromotionStatusReq req)
        {
            var result = _bonusManageBll.SetPromotionStatus(req.BonusCode, req.SiteId, req.Status);

            if (result != 0)
                throw new AdminException(ErrorCodeEnum.Fail);

            return new ApiResult()
            {
                ErrorCode = result
            };
        }

        /// <summary>
        /// BonusManagement/Set 更新促銷獎勵
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Bonus Management > [Create]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiResult Set(SetRequest request)
        {
            var apiResult = new ApiResult();

            try
            {
                //如果是新增時有填SpecialCode 檢查SpecialCode是否有重複
                if (request.IsAdd && request.PromotionInfo.IsSpecial)
                {
                    if (_bonusManageBll.CheckSpecialCodeDuplicate(request.PromotionInfo.SpecialCode))
                    {
                        apiResult.ErrorCode = (int)ErrorCodeEnum.DuplicateData;
                        apiResult.Message = "Duplicate Special Code.";
                        return apiResult;
                    }
                }

                var errorCode = _bonusManageBll.SetPromotion(request.IsAdd,
                    request.PromotionInfo,
                    request.DisplayConditionAboutVipLevel,
                    User.Identity.Name) ? 0 : 1;

                apiResult.ErrorCode = errorCode;
                apiResult.Message = errorCode == 1
                    ? "The bonus setting already exists which with same promotion-type & product-category."
                    : string.Empty;
            }
            catch (MySqlException sqlException)
            {
                _logger.LogError(sqlException, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = sqlException.Number;
                apiResult.Message = sqlException.Message;
                apiResult.RawMessage = sqlException.ToString();
            }

            return apiResult;
        }

        /// <summary>
        /// BonusManagement/Duplicate 複製促銷獎勵
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Bonus Management > [Edit] > [Copy] > [Save]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiResult Duplicate(DuplicateRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<PromotionInfo>>();

            try
            {
                if (request.IsSpecial)
                {
                    if (_bonusManageBll.CheckSpecialCodeDuplicate(request.NewSpecialCode))
                    {
                        apiResult.ErrorCode = (int)ErrorCodeEnum.DuplicateData;
                        apiResult.Message = "Duplicate datas about Special Code.";
                        return apiResult;
                    }
                }
                apiResult.ErrorCode = _bonusManageBll.DuplicatePromotion(
                    request.BonusCode,
                    request.SiteId,
                    request.NewBonusCode,
                    User.Identity.Name,
                    request.NewSpecialCode,
                    request.IsSpecial) ? 0 : 1;
                apiResult.Message = apiResult.ErrorCode == 1
                    ? "Duplicate fail.The bonus setting already exists which with same promotion-type & product-category."
                    : string.Empty;
            }
            catch (MySqlException sqlException)
            {
                _logger.LogError(sqlException, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = sqlException.Number;
                apiResult.Message = sqlException.Message;
                apiResult.RawMessage = sqlException.ToString();
            }

            return apiResult;
        }

        #region ScratchCard

        /// <summary>
        /// BonusManagement/GetScratchCardList 取得刮刮卡清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Bonus Management > [Search] > [edit] > [ScratchCard]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiListResult<ScratchCard> GetScratchCardList(GetScratchCardListRequest request)
        {
            var apiResult = new ApiListResult<ScratchCard>();

            var result = _bonusManageBll.GetScratchCardList(request.SpecialCode,
                (int)request.Status,
                request.PageNumber,
                request.PageSize,
                out int totalPages,
                out int rowCount);

            foreach (var data in result)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, TimeSpan.Zero);
            }

            apiResult.Data = result;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            apiResult.PageNumber = request.PageNumber;
            apiResult.TotalPages = totalPages;
            apiResult.RowCount = rowCount;

            return apiResult;
        }

        /// <summary>
        /// BonusManagement/ExportScratchCardList 匯出刮刮卡清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Bonus Management > [Search] > [ScratchCard] > [export]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public IActionResult ExportScratchCardList(ExportScratchCardListRequest request)
        {
            var exportData = _bonusManageBll.GetScratchCardListExport(request.SpecialCode, (int)request.Status);
            foreach (var data in exportData)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, TimeSpan.Zero);
            }
            var stream = _excelBll.Export(exportData);

            return _excelBll.ExcelFile(stream, "ScratchCardList");
        }

        /// <summary>
        /// BonusManagement/GenScratchCardCode 產生刮刮卡號碼
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Bonus Management > [ScratchCard] > [Generar ScratchCard] > [Generar]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiResult GenScratchCardCode(GenScratchCardCodeRequest request)
        {
            var apiResult = new ApiResult<dynamic>();

            apiResult.ErrorCode = _bonusManageBll.GenScratchCardCode(request.SpecialCode, request.Num, _authorizeBll.GetAuthData().UserName);

            return apiResult;
        }

        #endregion

        #endregion
    }
}