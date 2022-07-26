using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Member.Process
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class RegistrationManagementController : ControllerBase
    {
        private readonly CustInfoBLL _custInfoBll;
        private readonly ExcelBLL _excelBll;
        private readonly LiveRegistrationBLL _registrationBll;

        public RegistrationManagementController(CustInfoBLL custInfoBll, ExcelBLL excelBll, LiveRegistrationBLL registrationBll)
        {
            _custInfoBll = custInfoBll;
            _excelBll = excelBll;
            _registrationBll = registrationBll;
        }

        #region Model

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
        public class GetRegLiveStatusNotesReq
        {
            public int CustId { get; set; }
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
        #endregion

        #region Action

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
        [HttpPost]
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
        [HttpPost]
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
        [HttpPost]
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
        /// <param name="addCustNoteReq"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 23, 1 })]
        [HttpPost]
        public ApiResult<IEnumerable<RegLiveStatusNotes>> GetRegLiveStatusNotes(GetRegLiveStatusNotesReq req)
        {
            var result = _custInfoBll.GetRegLiveStatusNotes(req.CustId).ToList();
            foreach (var data in result)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, TimeSpan.Zero);
            }

            var apiResult = new ApiResult<IEnumerable<RegLiveStatusNotes>>
            {
                Data = result,
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
        /// <param name="addCustNoteReq"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 23, 5 })]
        [HttpPost]
        public ApiResult AddRegLiveStatusNote(ApiRequestWithLog<AddRegLiveStatusNote> req)
        {
            var apiResult = new ApiResult();
            if (_custInfoBll.AddRegLiveStatusNote(new ParamsWithLog<AddRegLiveStatusNote>(req, User.Identity.Name)))
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
        /// <param name="deleteCustNoteReq"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 23, 4 })]
        [HttpPost]
        public ApiResult DeleteRegLiveStatusNote(ApiRequestWithLog<DeleteRegLiveStatusNote> req)
        {
            var apiResult = new ApiResult();
            if (_custInfoBll.DeleteRegLiveStatusNote(
                    new ParamsWithLog<DeleteRegLiveStatusNote>(req, User.Identity.Name)))
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
        /// <param name="editRegLiveStatusNote"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 23, 3 })]
        [HttpPost]
        public ApiResult EditRegLiveStatusNote(ApiRequestWithLog<EditRegLiveStatusNote> req)
        {
            var apiResult = new ApiResult();
            if (_custInfoBll.EditRegLiveStatusNote(new ParamsWithLog<EditRegLiveStatusNote>(req, User.Identity.Name)))
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
            var exportData = _custInfoBll.GetExportRegLiveStatus(JsonConvert.SerializeObject(req.SiteIds), req.CurrencyId, req.DepositStatus, req.Status, req.User, req.Sort, req.StartDate, req.EndDate, req.AffiliatesCode, req.AgentCodeType);
            var stream = _excelBll.Export(exportData);
            return _excelBll.ExcelFile(stream, "Registration");
        }

        #endregion
    }
}