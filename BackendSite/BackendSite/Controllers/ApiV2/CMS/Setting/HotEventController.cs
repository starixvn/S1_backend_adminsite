using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class HotEventController : ControllerBase
    {
        private readonly HotEventBLL _hotEventBll;

        public HotEventController(HotEventBLL hotEventBll)
        {
            _hotEventBll = hotEventBll;
        }

        /// <summary>
        /// 取得聯賽標題下拉選單資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 56, 1 })]
        public async Task<ApiResult<IEnumerable<SportLeagueTitleOptionResponse>>> GetSportLeagueTitle()
        {
            var apiResult = new ApiResult<IEnumerable<SportLeagueTitleOptionResponse>>();
            try
            {
                apiResult.Data = await _hotEventBll.GetSportLeagueTitleAsync();
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
        /// 取得聯賽下拉選單資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 56, 1 })]
        public async Task<ApiResult<IEnumerable<SportLeaguesOptionResponse>>> GetSportLeagues()
        {
            var apiResult = new ApiResult<IEnumerable<SportLeaguesOptionResponse>>();
            try
            {
                apiResult.Data = await _hotEventBll.GetSportLeaguesAsync();
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
        /// 取得聯隊伍拉選單資料
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 56, 1 })]
        public async Task<ApiResult<IEnumerable<SportTeamOptionResponse>>> GetSportTeams(SportTeamsRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<SportTeamOptionResponse>>();
            try
            {
                apiResult.Data = await _hotEventBll.GetSportTeamOptionAsync(request);
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
        /// 取得 HotEvent 清單
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 56, 1 })]
        public async Task<ApiListResult<HotEventDataListResponse>> GetDataList(HotEventDataListRequest request)
        {
            var apiResult = new ApiListResult<HotEventDataListResponse>();
            try
            {
                var result = await _hotEventBll.GetHotEventDataListAsync(request);
                apiResult.Data = result.Data;
                apiResult.PageNumber = request.PageNumber;
                apiResult.RowCount = result.RowCount;
                apiResult.TotalPages = result.TotalPages;
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
        /// 取得單筆 HotEvent 明細
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 56, 1 })]
        public async Task<ApiResult<HotEventDetailResponse>> GetDetail(HotEventDetailRequest request)
        {
            var apiResult = new ApiResult<HotEventDetailResponse>();
            try
            {
                apiResult.Data = await _hotEventBll.GetHotEventDetail(request);
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
        /// 新增 HotEvent
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 56, 3 })]
        public async Task<ApiResult> CreateData(CreateHotEventRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                await _hotEventBll.CreateHotEventAsync(request, User.Identity.Name);
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
        /// 編輯 HotEvent
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 56, 3 })]
        public async Task<ApiResult> SetData(SetHotEventDataRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                await _hotEventBll.SetHotEventData(request, User.Identity.Name);
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
        /// 修改 HotEvent 狀態
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 56, 3 })]
        [HttpPost]
        public async Task<ApiResult> SetStatus(SetHotEventStatusRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                await _hotEventBll.SetHotEventDataAsync(request, User.Identity.Name);
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
        /// 刪除 HotEvent
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 56, 4 })]
        public async Task<ApiResult> DeleteData(DeleteHotEventRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                await _hotEventBll.DeleteHotEventAsync(request);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
    }
}


