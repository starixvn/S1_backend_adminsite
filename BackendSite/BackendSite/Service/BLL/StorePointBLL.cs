using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Store.StorePoint;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NPOI.XSSF.UserModel;

namespace BackendSite.Service.BLL
{
    public class StorePointBLL
    {
        private readonly CustInfoBLL _custInfoBll;
        private readonly CourierAdapter _courierAdapter;
        private readonly ILogger<StorePointBLL> _logger;
        private readonly string _storeSiteUrl;

        public StorePointBLL(IServiceProvider provider)
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            _storeSiteUrl = configuration.GetValue<string>("StoreService");
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _logger = provider.GetRequiredService<ILogger<StorePointBLL>>();
        }
        
        public Task<ApiListResult<object>> GetMemberListAsync(GetMemberListRequest req)
        {
            var url = _storeSiteUrl + "/api/Member/GetList";
            var result = _courierAdapter.PostAsync<ApiListResult<object>, GetMemberListRequest>(url, req);

            return result;
        }
        
        public Task<ApiListResult<object>> GetMemberRewardPointLogAsync(GetMemberRewardPointLogRequest req)
        {
            var url = _storeSiteUrl + "/api/Member/GetRewardPointLog";
            var result = _courierAdapter.PostAsync<ApiListResult<object>, GetMemberRewardPointLogRequest>(url, req);

            return result;
        }
        
        public Task<ApiResult<object>> ManualRewardPointAsync(CreateManualRewardPointInfoRequest req, string @operator)
        {
            var param = new CreateManualRewardPointInfoParams(req, @operator);
            var url = _storeSiteUrl + "/api/Member/ManualRewardPoint";
            var result = _courierAdapter.MyPostAsync<object, CreateManualRewardPointInfoParams>(url, param);

            return result;
        }
        
        public async Task<ApiResult> BatchManualRewardPointAsync(CreateManualRewardPointInfoBatchRequest req, string @operator)
        {
            // STEP 1: 讀取 Excel 內容
            var importResult = ImportExcel(req.File);
            if (importResult.FailedRows.Any())
            {
                return new ApiResult<object>
                {
                    Message =
                        $"import file {req.File.FileName} rows: {string.Join(',', importResult.FailedRows)} was failed, please try again",
                    ErrorCode = (int) ErrorCodeEnum.Fail
                };
            }

            // STEP 2: 塞入 CustId
            var list = importResult.Datas.ToList();
            var userNames = list.Select(x => x.UserName);
            var custList = await _custInfoBll.GetCustInfosAsync(req.SiteId, userNames);
            list.ForEach(x =>
            {
                var cust = custList
                    .FirstOrDefault(y => y.UserName.Equals(x.UserName, StringComparison.OrdinalIgnoreCase));

                x.CustId = cust?.CustId ?? 0;
                x.CurrencyId = cust?.CurrencyId ?? 0;
            });

            if (list.Any(x => x.CustId == 0))
            {
                var users = list
                    .Where(x => x.CustId == 0)
                    .Select(x => x.UserName);
                
                return new ApiResult<object>
                {
                    Message = $"MemberIds: {string.Join(',', users)} not exists, please try again",
                    ErrorCode = (int) ErrorCodeEnum.Fail
                };
            }
            
            // STEP 3: 執行
            var param = new CreateManualRewardPointInfoBatchParams(req.SiteId, list, @operator);
            var url = _storeSiteUrl + "/api/Member/BatchManualRewardPoint";
            var result = await _courierAdapter.MyPostAsync<object, CreateManualRewardPointInfoBatchParams>(url, param);

            return new ApiResult(result.ErrorCode, result.Message);;
        }
        
        public byte[] GetSampleExcelFile()
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("AddPoint");
            var categoryList = GetManualRewardPointCategory().Values.ToArray();

            sheet.CreateHeaders<ManualRewardPointItem>(freeze: true);
            sheet.SetDropDownList(categoryList, 1, 10, 1, 1);

            using MemoryStream memoryStream = new MemoryStream();
            workbook.Write(memoryStream);
            var result = memoryStream.ToArray();
            
            return result;
        }

        #region Private Methods

        private IDictionary<int, string> GetManualRewardPointCategory()
        {
            var result = ((ManualRewardPointCategoryEnum[]) Enum
                    .GetValues(typeof(ManualRewardPointCategoryEnum)))
                .ToDictionary(x => ((int) x), x => x.ToString());

            return result;
        }
        
        private IDictionary<string, int> GetManualRewardPointCategoryMapping()
        {
            var result = ((ManualRewardPointCategoryEnum[]) Enum
                    .GetValues(typeof(ManualRewardPointCategoryEnum)))
                .ToDictionary(x => x.ToString(), x => ((int) x));

            return result;
        }
        
        private ImportManualRewardPointInfoResult ImportExcel(IFormFile file)
        {
            using var fileStream = file.OpenReadStream();
            var workbook = new XSSFWorkbook(fileStream);
            var sheet = workbook.GetSheetAt(0);
            var firstRowNum = sheet.FirstRowNum + 1; // skip header
            var lastRowNum = sheet.LastRowNum;
            var list = new List<ManualRewardPointInfo>();
            var failedRows = new List<int>();
            var categoryMapping = GetManualRewardPointCategoryMapping();

            for (int i = firstRowNum; i <= lastRowNum; i++)
            {
                try
                {
                    var row = sheet.GetRow(i);
                    var item = new ManualRewardPointInfo
                    {
                        UserName = row.GetCell(0).CellItemToString(),
                        CategoryId = categoryMapping[row.GetCell(1).CellItemToString()],
                        Point = row.GetCell(2).CellItemToDecimal(),
                        Memo = row.GetCell(3).CellItemToString()
                    };
                    list.Add(item);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{file.FileName} row: {i} import error", ex);
                    failedRows.Add(i + 1);
                }
            }

            return new ImportManualRewardPointInfoResult
            {
                Datas = list,
                FailedRows = failedRows
            };
        }

        #endregion
    }
}