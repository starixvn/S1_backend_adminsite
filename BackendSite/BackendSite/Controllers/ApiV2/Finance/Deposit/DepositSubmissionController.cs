using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;

namespace BackendSite.Controllers.ApiV2.Finance.Deposit
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class DepositSubmissionController : ControllerBase
    {
        #region Fields

        private readonly DepositBLL _depositBll;
        private readonly SelLangBLL _selLangBll;

        #endregion

        #region Constructors

        public DepositSubmissionController(IServiceProvider provider)
        {
            _depositBll = provider.GetRequiredService<DepositBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
        }

        #endregion

        #region API Model

        public class DepositSubmissionRequest
        {
            public DepositDetailInfo depositDetailInfo { get; set; }
            public IFormFile Receipt { get; set; }
        }

        #endregion

        #region Action

        /// <summary>
        /// DepositSubmission/Submit 提交存款單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Deposit Submission > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositSubmission" })]
        [HttpPost]
        public ApiResult Submit([FromForm] DepositSubmissionRequest request)
        {

            var result = _depositBll.CreateDeposit(request.depositDetailInfo, request.Receipt);
            var apiResult = new ApiResult();
            if (result == 1)
            {
                apiResult.ErrorCode = 1;
                apiResult.Message = _selLangBll.GetMsg("msg_InsertSuccessful");
            }
            else if (result == -1)
            {
                apiResult.ErrorCode = -1;
                apiResult.Message = _selLangBll.GetMsg("msg_UserNotFound");
            }
            else
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = _selLangBll.GetMsg("msg_InsertFailed");
            }
            return apiResult;
        }

        #endregion

    }
}
