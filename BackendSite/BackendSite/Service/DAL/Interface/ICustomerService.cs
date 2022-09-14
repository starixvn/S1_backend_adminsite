using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface;

public interface ICustomerService
{
    Task<CustomerDto> GetCustomerBasicInfo(int custId);
    Task<IEnumerable<CustomerLoginIp>> GetCustomerIpListByCustId(int custId);
    Task<int> SetFullName(int custId, string fullName);
    Task<int> SetEmail(int custId, string email);
    Task<int> SetPhoneNo(int custId, string phoneNo);
    Task<int> SetBirthday(int custId, int year, int month, int date);
    Task<int> SetAddress(int custId, string country, string state, string postCode, string city, string line1, string line2);
    Task<int> SetKycStatus(int custId, int status);
}