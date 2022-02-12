using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class SlotGamesManagementController : ControllerBase
    {
        #region Fields
        private readonly SlotGamesManagementBLL _slotGamesManagementBLL;
        #endregion

        #region Constructors
        public SlotGamesManagementController(IServiceProvider provider)
        {
            _slotGamesManagementBLL = provider.GetRequiredService<SlotGamesManagementBLL>(); ;
        }
        #endregion

        #region Actions
        /// <summary>
        /// SlotGamesManagement/GetSlotGameList 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS >  Slot Games Management
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/SlotGamesManagement" })]
        public ApiListResult<SlotGameData> GetSlotGameList(GetSlotGameListReq request)
        {

            var apiResult = new ApiListResult<SlotGameData>();
            try
            {
                var result = _slotGamesManagementBLL.GetSlotGameList(request.Site, request.TPid, request.Name, request.Status, request.PageNumber, request.PageSize,out int TotalPages, out int RowCount);
                apiResult.TotalPages = TotalPages;
                apiResult.RowCount = RowCount;
                apiResult.Data = result.SlotGameDataList;
                apiResult.PageNumber = request.PageNumber;
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
        /// SlotGamesManagement/SetSlotGame 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS >  Slot Games Management 
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/SlotGamesManagement" })]
        public ApiResult SetSlotGame(SetSlotGameReq request)
        {
            var apiResult = new ApiResult();
            try
            {
                _slotGamesManagementBLL.SetSlotGame(request, User.Identity.Name);
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
        /// SlotGamesManagement/GetSlotGameCategoryList 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS >  Category Management 
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/CategoryManagement" })]
        public ApiListResult<SlotGameData> GetSlotGameCategoryList(GetSlotGameCategoryListReq request)
        {
            var apiResult = new ApiListResult<SlotGameData>();
            try
            {
                var result = _slotGamesManagementBLL.GetSlotGameCategoryList(request.Site, request.TPid, request.Name, request.Category, request.PageNumber, request.PageSize, out int TotalPages, out int RowCount);
                apiResult.TotalPages = TotalPages;
                apiResult.RowCount = RowCount;
                apiResult.Data = result.SlotGameCategoryList;
                apiResult.PageNumber = request.PageNumber;
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
        /// SlotGamesManagement/SetSlotGameCategory 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS >  Category Management 
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/CategoryManagement" })]
        public ApiResult SetSlotGameCategory(SetSlotGameCategoryReq request)
        {
            var apiResult = new ApiResult();
            try
            {
                _slotGamesManagementBLL.SetSlotGameCategory(request, User.Identity.Name);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }
            return apiResult;
        }
        #endregion
    }
}
