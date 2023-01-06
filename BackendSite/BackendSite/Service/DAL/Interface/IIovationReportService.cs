using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface;

public interface IIovationReportService
{
    Task<IovationReportPageResult<IovationReportData>> GetListAsync(int currencyId, string deviceType, string deviceId, string userName, DateTime startTime, DateTime endTime, int pageNumber, int pageSize);
    Task<IEnumerable<IovationReportData>> GetListWithoutPageAsync(int currencyId, string deviceType, string deviceId, string userName, DateTime startTime, DateTime endTime);
}