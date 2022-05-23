using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BackendSite.Controllers.ApiV2.Finance.PaymentSetting
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class ThirdPartyManagementController : ControllerBase
    {
        #region Fields

        private readonly PaymentManageBLL _paymentManageBll;
        private readonly PermissionBLL _permissionBll;

        #endregion

        #region Constructors

        public ThirdPartyManagementController(IServiceProvider provider)
        {
            _paymentManageBll = provider.GetRequiredService<PaymentManageBLL>();
            _permissionBll = provider.GetRequiredService<PermissionBLL>();
        }

        #endregion

        #region API Model

        public class UpdatePayInfoRequest
        {
            public int PayId { get; set; }

            /// <summary>
            /// 狀態(1=Enable、2=Disable)
            /// </summary>
            public PayListStatusEnum Status { get; set; }
        }

        public class UpdatePayChannelRequest
        {
            public int ChannelId { get; set; }

            public int Sort { get; set; }

            /// <summary>
            /// 狀態(1=Enable、2=Disable)
            /// </summary>
            public PayChannelStatusEnum Status { get; set; }

            public int PointLevel { get; set; }
        }

        public class UpdatePayCurrencyRequest
        {
            public int ChannelId { get; set; }

            public int CurrencyId { get; set; }

            public decimal MinAmount { get; set; }

            public decimal MaxAmount { get; set; }

            /// <summary>
            /// 數字規則(1=Integer、2=Decimal、3=Fixed)
            /// </summary>
            public AmountRuleEnum AmountRule { get; set; }

            public IEnumerable<decimal> FixedAmountList { get; set; }
        }

        public class GetBankListByPayIdRequest
        {
            public int PayId { get; set; }
        }

        public class UpdateBankInfoByPayIdRequest
        {
            public int PayId { get; set; }

            public int SysId { get; set; }

            /// <summary>
            /// 狀態(1=Enable、2=Disable)
            /// </summary>
            public BankInfoPayStatusEnum Status { get; set; }
        }

        #endregion

        #region Actions

        /// <summary>
        /// ThirdPartyManagement/GetList 取得第三方支付資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Payment Management
        /// </remarks>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/PaymentManage" })]
        [HttpPost]
        public ApiResult GetList()
        {
            var result = new ApiResult<FrontEndDatas>();

            result.Data = _paymentManageBll.GetPayInfoList();
            result.ErrorCode = (int)ErrorCodeEnum.Success;

            return result;
        }

        /// <summary>
        /// ThirdPartyManagement/UpdatePayInfo 更新第三方支付資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Payment Management > Pay Tab > Click Pay [Status]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/PaymentManage" })]
        [HttpPost]
        public ApiResult UpdatePayInfo(UpdatePayInfoRequest request)
        {
            var result = new ApiResult();

            _paymentManageBll.UpdatePayList(request.PayId, (int)request.Status, User.Identity.Name);
            result.ErrorCode = (int)ErrorCodeEnum.Success;

            return result;
        }

        /// <summary>
        /// ThirdPartyManagement/UpdatePayChannel 更新支付渠道資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Payment Management > Pay Tab > Channel Tab > Click Channel [Status]
        /// 
        ///            Finance > Payment Management > Pay Tab > Channel Tab > Click Channel [Edit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/PaymentManage" })]
        [HttpPost]
        public ApiResult UpdatePayChannel(UpdatePayChannelRequest request)
        {
            var result = new ApiResult();

            _paymentManageBll.UpdatePayChannel(request.ChannelId, request.Sort, (int)request.Status, request.PointLevel, User.Identity.Name);
            result.ErrorCode = (int)ErrorCodeEnum.Success;

            return result;
        }

        /// <summary>
        /// ThirdPartyManagement/UpdatePayCurrency 更新支付幣別資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Payment Management > Pay Tab > Channel Tab > Click Currency [Edit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/PaymentManage", "f_pm_editAmountRule" })]
        [HttpPost]
        public ApiResult UpdatePayCurrency(UpdatePayCurrencyRequest request)
        {
            var result = new ApiResult();
            string amountShow = null;
            if (request.AmountRule == AmountRuleEnum.Fixed)
            {
                amountShow = JsonConvert.SerializeObject(request.FixedAmountList);
            }

            if (_permissionBll.CheckPermission("Finance/PaymentManage", "f_pm_editAmountRule"))
            {
                //有權限，可以修改數字規則和固定數字清單
                _paymentManageBll.UpdatePayCurrency(request.ChannelId,
                request.CurrencyId,
                request.MinAmount,
                request.MaxAmount,
                (int)request.AmountRule,
                amountShow,
                User.Identity.Name);
            }
            else
            {
                //沒有權限
                _paymentManageBll.UpdatePayCurrency(request.ChannelId,
                    request.CurrencyId,
                    request.MinAmount,
                    request.MaxAmount,
                    User.Identity.Name);
            }
            result.ErrorCode = (int)ErrorCodeEnum.Success;

            return result;
        }

        //重複的邏輯...UpdatePayCurrency
        //[TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/PaymentManage" })]
        //[HttpPost]
        //public ApiResult UpdatePayCurrencyAmount([FromForm] int ChannelId, [FromForm] int CurrencyId, [FromForm] decimal MinAmount, [FromForm] decimal MaxAmount)
        //{
        //    var result = new ApiResult();

        //    _paymentManageBll.UpdatePayCurrency(ChannelId, CurrencyId, MinAmount, MaxAmount, User.Identity.Name);

        //    result.ErrorCode = (int)ErrorCodeEnum.Success;

        //    return result;
        //}

        /// <summary>
        /// ThirdPartyManagement/GetBankListByPayId 取得指定第三方支付銀行清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Payment Management > Pay Tab > Channel Tab > Click [Type]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/PaymentManage" })]
        [HttpPost]
        public ApiResult GetBankListByPayId(GetBankListByPayIdRequest request)
        {
            var result = new ApiResult<IEnumerable<PayListBankInfoSys>>();

            result.Data = _paymentManageBll.GetBankSysInfoList(request.PayId);
            result.ErrorCode = (int)ErrorCodeEnum.Success;

            return result;
        }

        /// <summary>
        /// ThirdPartyManagement/UpdateBankInfoByPayId 更新指定第三方支付銀行資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Payment Management > Pay Tab > Channel Tab > Click [Type] > Click [Status]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/PaymentManage" })]
        [HttpPost]
        public ApiResult UpdateBankInfoByPayId(UpdateBankInfoByPayIdRequest request)
        {
            var result = new ApiResult<IEnumerable<PayListBankInfoSys>>();

            _paymentManageBll.UpdateBankInfo(request.PayId, request.SysId, (int)request.Status, User.Identity.Name);
            result.ErrorCode = (int)ErrorCodeEnum.Success;

            return result;
        }

        #endregion

    }
}
