using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SlotGamesManagementController : ControllerBase
    {
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly SlotGamesManagementBLL slotGamesManagementBLL;
        public SlotGamesManagementController(ServerInfoBLL serverInfoBLL, SlotGamesManagementBLL slotGamesManagementBLL)
        {
            this.serverInfoBLL = serverInfoBLL;
            this.slotGamesManagementBLL = slotGamesManagementBLL;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/SlotGamesManagement" })]
        public ApiListResult<SlotGameData> GetSlotGameList(GetSlotGameListReq req)
        {

            var apiResult = new ApiListResult<SlotGameData>();
            try
            {
                var result = slotGamesManagementBLL.GetSlotGameList(req.Site, req.TPid, req.Name, req.Status, req.PageNumber, req.PageSize, out int TotalPages, out int RowCount);
                apiResult.TotalPages = TotalPages;
                apiResult.RowCount = RowCount;
                apiResult.Data = result.SlotGameDataList;
                apiResult.PageNumber = req.PageNumber;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/SlotGamesManagement" })]
        public ApiResult SetSlotGame(SetSlotGameReq req)
        {
            var apiResult = new ApiResult();
            try
            {
                slotGamesManagementBLL.SetSlotGame(req, User.Identity.Name);
                apiResult.ErrorCode = 0;
            }
            catch(Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }
            return apiResult;
        }
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/CategoryManagement" })]
        public ApiListResult<SlotGameData> GetSlotGameCategoryList(GetSlotGameCategoryListReq req)
        {
            var apiResult = new ApiListResult<SlotGameData>();
            try
            {
                var result = slotGamesManagementBLL.GetSlotGameCategoryList(req.Site, req.TPid, req.Name, req.Category, req.PageNumber, req.PageSize, out int TotalPages, out int RowCount);
                apiResult.TotalPages = TotalPages;
                apiResult.RowCount = RowCount;
                apiResult.Data = result.SlotGameCategoryList;
                apiResult.PageNumber = req.PageNumber;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }
            return apiResult;
        }
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/CategoryManagement" })]
        public ApiResult SetSlotGameCategory(SetSlotGameCategoryReq req)
        {
            var apiResult = new ApiResult();
            try
            {
                slotGamesManagementBLL.SetSlotGameCategory(req, User.Identity.Name);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }
            return apiResult;
        }
    }
}
