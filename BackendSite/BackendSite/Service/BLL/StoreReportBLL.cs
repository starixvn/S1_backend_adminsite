using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Option;
using BackendSite.Service.Model.Store.Report;
using NPOI.XSSF.UserModel;

namespace BackendSite.Service.BLL
{
    public class StoreReportBLL
    {
        private readonly ILogger<StoreReportBLL> _logger;
        private readonly CourierAdapter _courierAdapter;
        private readonly string _storeSiteUrl;

        public StoreReportBLL(IServiceProvider provider)
        {
            _logger = provider.GetRequiredService<ILogger<StoreReportBLL>>();
            var configBll = provider.GetRequiredService<ConfigBLL>();
            _storeSiteUrl = configBll.GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").StoreService;
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
        }

        public Task<ApiResult<object>> GetDepositPointReportAsync(GetDepositPointReportRequest req)
        {
            var url = _storeSiteUrl + "Report/GetDepositPointReport";
            var result = _courierAdapter.PostAsync<ApiResult<object>, GetDepositPointReportRequest>(url, req);

            return result;
        }

        public async Task<byte[]> ExportDepositPointReportAsync(ExportDepositPointReportRequest req)
        {
            var url = _storeSiteUrl + "Report/ExportDepositPointReport";
            var exportResult =
                await _courierAdapter.PostAsync<ApiResult<ExportDepositPointReportResult>, ExportDepositPointReportRequest>(url, req);

            if (exportResult.ErrorCode != (int)ErrorCodeEnum.Success)
            {
                _logger.LogError($"StoreService ExportDepositPointReport Failed. ErrorCode: {exportResult.ErrorCode}");
                throw new AdminException("ExportDepositPointReport Failed.");
            }

            var offset = req.StartTime.Offset;
            var workbook = new XSSFWorkbook();
            var sheet1 = workbook.CreateSheet("DepositPointReport");
            var sheet2 = workbook.CreateSheet("Overview");

            sheet1.WriteToSheet(exportResult.Data.List, true, offset);
            sheet2.WriteToSheet(exportResult.Data.Overview, true);

            await using var memoryStream = new MemoryStream();
            workbook.Write(memoryStream);
            var result = memoryStream.ToArray();

            return result;
        }

        public Task<ApiResult<object>> GetAdjustmentPointReportAsync(GetAdjustmentPointReportRequest req)
        {
            var url = _storeSiteUrl + "Report/GetAdjustmentPointReport";
            var result = _courierAdapter.PostAsync<ApiResult<object>, GetAdjustmentPointReportRequest>(url, req);

            return result;
        }

        public async Task<byte[]> ExportAdjustmentPointReportAsync(ExportAdjustmentPointReportRequest req)
        {
            var url = _storeSiteUrl + "Report/ExportAdjustmentPointReport";
            var exportResult =
                await _courierAdapter.PostAsync<ApiResult<ExportAdjustmentPointReportResult>, ExportAdjustmentPointReportRequest>(url, req);

            if (exportResult.ErrorCode != (int)ErrorCodeEnum.Success)
            {
                _logger.LogError($"StoreService ExportAdjustmentPointReport Failed. ErrorCode: {exportResult.ErrorCode}");
                throw new AdminException("ExportAdjustmentPointReport Failed.");
            }

            var offset = req.StartTime.Offset;
            var workbook = new XSSFWorkbook();
            var sheet1 = workbook.CreateSheet("AdjustmentPointReport");
            var sheet2 = workbook.CreateSheet("Overview");

            sheet1.WriteToSheet(exportResult.Data.List, true, offset);
            sheet2.WriteToSheet(exportResult.Data.Overview, true);

            await using var memoryStream = new MemoryStream();
            workbook.Write(memoryStream);
            var result = memoryStream.ToArray();

            return result;
        }

        public async Task<ApiResult<object>> GetTurnoverPointReportAsync(GetTurnoverPointReportRequest req)
        {
            var url = _storeSiteUrl + "Report/GetTurnoverPointReport";
            var result = await _courierAdapter.PostAsync<ApiResult<object>, GetTurnoverPointReportRequest>(url, req);

            return result;
        }

        public async Task<byte[]> ExportTurnoverPointReportAsync(ExportTurnoverPointReportRequest req)
        {
            var url = _storeSiteUrl + "Report/ExportTurnoverPointReport";
            var exportResult =
                await _courierAdapter.PostAsync<ApiResult<ExportDepositPointReportResult>, ExportTurnoverPointReportRequest>(url, req);

            if (exportResult.ErrorCode != (int)ErrorCodeEnum.Success)
            {
                _logger.LogError($"StoreService ExportTurnoverPointReport Failed. ErrorCode: {exportResult.ErrorCode}");
                throw new AdminException("ExportTurnoverPointReport Failed.");
            }

            var offset = req.StartTime.Offset;
            var workbook = new XSSFWorkbook();
            var sheet1 = workbook.CreateSheet("TurnoverPointReport");
            var sheet2 = workbook.CreateSheet("Overview");

            sheet1.WriteToSheet(exportResult.Data.List, true, offset);
            sheet2.WriteToSheet(exportResult.Data.Overview, true);

            await using var memoryStream = new MemoryStream();
            workbook.Write(memoryStream);
            var result = memoryStream.ToArray();

            return result;
        }

    }
}