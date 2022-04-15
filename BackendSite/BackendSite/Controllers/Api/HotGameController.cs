using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
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
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<GetHotGameDetailResponse>> GetDetail(int id)
        {
            var apiResult = new ApiResult<GetHotGameDetailResponse>
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = await _hotGame.Get(id)
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
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> Delete([FromForm]int id)
        {
            var result = await _hotGame.Delete(id);
            return new ApiResult
            {
                ErrorCode = result == 1 ? (int)ErrorCodeEnum.Success : result,
            };
        }

        /// <summary>
        /// HotGame/GetTpGameType 取得遊戲類型
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