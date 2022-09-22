using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Member.Process
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]")]
    [ApiController]
    public class RegistrationManagementController : ControllerBase
    {
        private readonly LiveReigstrationExcelBLL _excelBll;
        private readonly LiveRegistrationDecorator _registrationBll;

        public RegistrationManagementController(LiveReigstrationExcelBLL excelBll, LiveRegistrationDecorator registrationBll)
        {
            _excelBll = excelBll;
            _registrationBll = registrationBll;
        }
        /// <summary>
        /// RegistrationManagement/GetRegLiveStatusList 取得註冊會員資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Registration Management > [Submit](Query)
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 23, 1 })]
        [HttpPost("GetList")]
        public async Task<ApiListResult<LiveRegistration>> GetList(GetRegLiveStatusRequest request)
        {
            var searchResult = await _registrationBll.GetRegLiveStatusData(request);
            
            var apiResult = new ApiListResult<LiveRegistration>
            {
                Data = searchResult.LiveRegistration,
                PageNumber = searchResult.PageNumber,
                TotalPages = searchResult.TotalPages,
                RowCount = searchResult.RowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;
        }

        /// <summary>
        /// RegistrationManagement/ExportRegLiveStatus 匯出註冊會員資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Registration Management > [Submit](Query) > [Export Excel]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 23, 2 })]
        [HttpPost("Export")]
        public async Task<IActionResult> Export(ExportRegLiveStatusRequest request)
        {
            var exportData = await _registrationBll.GetExportRegLiveStatus(request);
            var stream = _excelBll.Export(exportData, offset: request.StartDate.Offset);
            return _excelBll.ExcelFile(stream, "Registration");
        }

        /// <summary>
        /// RegistrationManagement/SetRegLiveStatus 更新註冊會員資料狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Registration Management > [Submit](Query) > Edit [Status]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 23, 3 })]
        [HttpPost("Set")]
        public async Task<ApiResult> Set(SetRegLiveStatusRequest req)
        {
            try
            {
                var result = await _registrationBll.SetRegLiveStatus(req, User.Identity.Name);
                var apiResult = new ApiResult
                {
                    ErrorCode = result > 0 ? (int)ErrorCodeEnum.Success : (int)ErrorCodeEnum.Fail
                };
                return apiResult;
            }
            catch (Exception e)
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
            }
        }

        /// <summary>
        /// RegistrationManagement/CallingRecord 寫入客服致電紀錄
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Registration Management > Submit(Query) > [Complete once]
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 23, 3 })]
        [HttpPost("CallingRecord")]
        public async Task<ApiResult> CallingRecord(TeleCsdUpdateReq req)
        {
            try
            {
                if (await _registrationBll.AddCallingRecord(req.SiteId, req.CustId, User.Identity.Name) > 0)
                {
                    return new ApiResult
                    {
                        ErrorCode = (int)ErrorCodeEnum.Success,
                        Message = "Update Success."
                    };
                }

                throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
            }
            catch (Exception e)
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
            }
        }

        /// <summary>
        /// RegistrationManagement/GetRegLiveStatusNotes 取得會員註記
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Registration Management > [Submit](Query) > Note[Edit]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 23, 1 })]
        [HttpPost("Note/Get")]
        public async Task<ApiResult<IEnumerable<CustRegLiveStatusNotes>>> GetNote(GetRegLiveStatusNoteRequest req)
        {
            var notes = await _registrationBll.GetRegLiveStatusNotes(req.CustId);
            var apiResult = new ApiResult<IEnumerable<CustRegLiveStatusNotes>>
            {
                Data = notes,
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;
        }

        /// <summary>
        /// RegistrationManagement/AddRegLiveStatusNote 新增會員註記
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Registration Management > [Submit](Query)  > Note[Edit] > [Add]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 23, 3 })]
        [HttpPost("Note/Add")]
        public async Task<ApiResult> AddNote(AddRegLiveStatusNoteRequest req)
        {
            var result = await _registrationBll.AddRegLiveStatusNote(req.CustId, req.Note, User.Identity.Name);
            var apiResult = new ApiResult();
            if (result > 0)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                apiResult.Message = "Create fail.";
            }
            return apiResult;
        }

        /// <summary>
        /// RegistrationManagement/DeleteRegLiveStatusNote 刪除會員註記
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Registration Management > [Submit](Query) > Note[Edit] > Note List > [Delete]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 23, 3 })]
        [HttpPost("Note/Delete")]
        public async Task<ApiResult> DeleteNote(DeleteRegLiveStatusNoteRequest req)
        {
            var result = await _registrationBll.DeleteRegLiveStatusNote(req.CustId, req.Id, User.Identity.Name);
            var apiResult = new ApiResult();
            if (result > 0)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                apiResult.Message = "Delete fail.";
            }
            return apiResult;
        }

        /// <summary>
        /// RegistrationManagement/EditRegLiveStatusNote 更新會員註記
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Registration Management > [Submit](Query) > Note[Edit] > Note List > [Edit] > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 23, 3 })]
        [HttpPost("Note/Edit")]
        public async Task<ApiResult> EditNote(EditRegLiveStatusNoteRequest req)
        {
            var result = await _registrationBll.EditRegLiveStatusNote(req.CustId, req.Id, req.Note, User.Identity.Name);
            var apiResult = new ApiResult();
            if (result > 0)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = "Update fail.";
            }
            return apiResult;
        }
    }
}
