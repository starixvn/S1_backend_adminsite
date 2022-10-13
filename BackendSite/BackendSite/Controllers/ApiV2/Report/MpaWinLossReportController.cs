using BackendSite.Service.BLL;
using BackendSite.Service.Model;
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

        public async Task<ApiResult<Dictionary<string, MpaWinLossByDateResponse>>> GetWinLossByDate(MpaWinLossByDataRequest request)
        {
            var result = await _MaMpaWinLossBll.GetMapWinLossByDateResult(request);
            var apiResult = new ApiResult<Dictionary<string, MpaWinLossByDateResponse>>
            {
                Data = result
            };

            return apiResult;
        }




        //[HttpPost]
        //[TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 114, 1 })]
        //public ApiResult<WorldCupPredictionSettingResponse> GetWorldCupPredictionSetting()
        //{
        //    var apiResult = new ApiResult<WorldCupPredictionSettingResponse>();
        //    try
        //    {
        //        var responseData = _worldCupPredictionBll.GetCupPredictionSettingResponse("en"); //後台不用多語系，統一用英文，有需要可以更改
        //        apiResult.Data = responseData;
        //        apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
        //    }
        //    catch (Exception ex)
        //    {
        //        apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
        //        apiResult.Message = ex.Message;
        //    }

        //    return apiResult;
        //}

        //[HttpPost]
        //[TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 115, 1 })]
        //public ApiListResult<GetWorldCupMemberReportResponse> GetMemberReport(GetWorldCupMemberReportRequest request)
        //{
        //    var apiResult = new ApiListResult<GetWorldCupMemberReportResponse>();
        //    try
        //    {
        //        apiResult.Data = _worldCupPredictionBll.GetMemberReport(request,
        //            out var totalPages,
        //            out var rowCount); ;
        //        apiResult.TotalPages = totalPages;
        //        apiResult.PageNumber = request.PageNumber;
        //        apiResult.RowCount = rowCount;
        //        apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
        //    }
        //    catch (Exception ex)
        //    {
        //        apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
        //        apiResult.Message = ex.Message;
        //    }
        //    return apiResult;
        //}

    }
}
