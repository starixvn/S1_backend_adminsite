using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BackendSite.Controllers.ApiV2.Member.Verify
{

    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CustomerVerifyController : ControllerBase
    {
        private readonly CustInfoBLL _custInfoBll;
        private readonly Common _common;

        public CustomerVerifyController(IServiceProvider provider)
        {
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _common = provider.GetRequiredService<Common>();
        }

        public class VerifyRequest
        {
            public int SiteId { get; set; }
            public string SabaId { get; set; }
            public string UserName { get; set; }
        }

        [HttpPost]
        public ApiResult<CustInfoDetail> GetCustDetail(VerifyRequest request)
        {
            var apiResult = new ApiResult<CustInfoDetail>();

            var custUsername = string.IsNullOrEmpty(request.UserName) ? null : request.UserName;
            var custId = (string.IsNullOrEmpty(request.SabaId)) ? null : _common.TpIdToCustId(request.SabaId);

            var custInfosDetail = _custInfoBll.GetCustInfoDetails(custId, custUsername, request.SiteId);

            if (custInfosDetail != null)
            {
                switch (custInfosDetail.Status)
                {
                    case (int)CustomerStatusEnum.Running:
                        apiResult.Data = custInfosDetail;
                        apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                        break;

                    case (int)CustomerStatusEnum.Suspend:
                        throw new AdminException(ErrorCodeEnum.CustStatusIsSuspend);

                    case (int)CustomerStatusEnum.Closed:
                        throw new AdminException(ErrorCodeEnum.CustStatusIsClosed);
                }
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.CustNotFound);
            }
            return apiResult;
        }

    }
}
