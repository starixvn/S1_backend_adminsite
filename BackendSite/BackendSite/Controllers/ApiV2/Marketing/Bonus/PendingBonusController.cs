using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using static BackendSite.Service.BLL.PendingFundBLL;

namespace BackendSite.Controllers.ApiV2.Marketing.Bonus
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class PendingBonusController : ControllerBase
    {
        #region Fields

        private readonly BonusInfoBLL _bonusInfoBll;
        private readonly PendingFundBLL _pendingFundBll;
        private readonly SelLangBLL _selLangBll;

        #endregion

        #region Constructor

        public PendingBonusController(IServiceProvider provider)
        {
            _bonusInfoBll = provider.GetRequiredService<BonusInfoBLL>();
            _pendingFundBll = provider.GetRequiredService<PendingFundBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
        }

        #endregion

        #region API Model

        public class TransferListRequest
        {
            public List<int> BonusIdList { get; set; }
        }

        #endregion

        /// <summary>
        /// PendingBonus/GetList 取得待發獎金清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Pending Bonus
        /// </remarks>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/PendingBonus" })]
        [HttpPost]
        public ApiListResult<PendingBonus> GetList(ApiRequestPageModel request)
        {
            var data = _bonusInfoBll.GetPendingBonusList(request.PageNumber, request.PageSize, out int totalPages, out int rowCount);

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }

            var apiResult = new ApiListResult<PendingBonus>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success,
                PageNumber = request.PageNumber,
                TotalPages = totalPages,
                RowCount = rowCount
            };

            return apiResult;
        }

        /// <summary>
        /// PendingBonus/TransferMulti 發放多筆指定客戶獎金
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Pending Bonus > [Transfer] Or [Transfer Multi]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/PendingBonus" })]
        [HttpPost]
        public async Task<ApiResult<List<TransferMultiResult>>> TransferMultiAsync(TransferListRequest request)
        {
            var resultData = await _pendingFundBll.TransferMultiAsync(request.BonusIdList);

            var finalResult = ErrorCodeEnum.Success;

            if (resultData.Select(x => x.Result.ErrorCode).Contains((int)ErrorCodeEnum.Fail))
            {
                finalResult = ErrorCodeEnum.Fail;
            }

            return new ApiResult<List<TransferMultiResult>>
            {
                Data = resultData,
                ErrorCode = (int)finalResult
            };
        }

        /// <summary>
        /// PendingBonus/TransferAll 發放全部客戶獎金
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Pending Bonus > [TransferAll] 
        /// </remarks>
        /// <returns></returns>
        //[TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/PendingBonus" })]
        //[HttpPost]
        //public async Task<ApiResult> TransferAllAsync()
        //{
        //    var apiResult = await _pendingFundBll.BonusTransferAllAsync();

        //    if (apiResult.ErrorCode == (int)ErrorCodeEnum.Success)
        //    {
        //        apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
        //    }

        //    return apiResult;
        //}
    }
}