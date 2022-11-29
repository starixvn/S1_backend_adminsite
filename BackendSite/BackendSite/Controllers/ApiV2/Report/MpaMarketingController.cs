using System.Configuration;
using BackendSite.Service.BLL;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.BLL.Mpa;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.Marketing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.HPSF;

namespace BackendSite.Controllers.ApiV2.Report
{
    //[Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class MpaMarketingController : ControllerBase
    {
        private readonly IMpaMarketingWithdrawalAmountBLL _marketingWithdrawalAmountBll;
        private readonly ExcelBLL _excelBll;

        public MpaMarketingController(IMpaMarketingWithdrawalAmountBLL marketingWithdrawalAmountBll, ExcelBLL excelBll)
        {
            this._marketingWithdrawalAmountBll = marketingWithdrawalAmountBll;
            this._excelBll = excelBll;
        }

        [HttpPost]
        public async Task<ApiResult<WithdrawalAmountSummaryResponse>> WithdrawalSummaryAsync(WithdrawalAmountRequest request)
        {
            var apiResult = new ApiResult<WithdrawalAmountSummaryResponse>();

            try
            {
                var data = await _marketingWithdrawalAmountBll.GetWithdrawalAmountSummaryAsync(request);
                apiResult.Data = data;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        public ApiListResult<WithdrawalAmountDetailData> WithdrawalDetailsAsync(WithdrawalAmountRequest request)
        {
            var apiResult = new ApiListResult<WithdrawalAmountDetailData>();

            try
            {
                var result = _marketingWithdrawalAmountBll.GetWithdrawalAmountDetail(request, out var pageInfo);
                apiResult.Data = result;
                apiResult.PageNumber = request.PageNumber;
                apiResult.RowCount = pageInfo.RowCount;
                apiResult.TotalPages = pageInfo.TotalPages;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        public async Task<IActionResult> WithdrawalExportAsync(WithdrawalAmountRequest request)
        {
            var summaryData = await _marketingWithdrawalAmountBll.GetWithdrawalAmountSummaryAsync(request);
            var detailData = _marketingWithdrawalAmountBll.GetWithdrawalAmountDetail(request, out var pageInfo);

            var num = 1;
            detailData.ForEach(x =>
            {
                x.Num = num;
                num++;
            });

            var title = $"Withdrawal Amount, {request.StartDate.ToString("yyyy-dd-MM")} - {request.EndDate.ToString("yyyy-dd-MM")}, View in USD";

            var stream = _marketingWithdrawalAmountBll.ExportMpaWithdrawal(summaryData, detailData,
                isCompare: request.IsCompare,
                summaryTitle: title,
                detailTitle: title);

            return _excelBll.ExcelFile(stream, $"WithdrawalExport{Guid.NewGuid()}");
        }
    }
}