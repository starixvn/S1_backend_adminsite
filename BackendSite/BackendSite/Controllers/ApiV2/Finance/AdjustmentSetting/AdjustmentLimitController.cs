using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BackendSite.Controllers.ApiV2.Finance.AdjustmentSetting
{
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class AdjustmentLimitController : ControllerBase
    {

        #region Fields

        private readonly PaymentBLL _paymentBll;
        private readonly SelLangBLL _selLangBll;

        #endregion

        #region Constructors

        public AdjustmentLimitController(IServiceProvider provider)
        {
            _paymentBll = provider.GetRequiredService<PaymentBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
        }

        #endregion

        #region API Model

        public class GetAdjustmentRequest
        {
            public int SiteId { get; set; }

            public int CurrencyId { get; set; }
        }


        public class AddAdjustmentLimitRequest
        {
            public int SiteId { get; set; }

            public int CurrencyId { get; set; }
            public decimal AdjustmentMax { get; set; }
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/AdjustmentLimitManagement" })]
        [HttpPost]
        public ApiResult<AdjustmentLimtData> GetList(GetAdjustmentRequest request)
        {
            var apiResult = new ApiResult<AdjustmentLimtData>
            {
                Data = _paymentBll.GetAdjustmentData(request.SiteId, request.CurrencyId),
                ErrorCode = (int)ErrorCodeEnum.Success
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/AdjustmentLimitManagement" })]
        [HttpPost]
        public ApiResult Set(AddAdjustmentLimitRequest request)
        {
            var result =
                _paymentBll.AddAdjustmentLimitInfo(request.SiteId, request.CurrencyId, request.AdjustmentMax,
                    User.Identity.Name);
            var apiResult = new ApiResult();
            if (result > 0)
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
