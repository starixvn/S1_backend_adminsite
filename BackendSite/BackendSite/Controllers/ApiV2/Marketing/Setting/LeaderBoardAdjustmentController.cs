using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Report;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using OfficeOpenXml;

using ExcelData = BackendSite.Service.Model.Report.ExcelData;

namespace BackendSite.Controllers.ApiV2.Marketing.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class LeaderBoardAdjustmentController : ControllerBase
    {
        #region Fields

        private readonly ExcelBLL _excelBll;
        private readonly LeaderBoardBLL _leaderBoardBll;
        private readonly AuthorizeBLL _authorizeBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly ILogger<LeaderBoardAdjustmentController> _logger;

        #endregion

        #region Constructor

        public LeaderBoardAdjustmentController(IServiceProvider provider)
        {
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _leaderBoardBll = provider.GetRequiredService<LeaderBoardBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _logger = provider.GetRequiredService<ILogger<LeaderBoardAdjustmentController>>();
        }

        #endregion

        #region API

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 144, 1 })]
        public ApiListResult<LeaderboardExtraPointList> GetExtraPointList(LeaderboardExtraPointReq request)
        {
            var apiResult = new ApiListResult<LeaderboardExtraPointList>();

            var data = _leaderBoardBll.GetLeaderboardExtraPointList(
                request.SiteId,
                request.CategoryIds,
                request.CurrencyIds,
                request.StartDate,
                request.EndDate,
                request.PageNumber,
                request.PageSize,
                out var totalPages, out var rowCount);

            apiResult.Data = data;
            apiResult.TotalPages = totalPages;
            apiResult.PageNumber = request.PageNumber;
            apiResult.RowCount = rowCount;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 144, 2 })]
        public IActionResult GetExtraPointListExport(LeaderboardExtraPointReq request)
        {
            var data = _leaderBoardBll.GetLeaderboardExtraPointListExport(
                request.SiteId,
                request.CategoryIds,
                request.CurrencyIds,
                request.StartDate,
                request.EndDate);

            var stream = _excelBll.ExportV2(data);
            return _excelBll.ExcelFile(stream, "LeaderboardExtraPoint");
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 144, 5 })]
        public ApiResult SetExtraPoint(SetLeaderboardExtraPointReq request)
        {
            var apiResult = new ApiResult();

            var result = _leaderBoardBll.SetLeaderboardExtraPoint(
                request.ActiveDate,
                request.SiteId,
                request.CategoryId,
                request.UserName,
                request.ExtraPoint,
                _authorizeBll.GetAuthData().UserName);

            switch (result)
            {
                case 0:
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                    apiResult.Message = "";
                    break;
                case -2:
                    throw new AdminException(ErrorCodeEnum.CustNotFound);
                case -3:
                    apiResult.ErrorCode = result;
                    apiResult.Message = "ActiveDate only can be selected for today or after today";

                    break;
                default:
                    apiResult.Message = "Unknown Error";
                    throw new AdminException(ErrorCodeEnum.SystemException);
            }

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 144, 5 })]
        public List<ApiResult> SetBatchExtraPoint([FromForm] SetBatchLeaderboardExtraPointReq request)
        {
            //var apiResult = new ApiResult();
            var apiResult = new List<ApiResult>();
            var excelList = new List<ExcelData>();
            var result = false;

            if (request.excelUpload != null && _excelBll.IsValidFile(request.excelUpload.ContentType))
            {
                _logger.LogInformation($@"SetBatchExtraPoint-Read Excel And Send Start At {DateTime.UtcNow}.");

                var excelPkg = new ExcelPackage(request.excelUpload.OpenReadStream());
                var sheet = excelPkg.Workbook.Worksheets.First(); //取得Sheet1
                var isLastRow = false;
                var rowId = 2; // 因為有標題列，所以從第2列開始讀起

                do // 讀取資料，直到讀到空白列為止
                {
                    var activeDate = sheet.Cells[rowId, 1].Text.Trim();
                    var categoryId = sheet.Cells[rowId, 2].Text.Trim();
                    var username = sheet.Cells[rowId, 3].Text.Trim();
                    var extraPoint = sheet.Cells[rowId, 4].Text.Trim();

                    if (string.IsNullOrEmpty(username))
                    {
                        isLastRow = true;
                    }
                    else
                    {
                        if (Convert.ToDateTime(activeDate) < DateTime.UtcNow.AddHours(8).Date)
                        {
                            apiResult.Add(new ApiResult { ErrorCode = (int)ErrorCodeEnum.Fail, Message = $"Username: {username} , ActiveDate only can be selected for today or after today " });
                        }
                        else if (_custInfoBll.GetCustInfoByCust(request.SiteId, null, username).Result == null)
                        {
                            apiResult.Add(new ApiResult { ErrorCode = (int)ErrorCodeEnum.Fail, Message = $"Username: {username} , doesn't exist " });
                        }
                        else
                        {
                            excelList.Add(new ExcelData
                            {
                                ActiveDate = Convert.ToDateTime(activeDate),
                                CategoryId = int.Parse(categoryId),
                                UserName = username,
                                ExtraPoint = decimal.Parse(extraPoint)
                            });
                        }
                        rowId += 1;
                    }
                } while (!isLastRow);


                if (excelList.Any())
                {
                    var insertList = excelList.Select(x =>
                        new SetLeaderboardExtraPointModel
                        {
                            _SiteId = request.SiteId,
                            _ActiveDate = x.ActiveDate,
                            _CategoryId = x.CategoryId,
                            _UserName = x.UserName,
                            _ExtraPoint = x.ExtraPoint,
                            _LastUpdateUser = "Admin" //_authorizeBll.GetAuthData().UserName
                        }).ToList();
                    result = _leaderBoardBll.SetLeaderboardExtraPoint(insertList);
                }

                if (result)
                {
                    apiResult.Add(new ApiResult { ErrorCode = (int)ErrorCodeEnum.Success, Message = $"Set Batch ExtraPoint Success" });
                }

                _logger.LogInformation($@"SetBatchExtraPoint-Read Excel And Send End At {DateTime.UtcNow}.");
            }
            else
            {
                apiResult.Add(new ApiResult { ErrorCode = (int)ErrorCodeEnum.Fail, Message = "Wrong Excel File" });
            }

            return apiResult;
        }

        #endregion
    }
}


