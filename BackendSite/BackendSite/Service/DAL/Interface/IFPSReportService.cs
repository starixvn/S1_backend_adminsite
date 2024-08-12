using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface
{
    public interface IFPSReportService
    {
        Task<FPSReportPageResult<FPSReportData>> GetFPSListAsync(int currencyId, string deviceType, string deviceId, string userName, DateTime startTime, DateTime endTime, int pageNumber, int pageSize);
        Task<IEnumerable<FPSReportData>> GetFPSListWithoutPageAsync(int currencyId, string deviceType, string deviceId, string userName, DateTime startTime, DateTime endTime);
    }
}