using BackendSite.Service.Model;

namespace BackendSite.Service.BLL;

public interface IMemberQuery
{
    Task<GetMemberListResponse> GetListAsync(GetTempMemberListRequest request);
    Task<IEnumerable<MemberExportData>> GetExportDataAsync(ExportMemberRequest request);
    Task<CustomerBasicInfo> GetCustomerBasicInfoAsync(AuthData authData, int custId);
    Task<IEnumerable<MemberLoginDevice>> GetDeviceListByCustIdAsync(int custId);
    Task<IEnumerable<CustomerLoginIp>> GetIpListByCustIdAsync(int custId);
    Task<GetLoginLogResponse> GetLoginLogAsync(int custId, DateTime startDate, DateTime endDate, int pageSize, int pageNumber);
}