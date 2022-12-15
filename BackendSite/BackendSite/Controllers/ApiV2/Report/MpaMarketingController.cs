using BackendSite.Service.BLL;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.Marketing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class MpaMarketingController : ControllerBase
    {
        private readonly IMpaMarketingWithdrawalAmountBLL _marketingWithdrawalAmountBll;
        private readonly IMpaMarketingDepositAmountBLL _marketingDepositAmountBll;
        private readonly IMpaCommonService _mapMpaCommonService;
        private readonly ExcelBLL _excelBll;

        public MpaMarketingController(IServiceProvider provider)
        {
            this._marketingWithdrawalAmountBll = provider.GetRequiredService<IMpaMarketingWithdrawalAmountBLL>();
            this._marketingDepositAmountBll = provider.GetRequiredService<IMpaMarketingDepositAmountBLL>();
            this._mapMpaCommonService = provider.GetRequiredService<IMpaCommonService>();
            this._excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #region Withdrawal

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
            var currencyIdMapper = await _mapMpaCommonService.GetCurrencyIsoCodeMapper();

            var num = 1;
            detailData.ForEach(x =>
            {
                x.Num = num;
                num++;
            });

            var title = $"Withdrawal Amount, {request.StartDate.ToString("yyyy-dd-MM")} - {request.EndDate.ToString("yyyy-dd-MM")}, View in {currencyIdMapper[request.BaseCurrencyId]}";

            var stream = _marketingWithdrawalAmountBll.ExportMpaWithdrawal(summaryData, detailData,
                isCompare: request.IsCompare,
                summaryTitle: title,
                detailTitle: title);

            return _excelBll.ExcelFile(stream, $"WithdrawalExport");
        }

        #endregion

        #region Deposit

        [HttpPost]
        public async Task<ApiResult<DepositAmountSummaryResponse>> DepositSummaryAsync(DepositAmountRequest request)
        {
            var apiResult = new ApiResult<DepositAmountSummaryResponse>();

            try
            {
                var data = await _marketingDepositAmountBll.GetDepositAmountSummaryAsync(request);
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
        public async Task<ApiResult<DepositSinUpSummaryResponse>> DepositSignUpUrlSummaryAsync(DepositAmountRequest request)
        {
            var apiResult = new ApiResult<DepositSinUpSummaryResponse>();

            try
            {
                var data = await _marketingDepositAmountBll.GetDepositSinUpSummaryAsync(request);
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
        public ApiListResult<DepositSignUpDetailData> DepositSignUpDetail(DepositAmountRequest request)
        {
            var apiResult = new ApiListResult<DepositSignUpDetailData>();

            try
            {
                var result = _marketingDepositAmountBll.GetDepositSignUpDetail(request, out var pageInfo);
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
        public async Task<IActionResult> DepositExportAsync(DepositAmountRequest request)
        {
            var depositAmountSummary = await _marketingDepositAmountBll.GetDepositAmountSummaryAsync(request);
            var depositSignUpSummary = await _marketingDepositAmountBll.GetDepositSinUpSummaryExportData(request);
            var depositSignUpDetail = _marketingDepositAmountBll.GetDepositSignUpDetail(request, out var pageInfo);

            var num = 1;
            depositSignUpDetail.ForEach(x =>
            {
                x.Num = num;
                num++;
            });

            var stream = _marketingDepositAmountBll.ExportMpaDeposit(
                depositAmountSummary: depositAmountSummary,
                depositSinUpSummary: depositSignUpSummary,
                depositSignUpDetail: depositSignUpDetail,
                request);

            return _excelBll.ExcelFile(stream, $"DepositExport{Guid.NewGuid()}");
        }

        #endregion
    }
}