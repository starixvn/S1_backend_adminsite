using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.SystemFunc
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class ReasonController : ControllerBase
    {
        #region Fields

        private readonly ReasonManagementBLL _reasonManagementBll;
        private readonly NewAuthorizeBLL _authorizeBll;

        #endregion

        #region Constructors

        public ReasonController(IServiceProvider provider)
        {
            _reasonManagementBll = provider.GetRequiredService<ReasonManagementBLL>();
            _authorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
        }

        #endregion

        /// <summary>
        /// Reason/GetList 取得原因碼清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：BankFinancial > Reason Management > [Submit]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiListResult<ReasonManagementData> GetList(GetReasonManagementDataPara request)
        {
            var searchResult = _reasonManagementBll.GetReasonManagementData(request);

            return new ApiListResult<ReasonManagementData>
            {
                Data = searchResult.ReasonManagementData,
                PageNumber = searchResult.PageNumber,
                TotalPages = searchResult.TotalPages,
                RowCount = searchResult.RowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// Reason/Set 更新原因碼
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：BankFinancial > Reason Management > [Edit] > [Submit]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult Set(SetReasonManagementPara request)
        {
            var authData = _authorizeBll.GetAuthData();
            var (success, msg) = _reasonManagementBll.SetReasonManagement(request, authData.UserId);

            return new ApiResult
            {
                ErrorCode = (int)(success ? ErrorCodeEnum.Success : ErrorCodeEnum.Fail),
                Message = msg
            };
        }

        /// <summary>
        /// Reason/Insert 新增原因碼
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：BankFinancial > Reason Management > [Create] > [Submit]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult Insert(InsertReasonManagementPara request)
        {
            var authData = _authorizeBll.GetAuthData();
            var (success, msg) = _reasonManagementBll.InsertReasonManagement(request, authData.UserId);

            return new ApiResult
            {
                ErrorCode = (int)(success ? ErrorCodeEnum.Success : ErrorCodeEnum.Fail),
                Message = msg
            };
        }
    }
}