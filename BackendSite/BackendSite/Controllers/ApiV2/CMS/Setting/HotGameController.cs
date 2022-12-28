using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.TpMaintain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TpGameType = BackendSite.Service.Model.TpGameType;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class HotGameController : ControllerBase
    {
        private readonly HotGameBLL _hotGame;
        public HotGameController(HotGameBLL hotGame)
        {
            _hotGame = hotGame;
        }

        /// <summary>
        /// 取得熱門遊戲清單
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 95, 1 })]
        public async Task<ApiListResult<GetHotGameListResponseV2>> GetDataList(GetHotGameDataListRequestV2 request)
        {
            var apiResult = new ApiListResult<GetHotGameListResponseV2>();

            try
            {
                var result = await _hotGame.GetDataListV2(request);
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
        /// 取得指定熱門遊戲詳細資訊
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 95, 1 })]
        public async Task<ApiResult<GetHotGameDetailResponseV2>> GetDetail(GetHotGameDetailRequestV2 request)
        {
            var apiResult = new ApiResult<GetHotGameDetailResponseV2>();

            try
            {
                var data = await _hotGame.GetDetailV2(request.Id, request.SiteId);
                if (data == null)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                    apiResult.Message = "data not found";
                    return apiResult;
                }

                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.Data = data;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        /// <summary>
        /// [新增/更新] 熱門遊戲
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 95, 3 })]
        public async Task<ApiResult> Set(SetHotGameRequestV2 request)
        {
            var updateOperator = User.Identity.Name;
            var result = await _hotGame.CreateOrUpdateV2(request, updateOperator);

            return new ApiResult
            {
                ErrorCode = result == 1 ? (int)ErrorCodeEnum.Success : result,
            };
        }

        /// <summary>
        /// 刪除熱門遊戲
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 95, 3 })]
        public async Task<ApiResult> Delete(DeleteHotGameRequestV2 request)
        {
            var result = await _hotGame.DeleteV2(request);
            return new ApiResult
            {
                ErrorCode = result == 1
                    ? (int)ErrorCodeEnum.Success
                    : (int)ErrorCodeEnum.SystemException,
            };
        }

        /// <summary>
        /// 排序熱門遊戲
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 95, 3 })]
        public async Task<ApiResult> Sorting(IEnumerable<HotGameSortingRequestV2> request)
        {
            var result = await _hotGame.SortingV2(request);
            return new ApiResult
            {
                ErrorCode = result == 1 
                    ? (int)ErrorCodeEnum.Success 
                    : (int)ErrorCodeEnum.SystemException
            };
        }

        /// <summary>
        /// 取得遊戲第三方清單
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 95, 1 })]
        public async Task<ApiResult<IEnumerable<CommonTpInfo>>> GetCommonTpList()
        {
            return new ApiResult<IEnumerable<CommonTpInfo>>
            {
                Data = await _hotGame.GetCommonTpList(),
                ErrorCode = 0
            };
        }

        /// <summary>
        /// 取得第三方遊戲類型
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 95, 1 })]
        public async Task<ApiResult<IEnumerable<TpGameType>>> GetTpGameType(GetTpGameTypeRequest request)
        {
            return new ApiResult<IEnumerable<TpGameType>>()
            {
                ErrorCode = 0,
                Data = await _hotGame.GetTpGameType(request.TpId)
            };
        }

        /// <summary>
        /// HotGame Tag 分類
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 95, 1 })]
        public ApiResult<IEnumerable<HotGameTopCategoryDto>> GetHotGameTopCategory()
        {
            var hotGameTopCategory = new List<HotGameTopCategory>()
            {
                HotGameTopCategory.HotGame,
                HotGameTopCategory.NewGame,
                HotGameTopCategory.Progressive,
                HotGameTopCategory.Featured,
                HotGameTopCategory.TopCasino,
            };

            return new ApiResult<IEnumerable<HotGameTopCategoryDto>>()
            {
                ErrorCode = 0,
                Data = hotGameTopCategory.Select(thisEnum => new HotGameTopCategoryDto
                {
                    HotGameTopCategoryId = (int)thisEnum,
                    HotGameTopCategoryName = thisEnum.ToString()
                })
            };
        }
    }
}
