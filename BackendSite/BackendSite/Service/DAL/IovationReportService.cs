using System.Data;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;

namespace BackendSite.Service.DAL
{
    public class IovationReportService
    {
        private readonly MySqlClient _reportDbClient;

        public IovationReportService(IEnumerable<MySqlClient> dbClient)
        {
            _reportDbClient = dbClient.FirstOrDefault(a => a.DataBase == DataBase.Report);
        }

        public async Task<IovationReportPageResult<IovationReportData>> GetListAsync(int currencyId, string deviceType, string deviceId, string userName, DateTime startTime, DateTime endTime, int pageNumber, int pageSize)
        {
            var p = new DynamicParameters();
            p.Add("_CurrencyId", currencyId);
            p.Add("_DeviceType", deviceType);
            p.Add("_UserName", userName);
            p.Add("_DeviceId", deviceId);
            p.Add("_StartTime", startTime);
            p.Add("_EndTime", endTime);
            p.Add("_PageSize", Math.Abs(pageSize));
            p.Add("_StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), DbType.Int32, ParameterDirection.Input);
            p.Add("_PageNumber", Math.Abs(pageNumber), DbType.Int32, ParameterDirection.InputOutput);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = await _reportDbClient.QueryAsync<IovationReportData>("Admin_IovationReport_Get", p);
            var data = new IovationReportPageResult<IovationReportData>
            {
                IovationReport = result,
                PageNumber = p.Get<int?>("_PageNumber") ?? 0,
                TotalPages = p.Get<int?>("_TotalPages") ?? 0,
                RowCount = p.Get<int?>("_RowCount") ?? 0
            };
            return data;
        }

        public async Task<IEnumerable<IovationReportData>> GetListWithoutPageAsync(int currencyId, string deviceType, string deviceId, string userName, DateTime startTime, DateTime endTime)
        {
            var p = new DynamicParameters();
            p.Add("_CurrencyId", currencyId);
            p.Add("_DeviceType", deviceType);
            p.Add("_UserName", userName);
            p.Add("_DeviceId", deviceId);
            p.Add("_StartTime", startTime);
            p.Add("_EndTime", endTime);
            var result = await _reportDbClient.QueryAsync<IovationReportData>("Admin_IovationReportExport_Get", p);
            return result;
        }
    }
}