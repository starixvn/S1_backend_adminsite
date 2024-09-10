using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface
{
    public interface IFPSReportBLL
    {
        Task<FPSReportPageResult<FPSReport>> GetFPSListAsync(FPSReportGetListRequest request);
        Task<IEnumerable<FPSReport>> GetFPSListWithoutPageAsync(FPSReportExportRequest request);
        Task<IEnumerable<DeviceUser>> GetFPSAssociateAccount(string deviceId);
        Task<IEnumerable<AssociatedDevice>> GetFPSAssociateDevices(int custId);
    }
}