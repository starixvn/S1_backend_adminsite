
using System.Collections.Generic;
using System.Threading.Tasks;
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
        /// HotGame/GetList 取得熱門遊戲清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CMS > HotGame > [Submit]
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<GetHotGameListResponse>>> GetList(GetHotGameListRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<GetHotGameListResponse>>
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = await _hotGame.Get(request.SiteId, request.CurrencyId)
            };
            return apiResult;
        }

        /// <summary>
        /// HotGame/GetDetail 取得指定熱門遊戲詳細資訊
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CMS > HotGame > [Submit] > Click[Edit]
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<GetHotGameDetailResponse>> GetDetail(GetHotGameDetailRequest request)
        {
            var apiResult = new ApiResult<GetHotGameDetailResponse>
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = await _hotGame.Get(request.Id)
            };
            return apiResult;
        }
        
        /// <summary>
        /// HotGame/Set 新增/更新熱門遊戲
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CMS > HotGame > [Submit] > Click[Edit] > [Submit]
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> Set(SetHotGameRequest request)
        {
            var updateOperator = User.Identity.Name;
            var result = await _hotGame.CreateOrUpdate(request, updateOperator);
            
            return new ApiResult
            {
                ErrorCode = result == 1 ? (int)ErrorCodeEnum.Success : result,
            };
        }
        
        /// <summary>
        /// HotGame/Delete 刪除熱門遊戲
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CMS > HotGame > [Submit] > Click[Delete]
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> Delete(DeleteHotGameRequest request)
        {
            var result = await _hotGame.Delete(request.Id);
            return new ApiResult
            {
                ErrorCode = result == 1 ? (int)ErrorCodeEnum.Success : result,
            };
        }
        
        /// <summary>
        /// HotGame/Sorting 排序
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CMS > HotGame > [Submit] > Click[Sorting]
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> Sorting(IEnumerable<HotGameSortingRequest> request)
        {
            var result = await _hotGame.Sorting(request);
            return new ApiResult
            {
                ErrorCode = result == 1 ? (int)ErrorCodeEnum.Success : result,
            };
        }

        /// <summary>
        /// HotGame/GetCommonTpList 取得遊戲第三方清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：GET
        /// 
        /// Func Path：CMS > HotGame > [Submit]
        /// 
        /// </remarks>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<IEnumerable<CommonTpInfo>>> GetCommonTpList()
        {
            return new ApiResult<IEnumerable<CommonTpInfo>>
            {
                Data = await _hotGame.GetCommonTpList(),
                ErrorCode = 0
            };
        }
        
        /// <summary>
        /// HotGame/GetTpGameType 取得第三方遊戲類型
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：GET
        /// 
        /// Func Path：CMS > HotGame > [Submit]
        /// 
        /// </remarks>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<IEnumerable<TpGameType>>> GetTpGameType()
        {
            return new ApiResult<IEnumerable<TpGameType>>()
            {
                ErrorCode = 0,
                Data = await _hotGame.GetTpGameType()
            };
        }
    }
}