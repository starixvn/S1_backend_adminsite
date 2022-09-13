using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface;

public interface IMemberDashboardBLL
{
    Task<CustomerBasicInfo> GetCustomerBasicInfo(int custId);
    Task<IEnumerable<MemberLoginDevice>> GetDeviceListByCustId(int custId);
    Task<IEnumerable<CustomerLoginIp>> GetIpListByCustId(int custId);
    Task<int> SetFullName(int custId, string fullName);
    Task<int> SetEmail(int custId, string email);
    Task<int> SetPhoneNo(int custId, string phoneNo);
    Task<int> SetBirthday(int custId, int year, int month, int date);
}