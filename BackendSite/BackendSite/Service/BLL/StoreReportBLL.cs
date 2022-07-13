using System;
using System.IO;
using System.Threading.Tasks;
using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Store.Report;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            var configuration = provider.GetRequiredService<IConfiguration>();
            _logger = provider.GetRequiredService<ILogger<StoreReportBLL>>();
            _storeSiteUrl = configuration.GetValue<string>("StoreService");
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
        }
        
        public Task<ApiResult<object>> GetDepositPointReportAsync(GetDepositPointReportRequest req)
        {
            var url = _storeSiteUrl + "/api/Report/GetDepositPointReport";
            var result = _courierAdapter.PostAsync<ApiResult<object>, GetDepositPointReportRequest>(url, req);

            return result;
        }
        
        public async Task<byte[]> ExportDepositPointReportAsync(ExportDepositPointReportRequest req)
        {
            var url = _storeSiteUrl + "/api/Report/ExportDepositPointReport";
            var exportResult =
                await _courierAdapter.PostAsync<ApiResult<ExportDepositPointReportResult>, ExportDepositPointReportRequest>(url, req);

            if (exportResult.ErrorCode != (int) ErrorCodeEnum.Success)
            {
                _logger.LogError($"StoreService ExportDepositPointReport Failed. ErrorCode: {exportResult.ErrorCode}");
                throw new AdminException("ExportDepositPointReport Failed.");
            }

            var workbook = new XSSFWorkbook();
            var sheet1 = workbook.CreateSheet("DepositPointReport");
            var sheet2 = workbook.CreateSheet("Overview");

            sheet1.WriteToSheet(exportResult.Data.List, true);
            sheet2.WriteToSheet(exportResult.Data.Overview, true);
            
            await using var memoryStream = new MemoryStream();
            workbook.Write(memoryStream);
            var result = memoryStream.ToArray();

            return result;
        }
        
    }
}