using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Finance.AdjustmentSetting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class AdjustmentLimitController : ControllerBase
    {

        #region Fields

        private readonly PaymentBLL _paymentBll;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly SelLangBLL _selLangBll;

        #endregion

        #region Constructors

        public AdjustmentLimitController(IServiceProvider provider)
        {
            _paymentBll = provider.GetRequiredService<PaymentBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
        }

        #endregion

        #region API Model

        public class GetListRequest
        {
            public int SiteId { get; set; }
        }

        #endregion

        #region Actions

        /// <summary>
        /// AdjustmentLimit/GetList 取得調整限額清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Adjustment Limit Management > Click [SiteName]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 43, 1 })]
        [HttpPost]
        public ApiResult<IEnumerable<AdjustmentLimitData>> GetList(GetListRequest request)
        {
            var siteIds = new List<int>(); siteIds.Add(request.SiteId);
            var serverInfo = _serverInfoBll.GetSiteList().Where(x => x.SiteId == request.SiteId).First();
            var result = _paymentBll.GetAdjustmentData(siteIds, serverInfo.ICurrencyList, 1, 100,
                out int totalPages, out int rowCount);

            var apiResult = new ApiResult<IEnumerable<AdjustmentLimitData>>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success,
            };
            return apiResult;
        }

        /// <summary>
        /// AdjustmentLimit/Set 設定調整限額
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Adjustment Limit Management  > Click [SiteName] > [Edit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 43, 3 })]
        [HttpPost]
        public ApiResult Set(ApiRequestWithLog<AdjustmentLimit> request)
        {
            var result = _paymentBll.SetAdjustmentLimitInfo(
                new ParamsWithLog<AdjustmentLimit>(request, User.Identity.Name));
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
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
