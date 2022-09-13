using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface;

public interface ICustomerService
{
    Task<CustomerDto> GetCustomerBasicInfo(int custId);
    Task<IEnumerable<CustomerLoginIp>> GetCustomerIpListByCustId(int custId);
    Task<int> SetFullName(int custId, string fullName);
    Task<int> SetEmail(int custId, string email);
    Task<int> SetBirthday(int custId, int year, int month, int date);
}