using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackendSite.Controllers.ApiV2.Finance.WithdrawalSetting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class WithdrawalLimitUSDTController : ControllerBase
    {
        #region Fields

        private readonly WithdrawalBLL _withdrawalBll;
        private readonly SelLangBLL _selLangBll;

        #endregion

        #region Constructors

        public WithdrawalLimitUSDTController(IServiceProvider provider)
        {
            _withdrawalBll = provider.GetRequiredService<WithdrawalBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
        }

        #endregion

        #region API Model

        public class GetListRequest : ApiRequestPageModel
        {
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public NetworkTypeEnum NetworkType { get; set; }
        }

        public class CreateOrUpdateRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int VipLevel { get; set; }
            public double WithdrawalMin { get; set; }
            public double WithdrawalMax { get; set; }
            public NetworkTypeEnum NetworkType { get; set; }
        }

        public class DeleteRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int VipLevel { get; set; }
            public NetworkTypeEnum NetworkType { get; set; }
        }

        #endregion

        #region Actions

        /// <summary>
        /// WithdrawalLimitUSDT/GetList 取得USDT提款限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > USDT Withdrawal Limit Management > Click [SiteName] & [NetWork]
        /// </remarks>
        /// <param name="USDTwithdrawalLimit"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/USDTWithdrawalLimitManagement" })]
        [HttpPost]
        public ApiListResult<USDTWithdrawalLimitInfoManage> GetList(GetListRequest request)
        {
            var apiResult = new ApiListResult<USDTWithdrawalLimitInfoManage>();

            var datas = _withdrawalBll.GetUSDTWithdrawalLimitInfos(request.SiteIds,
                request.CurrencyIds,
                Enum.GetName(typeof(NetworkTypeEnum), request.NetworkType),
                request.PageNumber,
                request.PageSize,
                out int totalPages,
                out int rowCount);

            apiResult.Data = datas;
            apiResult.PageNumber = request.PageNumber;
            apiResult.TotalPages = totalPages;
            apiResult.RowCount = rowCount;
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        /// <summary>
        /// WithdrawalLimitUSDT/Create 新增USDT提款限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > USDT Withdrawal Limit Management > Click [SiteName] & [NetWork]  > [Add] > [Save]
        /// </remarks>
        /// <param name="USDTwithdrawalLimitInfoManage"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/USDTWithdrawalLimitManagement" })]
        [HttpPost]
        public ApiResult Create(CreateOrUpdateRequest request)
        {
            var data = new USDTWithdrawalLimitInfoManage()
            {
                SiteId = request.SiteId,
                CurrencyId = request.CurrencyId,
                VipLevel = request.VipLevel,
                WithdrawalMax = request.WithdrawalMax,
                WithdrawalMin = request.WithdrawalMin,
                NetworkType = (int)request.NetworkType,
            };

            var result = _withdrawalBll.AddUSDTWithdrawalLimit(data, User.Identity.Name);
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.UpdateFailed;
                apiResult.Message = _selLangBll.GetMsg("msg_Fail");
            }

            return apiResult;
        }

        /// <summary>
        /// WithdrawalLimitUSDT/Update 更新USDT提款限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > USDT Withdrawal Limit Management > Click [SiteName] & [NetWork] > [Edit] > [Save]
        /// </remarks>
        /// <param name="USDTwithdrawalLimitInfoManage"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/USDTWithdrawalLimitManagement" })]
        [HttpPost]
        public ApiResult Update(CreateOrUpdateRequest request)
        {
            var data = new USDTWithdrawalLimitInfoManage()
            {
                SiteId = request.SiteId,
                CurrencyId = request.CurrencyId,
                VipLevel = request.VipLevel,
                WithdrawalMax = request.WithdrawalMax,
                WithdrawalMin = request.WithdrawalMin,
                NetworkType = (int)request.NetworkType,
            };

            var result = _withdrawalBll.EditUSDTWithdrawalLimit(data, User.Identity.Name);
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = 0;
            }
            else
            {
                apiResult.ErrorCode = 9;
                apiResult.Message = _selLangBll.GetMsg("msg_Fail");
            }

            return apiResult;
        }

        /// <summary>
        /// WithdrawalLimitUSDT/Delete 刪除USDT提款限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > USDT Withdrawal Limit Management > Click [SiteName] & [NetWork] > [Delete]
        /// </remarks>
        /// <param name="USDTwithdrawalLimitInfoManage"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/USDTWithdrawalLimitManagement" })]
        [HttpPost]
        public ApiResult Delete(DeleteRequest request)
        {
            var data = new USDTWithdrawalLimitInfoManage()
            {
                SiteId = request.SiteId,
                CurrencyId = request.CurrencyId,
                VipLevel = request.VipLevel,
                NetworkType = (int)request.NetworkType,
            };

            var result = _withdrawalBll.DeleteUSDTWithdrawalLimit(data, User.Identity.Name);
            var apiResult = new ApiResult();
            if (result > 0)
            {
                apiResult.ErrorCode = 0;
            }
            else
            {
                apiResult.ErrorCode = 9;
                apiResult.Message = _selLangBll.GetMsg("msg_Fail");
            }

            return apiResult;
        }

        #endregion

    }
}
