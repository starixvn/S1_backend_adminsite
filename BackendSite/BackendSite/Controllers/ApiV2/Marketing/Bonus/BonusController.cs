using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MySql.Data.MySqlClient;

using OfficeOpenXml;

namespace BackendSite.Controllers.ApiV2.Marketing.Bonus
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BonusController : ControllerBase
    {
        #region Fields

        private readonly BonusManageBLL _bonusManageBll;
        private readonly AuthorizeBLL _authorizeBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly Common _common;
        private readonly ExcelBLL _excelBll;
        private readonly ILogger<BonusController> _logger;
        private readonly NewAuthorizeBLL _newAuthorizeBll;

        #endregion

        #region Constructors

        public BonusController(IServiceProvider provider)
        {
            _bonusManageBll = provider.GetRequiredService<BonusManageBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _common = provider.GetRequiredService<Common>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _logger = provider.GetRequiredService<ILogger<BonusController>>();
            _newAuthorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
        }

        #endregion

        #region API Model

        public class GetListRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public IEnumerable<int> PromotionTypes { get; set; }

            /// <summary>
            /// Bonus狀態：
            /// 0=All
            /// 1=Active
            /// 2=Inactive
            /// </summary>
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
            /// <summary>
            /// Bonus支援裝置：
            /// 0=Select All
            /// 1=Web and App
            /// 2=Web
            /// 3=App
            /// </summary>
            public int Device { get; set; }
            public string BonusCode { get; set; }
            public string ScratchCardCode { get; set; }
            public bool IsSpecial { get; set; }
        }
        public class GetByBonusCodeRequest
        {
            public int SiteId { get; set; }
            public string BonusCode { get; set; }
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
        public class SendBonusUsingListRequest
        {
            public int SiteId { get; set; }
            public string BonusCode { get; set; }
            public IFormFile excelUpload { get; set; }
        }
        public class GetBonusUsingListRequest
        {
            public int SiteId { get; set; }
            public string BonusCode { get; set; }
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 45, 1 })]
        public ApiListResult<PromotionInfo> GetList(GetListRequest request)
        {
            var apiResult = new ApiListResult<PromotionInfo>();

            try
            {
                var siteId = _newAuthorizeBll.CheckBrand();
                apiResult.Data = _bonusManageBll.GetPromotionList(new List<int> { siteId },
                    request.CurrencyIds,
                    request.PromotionTypes,
                    request.Status,
                    request.StartDate.ToUnixTimeSeconds(),
                    request.EndDate.ToUnixTimeSeconds(),
                    request.Device,
                    request.BonusCode,
                    request.ScratchCardCode,
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 45, 1 })]
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
        /// BonusManagement/UpdateStatus 更新促銷獎勵狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Bonus Management > [Search] > [Update Status] 
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 45, 3 })]
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 45, 3 })]
        public ApiResult Set(SetRequest request)
        {
            var apiResult = new ApiResult();

            try
            {
                request.PromotionInfo.SpecialCode = request.PromotionInfo.IsSpecial ? request.PromotionInfo.SpecialCode : null;
                if (request.IsAdd && _bonusManageBll.CheckPromotionExists(request.PromotionInfo.BonusCode,
                        request.PromotionInfo.SpecialCode))
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.DuplicateData;
                    apiResult.Message = "Duplicate Bonus Code or Scratch Code.";
                    return apiResult;
                }

                switch (request.PromotionInfo.PromotionType)
                {
                    case   (int)PromotionTypeEnum.FreeBet 
                        or (int)PromotionTypeEnum.Cashback
                        or (int)PromotionTypeEnum.RetentionFreeBet:
                        
                        if (request.PromotionInfo.Specific.Any(specific =>
                                    specific.Status == (int)PromotionSpecificStatusEnum.Active &&
                                    CheckRollover(specific.RollOver, specific.WinOver, apiResult)))
                            {
                                return apiResult;
                            }

                        break;
                    case   (int)PromotionTypeEnum.FstDeposit
                        or (int)PromotionTypeEnum.ReDeposit 
                        or (int)PromotionTypeEnum.MissionBonus:
                        
                        if (request.PromotionInfo.Specific.Any(specific =>
                                specific.Status == (int)PromotionSpecificStatusEnum.Active &&
                                specific.TierAmountList.Any(tierAmount =>
                                    CheckRollover(tierAmount.RollOver, tierAmount.WinOver, apiResult))))
                        {
                            return apiResult;
                        }

                        if (request.PromotionInfo.Specific.Any(specific => specific.TierAmountList.Count() > 10))
                        {
                            apiResult.ErrorCode = (int)ErrorCodeEnum.UpdateFailed;
                            apiResult.Message = "Tier Amount can only be set up to 10.";
                            return apiResult;
                        }
                        break;
                }

                // 檢查CashBack的PromotionCategory類型
                if (request.PromotionInfo.PromotionType == (int)PromotionTypeEnum.Cashback
                && !Enum.IsDefined(typeof(PromotionCategoryEnum), request.PromotionInfo.CategoryId))
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.UpdateFailed;
                    apiResult.Message = $"This PromotionCategory({request.PromotionInfo.CategoryId}) not support Cashback.";
                    return apiResult;
                }

                var errorCode = _bonusManageBll.SetPromotion(request.IsAdd,
                    request.PromotionInfo,
                    request.DisplayConditionAboutVipLevel,
                    User.Identity?.Name) ? 0 : 1;

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

        private bool CheckRollover(int rollOver, int winOver, ApiResult apiResult)
        {
            switch (rollOver)
            {
                case <= 0 when winOver <= 0:
                    apiResult.ErrorCode = (int)ErrorCodeEnum.DateOutOfRange;
                    apiResult.Message = "RollOver or WinOver must have at least one greater than 0.";
                    return true;
                case > 0 when winOver > 0:
                    apiResult.ErrorCode = (int)ErrorCodeEnum.DateOutOfRange;
                    apiResult.Message = "RollOver or WinOver can only have one greater than 0.";
                    return true;
            }

            return false;
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 45, 3 })]
        public ApiResult Duplicate(DuplicateRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<PromotionInfo>>();

            try
            {
                request.NewSpecialCode = request.IsSpecial ? request.NewSpecialCode : null;
                if (_bonusManageBll.CheckPromotionExists(request.NewBonusCode, request.NewSpecialCode))
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.DuplicateData;
                    apiResult.Message = "Duplicate Bonus Code or Scratch Code.";
                    return apiResult;
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

        /// <summary>
        /// BonusManagement/SendBonusUsingList 發送Bonus允許名單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Bonus Management > [Edit] 
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 45, 3 })]
        [HttpPost]
        public async Task<ApiResult<int>> SendBonusUsingList([FromForm] SendBonusUsingListRequest req)
        {
            var authData = _authorizeBll.GetAuthData();
            var excelList = new List<BonusCustUsing>();
            var result = false;
            _logger.LogInformation($@"SendBonusUsingList-Read Excel And Send Start At {DateTime.UtcNow}.");

            if (req.excelUpload != null)
            {
                if (_excelBll.IsValidFile(req.excelUpload.ContentType))
                {
                    var excelPkg = new ExcelPackage(req.excelUpload.OpenReadStream());
                    var sheet = excelPkg.Workbook.Worksheets.First(); //取得Sheet1
                    var isLastRow = false;
                    var rowId = 2; // 因為有標題列，所以從第2列開始讀起

                    var id = _bonusManageBll.SetBonusCustUsingInfo(req.SiteId, req.BonusCode, authData.UserName);

                    do // 讀取資料，直到讀到空白列為止
                    {
                        var sabaId = sheet.Cells[rowId, 1].Text.Trim();
                        var userName = sheet.Cells[rowId, 2].Text.Trim();
                        var custId = _common.TpIdToCustId(sabaId);

                        if (custId is null)
                        {
                            var custInfo = _custInfoBll.GetCustInfoDetails(null, userName, req.SiteId);
                            custId = custInfo?.CustId;
                        }

                        if (string.IsNullOrEmpty(sabaId) && string.IsNullOrEmpty(userName))
                        {
                            isLastRow = true;
                        }
                        else
                        {
                            if (custId is not null)
                            {
                                var data = new BonusCustUsing
                                {
                                    Id = id,
                                    CustId = custId
                                };
                                excelList.Add(data);
                            }
                            rowId += 1;
                        }
                    } while (!isLastRow);

                    result = _bonusManageBll.SetBonusCustUsingDetail(excelList);

                    _logger.LogInformation($@"SendBonusUsingList-Read Excel And Send End At {DateTime.UtcNow}.");
                }
                else
                {
                    return new ApiResult<int>()
                    {
                        ErrorCode = -1,
                        Message = "File Type Error"
                    };
                }
            }

            if (result)
            {
                return new ApiResult<int>()
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Upload Finish."
                };
            }
            else
            {
                return new ApiResult<int>()
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Upload Fail."
                };
            }
        }

        /// <summary>
        /// BonusManagement/GetBonusUsingList 取得 Bonus允許名單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Bonus Management > [Edit] > [Member List]
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 45, 1 })]
        [HttpPost]
        public List<BonusCustUsingList> GetBonusUsingList(GetBonusUsingListRequest req)
        {
            var result = _bonusManageBll.GetBonusCustUsingDetail(req.SiteId, req.BonusCode).ToList();

            return result;
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 45, 1 })]
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 45, 2 })]
        public IActionResult ExportScratchCardList(ExportScratchCardListRequest request)
        {
            var exportData = _bonusManageBll.GetScratchCardListExport(request.SpecialCode, (int)request.Status);
            foreach (var data in exportData)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, TimeSpan.Zero);
            }
            var stream = _excelBll.ExportV2(exportData);

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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 45, 3 })]
        public ApiResult GenScratchCardCode(GenScratchCardCodeRequest request)
        {
            var apiResult = new ApiResult<dynamic>();

            apiResult.ErrorCode = _bonusManageBll.GenScratchCardCode(request.SpecialCode, request.Num, _newAuthorizeBll.GetAuthData().UserName);

            return apiResult;
        }

        #endregion

        #endregion
    }
}