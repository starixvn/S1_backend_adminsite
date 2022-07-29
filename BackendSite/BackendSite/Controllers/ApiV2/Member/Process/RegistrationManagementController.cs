using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Member.Process
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]")]
    [ApiController]
    public class RegistrationManagementController : ControllerBase
    {
        private readonly ExcelBLL _excelBll;
        private readonly LiveRegistrationDecorator _registrationBll;

        public RegistrationManagementController(ExcelBLL excelBll, LiveRegistrationDecorator registrationBll)
        {
            _excelBll = excelBll;
            _registrationBll = registrationBll;
        }

        public class RegLiveStatusReq : ApiRequestPageModel
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int Status { get; set; }
            public int DepositStatus { get; set; }
            public string AffiliatesCode { get; set; }
            public int AgentCodeType { get; set; }
            public string UserName { get; set; }
            public int CallOutStatus { get; set; }
            public int DormantUser { get; set; }
            public int DateType { get; set; }
            public DateTimeOffset StartDate { get; set; }
            public DateTimeOffset EndDate { get; set; }
            public int Sort { get; set; }
        }

        public class ExportRegLiveStatusListReq
        {
            public IEnumerable<int> SiteIds { get; set; }
            public int CurrencyId { get; set; }
            public int DepositStatus { get; set; }
            public int Status { get; set; }
            public string User { get; set; }
            public int Sort { get; set; }
            public DateTimeOffset StartDate { get; set; }
            public DateTimeOffset EndDate { get; set; }
            public string AffiliatesCode { get; set; }
            public int AgentCodeType { get; set; }
        }
        
        public class TeleCsdUpdateReq
        {
            public int CustId { get; set; }
            public int SiteId { get; set; }
        }

        /// <summary>
        /// RegistrationManagement/GetRegLiveStatusList 取得會員註冊資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Registration Management > [Submit](Query)
        /// </remarks>
        /// <param name="custInfoReq"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 23, 1 })]
        [HttpPost("GetRegLiveStatusList")]
        public async Task<ApiListResult<LiveRegistration>> GetRegLiveStatusList(RegLiveStatusReq custInfoReq)
        {
            var apiResult = new ApiListResult<LiveRegistration>();
            try
            {
                var searchResult = await _registrationBll.GetRegLiveStatusData(custInfoReq.SiteId,
                    custInfoReq.CurrencyId, custInfoReq.DepositStatus, custInfoReq.Status
                    , custInfoReq.UserName, custInfoReq.PageNumber, custInfoReq.PageSize, custInfoReq.Sort,
                    custInfoReq.StartDate, custInfoReq.EndDate, custInfoReq.AffiliatesCode, custInfoReq.AgentCodeType);
                
                apiResult.Data = searchResult.LiveRegistration;
                apiResult.PageNumber = searchResult.PageNumber;
                apiResult.TotalPages = searchResult.TotalPages;
                apiResult.RowCount = searchResult.RowCount;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        /// <summary>
        /// RegistrationManagement/SetRegLiveStatus 更新會員註冊資料狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Registration Management > [Submit](Query) > Edit [Status]
        /// </remarks>
        /// <param name="para"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 23, 12 })]
        [HttpPost("SetRegLiveStatus")]
        public async Task<ApiResult> SetRegLiveStatus(ApiRequestWithLog<SetRegLiveStatus> req)
        {
            try
            {
                var result = await _registrationBll.SetRegLiveStatus(new ParamsWithLog<SetRegLiveStatus>(req, User.Identity.Name));
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
        /// RegistrationManagement/CallMark 寫入客服致電紀錄
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Registration Management > Submit(Query) > [Complete once]
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 23, 12 })]
        [HttpPost("CallRecord")]
        public async Task<ApiResult> CallRecord(TeleCsdUpdateReq req)
        {
            try
            {
                if (await _registrationBll.UpdateCustomerTele(req.SiteId, req.CustId, User.Identity.Name) > 0)
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
        /// RegistrationManagement/GetRegLiveStatusNotes 取得會員註冊資料註記
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
        /// RegistrationManagement/AddRegLiveStatusNote 新增會員註冊資料註記
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Registration Management > [Submit](Query)  > Note[Edit] > [Add]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 23, 5 })]
        [HttpPost]
        [Route("Note/Add")]
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
        /// RegistrationManagement/DeleteRegLiveStatusNote 刪除會員註冊資料註記
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Registration Management > [Submit](Query) > Note[Edit] > Note List > [Delete]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 23, 4 })]
        [HttpPost]
        [Route("Note/Delete")]
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
        /// RegistrationManagement/EditRegLiveStatusNote 更新會員註冊資料註記
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Registration Management > [Submit](Query) > Note[Edit] > Note List > [Edit] > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 23, 3 })]
        [HttpPost]
        [Route("Note/Edit")]
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

        /// <summary>
        /// RegistrationManagement/ExportRegLiveStatus 匯出會員註冊資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Registration Management > [Submit](Query) > [Export Excel]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 23, 2 })]
        [HttpPost]
        public IActionResult ExportRegLiveStatusList(ExportRegLiveStatusListReq req)
        {
            var exportData = _registrationBll.GetExportRegLiveStatus(JsonConvert.SerializeObject(req.SiteIds), req.CurrencyId, req.DepositStatus, req.Status, req.User, req.Sort, req.StartDate, req.EndDate, req.AffiliatesCode, req.AgentCodeType);
            var stream = _excelBll.Export(exportData);
            return _excelBll.ExcelFile(stream, "Registration");
        }
    }
}