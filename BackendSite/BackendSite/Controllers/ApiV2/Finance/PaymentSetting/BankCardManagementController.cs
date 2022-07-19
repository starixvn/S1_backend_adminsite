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
using System.Linq;
using BackendSite.Service.Model.BankCardManagement;

namespace BackendSite.Controllers.ApiV2.Finance.PaymentSetting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BankCardManagementController : ControllerBase
    {

        #region Fields

        private readonly SelLangBLL _selLangBll;
        private readonly BankDetailBLL _bankDetailBll;

        #endregion

        #region Contructors

        public BankCardManagementController(IServiceProvider provider)
        {
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
            _bankDetailBll = provider.GetRequiredService<BankDetailBLL>();
        }

        #endregion

        #region API Model

        public class GetListRequest : ApiRequestPageModel
        {
            public IEnumerable<int> SiteIds { get; set; }

            public IEnumerable<int> CurrencyIds { get; set; }

            public List<BankDetailOffStatusEnum> Statuses { get; set; }
        }

        #endregion

        #region Actions

        /// <summary>
        /// BankCardManagement/GetList 取得Offline銀行卡設定清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Setting > Bank Acc Management > Click [Currency Tab] > List
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<OfflineBankingSetting> GetList(GetOfflineBankingSettingRequest request)
        {
            var result = _bankDetailBll.GetOfflineBankingSettingList(
                new GetOfflineBankingSettingParams(request), out var totalPages, out var rowCount);
            
            return new ApiListResult<OfflineBankingSetting>
            {
                Data = result,
                PageNumber = request.PageNumber,
                TotalPages = totalPages,
                RowCount = rowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// BankCardManagement/Set Offline銀行卡設定
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Setting > Bank Acc Management > Click [Currency Tab] > List > [Edit]
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult Set(ApiRequestWithLog<SetBankCard> request)
        {
            // if (request.CurrencyId != 61)
            // {
            //     request.Before.IFSCCode = "";
            //     request.After.IFSCCode = "";
            // }
            var result = _bankDetailBll.SetBankCardwithIFSC(
                new ParamsWithLog<SetBankCard>(request, User.Identity.Name));

            var apiResult = new ApiResult();
            if (result > 0)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.UpdateFailed;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }
        #endregion

    }
}
