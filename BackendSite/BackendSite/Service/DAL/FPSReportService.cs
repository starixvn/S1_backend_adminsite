using System.Data;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class FPSReportService : IFPSReportService
    {
        private readonly MySqlClient _reportDbClient;

        public FPSReportService(IEnumerable<MySqlClient> dbClient)
        {
            _reportDbClient = dbClient.FirstOrDefault(a => a.DataBase == DataBase.Report);
        }
        public async Task<FPSReportPageResult<FPSReportData>> GetFPSListAsync(IEnumerable<int> siteIds,IEnumerable<int> currencyIds, string deviceType, string deviceId, string userName, DateTime startTime,
            DateTime endTime, int pageNumber, int pageSize)
        {

            var p = new DynamicParameters();
            p.Add("_SiteList", JsonConvert.SerializeObject(siteIds));
            p.Add("_CurrencyList", JsonConvert.SerializeObject(currencyIds));
            p.Add("_DeviceType", deviceType);
            p.Add("_UserName", userName);
            p.Add("_DeviceId", deviceId);
            p.Add("_StartTime", startTime);
            p.Add("_EndTime", endTime);
            p.Add("_PageSize", Math.Abs(pageSize));
            p.Add("_StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), DbType.Int32, ParameterDirection.Input);
            p.Add("_PageNumber", Math.Abs(pageNumber), DbType.Int32, ParameterDirection.Input);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = await _reportDbClient.QueryAsync<FPSReportData>("Admin_FPSReport_Get", p);
            var data = new FPSReportPageResult<FPSReportData>
            {
                FPSDataReport = result,
                TotalPages = p.Get<int?>("_TotalPages") ?? 0,
                RowCount = p.Get<int?>("_RowCount") ?? 0
            };
            return data;
        }

        public async Task<IEnumerable<FPSReportData>> GetFPSListWithoutPageAsync(IEnumerable<int> siteIds,IEnumerable<int> currencyIds, string deviceType, string deviceId, string userName, DateTime startTime,
            DateTime endTime)
        {
            var p = new DynamicParameters();
            p.Add("_SiteList", JsonConvert.SerializeObject(siteIds));
            p.Add("_CurrencyList", JsonConvert.SerializeObject(currencyIds));
            p.Add("_DeviceType", deviceType);
            p.Add("_UserName", userName);
            p.Add("_DeviceId", deviceId);
            p.Add("_StartTime", startTime);
            p.Add("_EndTime", endTime);
            var result = await _reportDbClient.QueryAsync<FPSReportData>("Admin_FPSReportExport_Get", p);
            return result;
        }
    }
}