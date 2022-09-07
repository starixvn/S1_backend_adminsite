using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface;

public interface IMemberBLL
{
    Task<CustomerBasicInfo> GetCustomerBasicInfo(int custId);
    Task<IEnumerable<MemberLoginDevice>> GetDeviceListByCustId(int custId);
    Task<IEnumerable<CustomerLoginIp>> GetIpListByCustId(int custId);
}