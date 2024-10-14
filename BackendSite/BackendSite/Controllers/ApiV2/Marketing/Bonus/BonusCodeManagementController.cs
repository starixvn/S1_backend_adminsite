using System.ComponentModel.DataAnnotations;

using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Marketing.Bonus
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BonusCodeManagementController : Controller
    {
        #region Fields

        private readonly BonusCodeBLL _bonusCodeBll;
        private readonly NewAuthorizeBLL _newAuthorizeBll;

        #endregion

        #region Constructor

        public BonusCodeManagementController(IServiceProvider provider)
        {
            _bonusCodeBll = provider.GetRequiredService<BonusCodeBLL>();
            _newAuthorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
        }

        #endregion

        #region API MODEL
        public class GetListRequest
        {
            public int TpId { get; set; }
            public string GameId { get; set; }
            public int CurrencyId { get; set; }
            public DateTimeOffset StartDate { get; set; }
            public DateTimeOffset EndDate { get; set; }
        }

        public class CreateRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int TpId { get; set; }
            public string GameId { get; set; }
            public string BonusCode { get; set; }
            public int Round { get; set; }
            
        }

        #endregion

        #region Action
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 155, 1 })]
        public ApiResult<IEnumerable<TpBonusCodeList>> GetList(GetListRequest request)
        {
            var siteId = _newAuthorizeBll.CheckBrand();
            var startTime = request.StartDate.ToUniversalTime().DateTime;
            var endTime = request.EndDate.ToUniversalTime().DateTime;

            var data = _bonusCodeBll.GetList(siteId,request.TpId,request.CurrencyId,request.GameId,startTime,endTime);

            return new ApiResult<IEnumerable<TpBonusCodeList>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 155, 1 })]
        public async Task<ApiResult> CreateAsync(CreateRequest request)
        {
            var result = await _bonusCodeBll.CreateBonusCode(request.SiteId, request.CurrencyId, request.TpId, request.GameId,
                request.Round, request.BonusCode);

            return result;
        }
        #endregion
    }
}
