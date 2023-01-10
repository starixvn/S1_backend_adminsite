using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Marketing.VerificationReward
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class VerificationRewardReportController : Controller
    {
        private readonly VerificationRewardBLL _verificationRewardBll;
        private readonly ExcelBLL _excelBll;
        private readonly NewAuthorizeBLL _authorizeBll;
        private readonly Common _common;
        private readonly CustInfoBLL _custInfoBll;

        public VerificationRewardReportController(IServiceProvider provider)
        {
            _verificationRewardBll = provider.GetService<VerificationRewardBLL>();
            _authorizeBll = provider.GetService<NewAuthorizeBLL>();
            _excelBll = provider.GetService<ExcelBLL>();
            _common = provider.GetService<Common>();
            _custInfoBll = provider.GetService<CustInfoBLL>();
        }

        #region API Model
        
        public class GetRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public string UserName { get; set; }
            public string SabaId { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }

            public DateTimeOffset StartDate
            {
                get => _startDate;
                set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }

            public DateTimeOffset EndDate
            {
                get => _endDate;
                set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
        }

        public class ExportRequest
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public string UserName { get; set; }
            public string SabaId { get; set; }

            public DateTimeOffset StartDate
            {
                get => _startDate;
                set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }

            public DateTimeOffset EndDate
            {
                get => _endDate;
                set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
        }

        #endregion

        /// <summary>
        /// VerificationRewardReport/Get 查詢用戶領取驗證獎勵報表
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Verification Reward Report > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public async Task<ApiListResult<CustVerificationRewardModel>> Get(GetRequest request)
        {
            var siteId = _authorizeBll.CheckBrand();
            var custId = await GetCustId(request.UserName, request.SabaId, siteId);
            if (custId == null)
            {
                return new ApiListResult<CustVerificationRewardModel>
                {
                    Data = Array.Empty<CustVerificationRewardModel>(),
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
            
            var data = _verificationRewardBll.GetVerificationRewardReportV2(
                siteId,
                request.CurrencyIds,
                custId.Value,
                request.StartDate,
                request.EndDate,
                request.PageSize,
                request.PageNumber,
                out var totalPages,
                out var rowCount);
            
            return new ApiListResult<CustVerificationRewardModel>
            {
                Data = data,
                TotalPages = totalPages,
                PageNumber = request.PageNumber,
                RowCount = rowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// VerificationRewardReport/Export 匯出用戶領取驗證獎勵報表
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Verification Reward Report > [Submit] > [Export Excel]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public async Task<IActionResult> Export(ExportRequest request)
        {
            var siteId = _authorizeBll.CheckBrand();
            var custId = await GetCustId(request.UserName, request.SabaId, siteId);

            var exportData = custId == null
                ? Array.Empty<ExportVerificationRewardModel>()
                : await _verificationRewardBll.GetVerificationRewardWithoutPageV2(siteId,
                    request.CurrencyIds,
                    custId.Value,
                    request.StartDate,
                    request.EndDate);

            var stream = _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "VerificationRewardReport");
        }

        private async Task<int?> GetCustId(string userName, string sabaId, int siteId)
        {
            if (string.IsNullOrWhiteSpace(userName) && string.IsNullOrWhiteSpace(sabaId))
                return 0;
            
            if (!string.IsNullOrWhiteSpace(sabaId))
                return _common.TpIdToCustId(sabaId);
            
            var cust = await _custInfoBll.GetCustId(null, userName, siteId);
            
            return cust?.CustId;
        }
    }
}