using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface
{
    public interface IFPSReportService
    {
        Task<FPSReportPageResult<FPSReportData>> GetFPSListAsync(IEnumerable<int> siteIds,IEnumerable<int> currencyIds, string deviceType, string deviceId, string userName, DateTime startTime, DateTime endTime, int pageNumber, int pageSize);
        Task<IEnumerable<FPSReportData>> GetFPSListWithoutPageAsync(IEnumerable<int> siteIds,IEnumerable<int> currencyIds, string deviceType, string deviceId, string userName, DateTime startTime, DateTime endTime);
    }
}