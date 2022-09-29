using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Finance.WithdrawalSetting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class WithdrawalLimitController : ControllerBase
    {
        #region Fields

        private readonly WithdrawalBLL _withdrawalBll;
        private readonly SelLangBLL _selLangBll;

        #endregion

        #region Constructors

        public WithdrawalLimitController(IServiceProvider provider)
        {
            _withdrawalBll = provider.GetRequiredService<WithdrawalBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
        }

        #endregion

        #region API Model
        
        public class GetListRequest 
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
        }
        
        #endregion

        #region Action

        /// <summary>
        /// WithdrawalLimit/GetList 取得提款限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Limit Management > Click [SiteName]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult GetList(GetListRequest request)
        {
            var data = _withdrawalBll.GetWithdrawalLimitInfos(request.SiteId, request.CurrencyId);
            var apiResult = new ApiResult<IEnumerable<WithdrawalLimitInfoManage>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;
        }

        /// <summary>
        /// WithdrawalLimit/Create 新增提款限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Limit Management > Click [SiteName] > [Add] > [Save]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult Create(ApiRequestWithLog<SetWithdrawalLimitInfo> request)
        {
            var result = _withdrawalBll.AddWithdrawalLimit(
                new ParamsWithLog<SetWithdrawalLimitInfo>(request, User.Identity.Name));
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }

            return apiResult;
        }

        /// <summary>
        /// WithdrawalLimit/Update 更新提款限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Limit Management > Click [SiteName] > [Edit] > [Save]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult Update(ApiRequestWithLog<IEnumerable<SetWithdrawalLimitInfo>> request)
        {
            var result = _withdrawalBll.EditWithdrawalLimit(new ParamsWithLog<IEnumerable<SetWithdrawalLimitInfo>>(request, User.Identity.Name));
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }

            return apiResult;
        }

        /// <summary>
        /// WithdrawalLimit/Delete 刪除提款限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Limit Management > Click [SiteName] > [Delete]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult Delete(ApiRequestWithLog<SetWithdrawalLimitInfo> request)
        {
            var result = _withdrawalBll.DeleteWithdrawalLimit(
                new ParamsWithLog<SetWithdrawalLimitInfo>(request, User.Identity.Name));
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }

            return apiResult;
        }

        #endregion
    }
}
