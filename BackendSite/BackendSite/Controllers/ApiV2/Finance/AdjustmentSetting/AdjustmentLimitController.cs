using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace BackendSite.Controllers.ApiV2.Finance.AdjustmentSetting
{
    [Authorize]
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

        public class GetListRequest : ApiRequestPageModel
        {
            public IEnumerable<int> SiteIds { get; set; }

            public IEnumerable<int> CurrencyIds { get; set; }
        }


        public class SetRequest
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
        public ApiListResult<AdjustmentLimitData> GetList(GetListRequest request)
        {
            var result = _paymentBll.GetAdjustmentData(request.SiteIds, request.CurrencyIds, request.PageNumber, request.PageSize,
                out int totalPages, out int rowCount);

            var apiResult = new ApiListResult<AdjustmentLimitData>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success,
                PageNumber = request.PageNumber,
                TotalPages = totalPages,
                RowCount = rowCount
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
