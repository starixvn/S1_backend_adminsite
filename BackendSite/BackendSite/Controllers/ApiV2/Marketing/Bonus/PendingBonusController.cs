using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

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

        public class BonusTransferReq
        {
            public int CustId { get; set; }
            public int TpId {get; set; }
        }

        #endregion
        
        [HttpPost]
        public ApiResult<IEnumerable<PendingBonus>> GetPendingBonusList()
        {
            var apiResult = new ApiResult<IEnumerable<PendingBonus>>
            {
                Data = _bonusInfoBll.GetPendingBonusList(),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
            return apiResult;
        }
        
        [HttpPost]
        public async Task<ApiResult> BonusTransferAsync(BonusTransferReq request)
        {
            var apiResult = await _pendingFundBll.BonusTransferAsync(request.CustId, request.TpId);

            if (apiResult.ErrorCode == (int)ErrorCodeEnum.Success)
            {
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.Message = string.IsNullOrWhiteSpace(apiResult.RawMessage) ? apiResult.Message : apiResult.RawMessage;
            }

            return apiResult;
        }
        
        [HttpPost]
        public async Task<ApiResult> BonusTransferAllAsync()
        {
            var apiResult = await _pendingFundBll.BonusTransferAllAsync();

            if (apiResult.ErrorCode == (int)ErrorCodeEnum.Success)
            {
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }

            return apiResult;
        }
    }
}