using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface;

public interface IMemberDashboardBLL
{
    Task<CustomerBasicInfo> GetCustomerBasicInfo(int custId);
    Task<IEnumerable<MemberLoginDevice>> GetDeviceListByCustId(int custId);
    Task<IEnumerable<CustomerLoginIp>> GetIpListByCustId(int custId);
    Task<int> SetFullName(int custId, string fullName);
}