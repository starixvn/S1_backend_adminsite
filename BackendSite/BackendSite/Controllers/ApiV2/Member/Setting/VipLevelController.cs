using System.Text.RegularExpressions;

using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using OfficeOpenXml;

namespace BackendSite.Controllers.ApiV2.Member.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class VipLevelController : ControllerBase
    {
        #region Fields

        private readonly AuthorizeBLL _authorizeBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly MessageBLL _messageBll;
        private readonly IVipLevelBLL _vipLevelBll;

        public VipLevelController(AuthorizeBLL authorizeBll, CustInfoBLL custInfoBll, MessageBLL messageBll, IVipLevelBLL vipLevelBll)
        {
            _authorizeBll = authorizeBll;
            _custInfoBll = custInfoBll;
            _messageBll = messageBll;
            _vipLevelBll = vipLevelBll;
        }

        #endregion

        #region Constructor


        #endregion

        #region API Model

        public class ChangeVipLevelBatchRequest
        {
            public int SiteId { get; set; }
            public IFormFile ExcelUpload { get; set; }
        }

        public class GetUpgradeSettingRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
        }

        public class SetUpgradeSettingRequest
        {
            public int Level { get; set; }
            public decimal UpDeposit { get; set; }
            public decimal UpBonus { get; set; }
            public decimal DownDeposit { get; set; }
            public decimal DownTurnover { get; set; }
        }
        #endregion

        /// <summary>
        /// VipLevel/GetSetting 取得VIP等級設定資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > VIP Level > Get Vip Level Setting
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<VipLevelSetting>>> GetSetting(GetVipLevelSettingRequest request)
        {
            var apiListResult = new ApiResult<IEnumerable<VipLevelSetting>>
            {
                Data = await _vipLevelBll.GetVipLevelSettingAsync(request.SiteId),
                ErrorCode = 0
            };
            return apiListResult;
        }

        /// <summary>
        /// VipLevel/ChangeVipLevelBatch 批次更新會員VIP等級
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Content-Type：Form-Data
        /// 
        /// Func Path：CSD > Change Vip Level Batch > [Submit]
        /// </remarks>
        /// <param name="siteId"></param>
        /// <param name="excelUpload"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 51, 3 })]
        [HttpPost]
        public async Task<List<ApiResult>> ChangeVipLevelBatchAsync([FromForm] ChangeVipLevelBatchRequest request)
        {
            var apiResult = new List<ApiResult>();
            var authData = _authorizeBll.GetAuthData();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            try
            {
                if (request.ExcelUpload.ContentType.Equals("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", StringComparison.OrdinalIgnoreCase)
                    || request.ExcelUpload.ContentType.Equals("application/vnd.ms-excel", StringComparison.OrdinalIgnoreCase)
                    || request.ExcelUpload.ContentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
                {
                    var excelPkg = new ExcelPackage(request.ExcelUpload.OpenReadStream());
                    var sheet = excelPkg.Workbook.Worksheets.First();//取得Sheet1
                    var isLastRow = false;
                    var rowId = 2;   // 因為有標題列，所以從第2列開始讀起

                    do  // 讀取資料，直到讀到空白列為止
                    {
                        var custUsername = string.IsNullOrEmpty(sheet.Cells[rowId, 1].Text) ? null : sheet.Cells[rowId, 1].Text;

                        var sabaId = sheet.Cells[rowId, 2].Text.Trim();
                        var _sabaId = "";
                        if (sabaId.IndexOf("_") > -1)
                        {
                            _sabaId = sabaId.Substring(sabaId.LastIndexOf("_") + 1, sabaId.Length - sabaId.LastIndexOf("_") - 1);
                        }
                        else if (sabaId.IndexOf("x") > -1)
                        {
                            _sabaId = sabaId.Substring(sabaId.LastIndexOf("x") + 1, sabaId.Length - sabaId.LastIndexOf("x") - 1);
                        }

                        var m = Regex.Match(_sabaId, @"\d+");
                        var _custId = int.TryParse(m.Value, out var custId) ? custId : (int?)null;
                        int.TryParse(sheet.Cells[rowId, 3].Text.Trim(), out var vipLevel);
                        var memo = sheet.Cells[rowId, 4].Text.Trim();
                        var isPayoutReward = string.Equals(sheet.Cells[rowId, 5].Text.Trim(), "V", StringComparison.OrdinalIgnoreCase);
                        var isPayoutRewardPoint = string.Equals(sheet.Cells[rowId, 6].Text.Trim(), "V", StringComparison.OrdinalIgnoreCase);
                        
                        if (string.IsNullOrEmpty(custUsername) && _custId == null)
                        {
                            isLastRow = true;
                        }
                        else
                        {
                            var custInfoDetail = _custInfoBll.GetCustInfoDetails(_custId, custUsername, request.SiteId);

                            if (custInfoDetail == null)
                            {
                                apiResult.Add(new ApiResult() { ErrorCode = 1002, Message = $"Not fund. Username: {custUsername}. SabaId: {sabaId}." });
                            }
                            else if (custInfoDetail.Status == (int)CustomerStatusEnum.Running)
                            {
                                try
                                {
                                    if (_messageBll.ChangeVipLevelBatch(custInfoDetail.SiteId, custInfoDetail.CurrencyId, custInfoDetail.CustId, custInfoDetail.UserName, vipLevel, memo, isPayoutReward, isPayoutRewardPoint, authData.UserName) > 0)
                                    {
                                        apiResult.Add(new ApiResult() { ErrorCode = (int)ErrorCodeEnum.Success, Message = $"Success. Username: {custUsername}. SabaId: {sabaId}. " });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    apiResult.Add(new ApiResult() { ErrorCode = (int)ErrorCodeEnum.SystemException, Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. {ex}" });
                                }
                            }
                            else if (custInfoDetail.Status == (int)CustomerStatusEnum.Suspend)
                            {
                                apiResult.Add(new ApiResult() { ErrorCode = 1002, Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. Suspend Status." });
                            }
                            else if (custInfoDetail.Status == (int)CustomerStatusEnum.Lock)
                            {
                                apiResult.Add(new ApiResult() { ErrorCode = 1002, Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. Lock Status." });
                            }
                            else
                            {
                                apiResult.Add(new ApiResult() { ErrorCode = 1002, Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. Closed Status." });
                            }
                            rowId += 1;
                        }
                    } while (!isLastRow);
                    return apiResult;
                }
                else
                {
                    apiResult.Add(new ApiResult() { ErrorCode = (int)ErrorCodeEnum.Fail, Message = "File Type Error" });
                    return apiResult;
                }
            }
            catch (Exception ex)
            {
                apiResult.Add(new ApiResult() { ErrorCode = (int)ErrorCodeEnum.SystemException, Message = ex.ToString() });
                return apiResult;
            }
        }

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 146, 1 })]
        [HttpPost]
        public async Task<ApiResult<IEnumerable<VipUpgradeSetting>>> GetUpgradeSetting(GetUpgradeSettingRequest request)
        {
            var apiListResult = new ApiResult<IEnumerable<VipUpgradeSetting>>
            {
                Data = await _vipLevelBll.GetUpgradeSetting(request.SiteId, request.CurrencyId),
                ErrorCode = 0
            };
            return apiListResult;
        }

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 146, 3 })]
        [HttpPost]
        public async Task<ApiResult> SetUpgradeSetting(ApiRequestWithLog<VipUpgradeSetting> req)
        {
            var result = await _vipLevelBll.SetUpgradeSetting(
                new ParamsWithLog<VipUpgradeSetting>(req, User.Identity.Name));
            return new ApiResult
            {
                ErrorCode = result == 1 ? (int)ErrorCodeEnum.Success : result,
            };
        }

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 146, 1 })]
        [HttpPost]
        public async Task<ApiResult<IEnumerable<int>>> GetVipUpgradeSupportCurrency(GetUpgradeSettingRequest request)
        {
            var apiListResult = new ApiResult<IEnumerable<int>>
            {
                Data = await _vipLevelBll.GetVipUpgradeSupportCurrency(request.SiteId),
                ErrorCode = 0
            };
            return apiListResult;
        }

        public async Task<ApiResult<IEnumerable<VipLevelName>>> GetVipLevelName(GetUpgradeSettingRequest request)
        {
            var apiListResult = new ApiResult<IEnumerable<VipLevelName>>
            {
                Data = await _vipLevelBll.GetVipLevelName(request.SiteId),
                ErrorCode = 0
            };
            return apiListResult;
        }
    }
}