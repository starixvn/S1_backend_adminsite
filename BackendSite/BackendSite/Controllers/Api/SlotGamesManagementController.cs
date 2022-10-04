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
        private readonly SlotGamesManagementBLL _slotGamesManagementBLL;

        public SlotGamesManagementController(SlotGamesManagementBLL slotGamesManagementBLL)
        {
            _slotGamesManagementBLL = slotGamesManagementBLL;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/SlotGamesManagement" })]
        public ApiListResult<SlotGameData> GetSlotGameList(GetSlotGameListReq request)
        {
            try
            {
                var data = _slotGamesManagementBLL.GetSlotGameList(request, out var pageInfo);
                return new ApiListResult<SlotGameData>(ErrorCodeEnum.Success, pageInfo, data);
            }
            catch (Exception ex)
            {
                return new ApiListResult<SlotGameData>((int)ErrorCodeEnum.SystemException, ex.Message);
            }
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/SlotGamesManagement" })]
        public ApiResult SetSlotGame(SetSlotGameReq req)
        {
            var apiResult = new ApiResult();
            try
            {
                var result = _slotGamesManagementBLL.SetSlotGame(req, User.Identity.Name);
                if(result > 0)
                {
                    apiResult.ErrorCode = 0;
                }
                else
                {
                    apiResult.ErrorCode = -1;
                }
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
        public ApiListResult<SlotGameData> GetSlotGameCategoryList(GetSlotGameCategoryListReq request)
        {
            try
            {
                var data = _slotGamesManagementBLL.GetSlotGameCategoryList(request, out var pageInfo);
                return new ApiListResult<SlotGameData>(ErrorCodeEnum.Success, pageInfo, data);
            }
            catch (Exception ex)
            {
                return new ApiListResult<SlotGameData>((int)ErrorCodeEnum.SystemException, ex.Message);
            }
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/CategoryManagement" })]
        public ApiResult SetSlotGameCategory(SetSlotGameCategoryReq req)
        {
            var apiResult = new ApiResult();
            try
            {
                var result = _slotGamesManagementBLL.SetSlotGameCategory(req, User.Identity.Name);
                if (result > 0)
                {
                    apiResult.ErrorCode = 0;
                }
                else
                {
                    apiResult.ErrorCode = -1;
                }
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
