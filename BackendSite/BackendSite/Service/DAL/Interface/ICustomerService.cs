using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface;

public interface ICustomerService
{
    Task<CustomerDto> GetCustomerBasicInfo(int custId);
    Task<IEnumerable<CustomerLoginIp>> GetCustomerIpListByCustId(int custId);
    Task<int> EditFullName(int custId, string fullName);
    Task<int> EditEmail(int custId, string email);
    Task<int> SetPhoneNo(int custId, string phoneNo);
    Task<int> EditBirthday(int custId, int year, int month, int date);
    Task<int> EditAddress(int custId, string country, string state, string postCode, string city, string line1, string line2);
    Task<int> SwitchKyc(int custId, int status);
    Task<int> SwitchRisk(int custId, int status);
    Task<int> SwitchPhoneVerified(int custId, int status);
    Task<int> SwitchEmailVerified(int custId, int status);
    Task<int> ResetPassword(int custId, string newPassword);
}