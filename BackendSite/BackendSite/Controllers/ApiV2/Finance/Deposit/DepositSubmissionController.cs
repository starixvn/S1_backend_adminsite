using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

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
        private readonly AdjustmentLogReportBLL _adjustmentLogReportBll;

        #endregion

        #region Constructors

        public DepositSubmissionController(IServiceProvider provider)
        {
            _depositBll = provider.GetRequiredService<DepositBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
            _adjustmentLogReportBll = provider.GetRequiredService<AdjustmentLogReportBLL>();
        }

        #endregion

        #region API Model

        public class DepositSubmissionRequest
        {
            public DepositDetailInfo Detail { get; set; }
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
        public async Task<ApiResult> SubmitAsync([FromForm] DepositSubmissionRequest request)
        {
            var result = await _depositBll.CreateDeposit(request.Detail, request.Receipt);
            
            var apiResult = new ApiResult();
            if (result == 1)
            {
                _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                {
                    Operator = request.Detail.Operator,
                    SiteId = request.Detail.SiteId,
                    CurrencyId = request.Detail.CurrencyId,
                    ActionType = ActionType.Create,
                    MenuEnum = AdminAdjustmentLogMenuEnum.DepositSubmission,
                    BeforeDetailInfo = null,
                    AfterDetailInfo = JsonConvert.SerializeObject(request.Detail)
                });
                
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_InsertSuccessful");
            }
            else if (result == -1)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                apiResult.Message = _selLangBll.GetMsg("msg_UserNotFound");
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                apiResult.Message = _selLangBll.GetMsg("msg_InsertFailed");
            }
            return apiResult;
        }

        #endregion

    }
}
