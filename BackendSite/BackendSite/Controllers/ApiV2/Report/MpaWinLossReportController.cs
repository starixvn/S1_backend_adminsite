using BackendSite.Service.BLL.Mpa;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Report
{
    //[Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class MpaWinLossReportController : ControllerBase
    {

        public readonly MpaWinLossBLL _MaMpaWinLossBll;

        public MpaWinLossReportController(MpaWinLossBLL MaMpaWinLossBll)
        {
            this._MaMpaWinLossBll = MaMpaWinLossBll;
        }

        [HttpPost]
        public async Task<ApiResult<Dictionary<string, MpaWinLossByDateResponse>>> GetWinLossByDate(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<Dictionary<string, MpaWinLossByDateResponse>>();
            try
            {
                var responseData = await _MaMpaWinLossBll.GetMapWinLossByDateResult(request, MpaWinLossGroupByTypeEnum.Date);
                apiResult.Data = responseData;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }  

            return apiResult;
        }

        [HttpPost]
        public async Task<ApiResult<Dictionary<string, MpaWinLossByDateResponse>>> GetWinLossByMonth(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<Dictionary<string, MpaWinLossByDateResponse>>();
            try
            {
                var responseData = await _MaMpaWinLossBll.GetMapWinLossByDateResult(request, MpaWinLossGroupByTypeEnum.Month);
                apiResult.Data = responseData;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        public async Task<ApiResult<Dictionary<string, MpaWinLossByTpGameTypeResponse>>> GetWinLossByTpGameType(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<Dictionary<string, MpaWinLossByTpGameTypeResponse>>();
            try
            {
                var responseData = await _MaMpaWinLossBll.GetMapWinLossByTpGameTypeResult(request, MpaWinLossGroupByTypeEnum.GameType);
                apiResult.Data = responseData;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        public async Task<ApiResult<Dictionary<string, MpaWinLossByTpCategoryResponse>>> GetWinLossByTpCategoryType(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<Dictionary<string, MpaWinLossByTpCategoryResponse>>();
            try
            {
                var responseData = await _MaMpaWinLossBll.GetMapWinLossByTpCategoryResult(request, MpaWinLossGroupByTypeEnum.TpCategory);
                apiResult.Data = responseData;
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
