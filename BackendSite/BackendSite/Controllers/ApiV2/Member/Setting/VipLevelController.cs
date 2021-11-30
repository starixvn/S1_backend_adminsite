using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;

namespace BackendSite.Controllers.ApiV2.Member.Setting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class VipLevelController : ControllerBase
    {
        #region Fields
        
        private readonly AuthorizeBLL _authorizeBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly MessageBLL _messageBll;
        
        #endregion
        
        #region Constructor
        
        public VipLevelController(IServiceProvider provider)
        {
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _messageBll = provider.GetRequiredService<MessageBLL>();
        }

        #endregion

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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/ChangeVipLevelBatch" })]
        [HttpPost]
        public async Task<List<ApiResult>> ChangeVipLevelBatch([FromForm] int siteId, 
            [FromForm] IFormFile excelUpload)
        {
            var apiResult = new List<ApiResult>();
            var authData = _authorizeBll.GetAuthData();
            try
            {
                if (excelUpload.ContentType.Equals("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", StringComparison.OrdinalIgnoreCase) || excelUpload.ContentType.Equals("application/vnd.ms-excel", StringComparison.OrdinalIgnoreCase) || excelUpload.ContentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
                {
                    var excelPkg = new ExcelPackage(excelUpload.OpenReadStream());
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
                        if (string.IsNullOrEmpty(custUsername) && _custId == null)
                        {
                            isLastRow = true;
                        }
                        else
                        {
                            var custInfoDetail = _custInfoBll.GetCustInfoDetails(_custId, custUsername, siteId);

                            if (custInfoDetail == null)
                            {
                                apiResult.Add(new ApiResult() { ErrorCode = 1002, Message = $"Not fund. Username: {custUsername}. SabaId: {sabaId}." });
                            }
                            else if (custInfoDetail.Status == (int)CustomerStatusEnum.Running)
                            {
                                try
                                {
                                    if (_messageBll.ChangeVipLevelBatch(custInfoDetail.SiteId, custInfoDetail.CustId, custInfoDetail.UserName, vipLevel, memo, authData.UserName) > 0)
                                    {
                                        apiResult.Add(new ApiResult() { ErrorCode = (int) ErrorCodeEnum.Success, Message = $"Success. Username: {custUsername}. SabaId: {sabaId}. " });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    apiResult.Add(new ApiResult() { ErrorCode = (int) ErrorCodeEnum.SystemException, Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. {ex}" });
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
                    apiResult.Add(new ApiResult() { ErrorCode = (int) ErrorCodeEnum.Fail, Message = "File Type Error" });
                    return apiResult;
                }
            }
            catch (Exception ex)
            {
                apiResult.Add(new ApiResult() { ErrorCode = (int) ErrorCodeEnum.SystemException, Message = ex.ToString() });
                return apiResult;
            }
        }

    }
}