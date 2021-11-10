using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Attributes;
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
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class RegistrationManagementController : ControllerBase
    {
        #region Fields

        private readonly CustInfoBLL _custInfoBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructor

        public RegistrationManagementController(IServiceProvider provider)
        {
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region Model

        public class RegLiveStatusReq : ApiRequestPageModel
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public int CurrencyId { get; set; }
            public int DepositStatus { get; set; }
            public int Status { get; set; }
            public string User { get; set; }
            public int Sort { get; set; }
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
            public string AffiliatesCode { get; set; }
            public int AgentCodeType { get; set; }
        }
        public class SetRegLiveStatusReq
        {
            public int SiteId { get; set; }
            public int CustId { get; set; }
            public int Status { get; set; }
        }
        public class AddRegLiveStatusNotesReq
        {
            public int CustId { get; set; }
            public string Note { get; set; }
        }
        public class DeleteRegLiveStatusNotesReq
        {
            public int CustId { get; set; }
            public int Id { get; set; }
        }
        public class EditRegLiveStatusNotesReq
        {
            public int CustId { get; set; }
            public int Id { get; set; }
            public string Note { get; set; }
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/RegistrationManagement" })]
        [HttpPost]
        public ApiListResult<RegLiveStatus> GetRegLiveStatusList(RegLiveStatusReq custInfoReq)
        {
            var apiResult = new ApiListResult<RegLiveStatus>();
            try
            {
                var searchResult = _custInfoBll.GetRegLiveStatusData(custInfoReq.SiteIds, custInfoReq.CurrencyId, custInfoReq.DepositStatus, custInfoReq.Status
                , custInfoReq.User, custInfoReq.PageNumber, custInfoReq.PageSize, custInfoReq.Sort, custInfoReq.StartDate, custInfoReq.EndDate, custInfoReq.AffiliatesCode, custInfoReq.AgentCodeType);
                foreach (var data in searchResult.RegLiveStatusDatas)
                {
                    data.RegTime = new DateTimeOffset(data.RegTime.DateTime, TimeSpan.Zero);
                }
                apiResult.Data = searchResult.RegLiveStatusDatas;
                apiResult.PageNumber = searchResult.PageNumber;
                apiResult.TotalPages = searchResult.TotalPages;
                apiResult.RowCount = searchResult.RowCount;
                apiResult.ErrorCode = (int)ErrorMessageCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorMessageCodeEnum.SystemException;
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/RegistrationManagement" })]
        [HttpPost]
        public ApiResult SetRegLiveStatus(SetRegLiveStatusReq para)
        {
            var apiResult = new ApiResult();
            if (_custInfoBll.SetRegLiveStatus(para.CustId, para.Status, para.SiteId, User.Identity.Name) > 0)
            {
                apiResult.ErrorCode = (int)ErrorMessageCodeEnum.Success;
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorMessageCodeEnum.Fail;
                apiResult.Message = "update fail";
            }
            return apiResult;
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/RegistrationManagement" })]
        [HttpPost]
        public ApiResult<IEnumerable<RegLiveStatusNotes>> GetRegLiveStatusNotes(AddRegLiveStatusNotesReq addCustNoteReq)
        {
            var result = _custInfoBll.GetRegLiveStatusNotes(addCustNoteReq.CustId).ToList();
            foreach (var data in result)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, TimeSpan.Zero);
            }

            var apiResult = new ApiResult<IEnumerable<RegLiveStatusNotes>>
            {
                Data = result,
                ErrorCode = (int)ErrorMessageCodeEnum.Success
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/RegistrationManagement", "rm_add_note" })]
        [HttpPost]
        public ApiResult AddRegLiveStatusNote(AddRegLiveStatusNotesReq addCustNoteReq)
        {
            var apiResult = new ApiResult();
            if (_custInfoBll.AddRegLiveStatusNote(addCustNoteReq.CustId, addCustNoteReq.Note, User.Identity.Name) > 0)
            {
                apiResult.ErrorCode = (int)ErrorMessageCodeEnum.Success;
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorMessageCodeEnum.Fail;
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/RegistrationManagement", "rm_delete_note" })]
        [HttpPost]
        public ApiResult DeleteRegLiveStatusNote(DeleteRegLiveStatusNotesReq deleteCustNoteReq)
        {
            var apiResult = new ApiResult();
            if (_custInfoBll.DeleteRegLiveStatusNote(deleteCustNoteReq.CustId, deleteCustNoteReq.Id) > 0)
            {
                apiResult.ErrorCode = (int)ErrorMessageCodeEnum.Success;
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorMessageCodeEnum.Fail;
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/RegistrationManagement", "rm_edit_note" })]
        [HttpPost]
        public ApiResult EditRegLiveStatusNote(EditRegLiveStatusNotesReq editRegLiveStatusNote)
        {
            var apiResult = new ApiResult();
            if (_custInfoBll.EditRegLiveStatusNote(editRegLiveStatusNote.CustId, editRegLiveStatusNote.Id, editRegLiveStatusNote.Note, User.Identity.Name) > 0)
            {
                apiResult.ErrorCode = (int)ErrorMessageCodeEnum.Success;
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorMessageCodeEnum.SystemException;
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/RegistrationManagement" })]
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