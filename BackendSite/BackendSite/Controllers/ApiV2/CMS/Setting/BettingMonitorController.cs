using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BettingMonitorController : ControllerBase
    {
        #region Fields

        private readonly BettingMonitorBLL _bettingMonitorBll;
        private readonly SelLangBLL _selLangBll;

        #endregion

        #region Constructor

        public BettingMonitorController(IServiceProvider provider)
        {
            _bettingMonitorBll = provider.GetRequiredService<BettingMonitorBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
        }

        #endregion

        #region API Model

        public class GetRequest : ApiRequestPageModel
        {
            public int SiteId { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public IEnumerable<int> TpIds { get; set; }
            public IEnumerable<int> CategoryIds { get; set; }
            public IEnumerable<int> GameTypes { get; set; }
        }

        public class BaseSetting
        {
            public int TpId { get; set; }
            public int GameType { get; set; }
        }
        
        public class SetRequest : BaseSetting
        {
            public int Frequency { get; set; }
            public int Status { get; set; }
        }

        #endregion

        #region API
        
        [HttpPost]
        public ApiListResult<BettingMonitorModel> GetList(GetRequest request)
        {
            var apiResult = new ApiListResult<BettingMonitorModel>();
            try
            {
                apiResult.Data = _bettingMonitorBll.GetList(
                    request.SiteId,
                    request.CurrencyIds,
                    request.TpIds,
                    request.CategoryIds,
                    request.GameTypes,
                    request.PageNumber,
                    request.PageSize,
                    out var totalPages,
                    out var rowCount);
                
                apiResult.PageNumber = request.PageNumber;
                apiResult.RowCount = rowCount;
                apiResult.TotalPages = totalPages;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }
            return apiResult;
        }
        
        [HttpPost]
        public ApiResult SetSetting(ApiRequestWithLog<SetRequest> request)
        {
            var result = _bettingMonitorBll.SetSetting(new ParamsWithLog<SetRequest>(request, User.Identity.Name));
            
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

        [HttpPost]
        public ApiResult DelSetting(ApiRequestWithLog<BaseSetting> request)
        {
            var result = _bettingMonitorBll.DelSetting(new ParamsWithLog<BaseSetting>(request, User.Identity.Name));
            
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
