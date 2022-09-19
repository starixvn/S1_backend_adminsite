using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface;

public interface IMemberDashboardBLL
{
    Task<CustomerBasicInfo> GetCustomerBasicInfo(int custId);
    Task<IEnumerable<MemberLoginDevice>> GetDeviceListByCustId(int custId);
    Task<IEnumerable<CustomerLoginIp>> GetIpListByCustId(int custId);
    Task<int> EditFullName(int custId, string fullName);
    Task<int> EditEmail(int custId, string email);
    Task<int> EditPhoneNo(int custId, string phoneNo);
    Task<int> EditBirthday(int custId, int year, int month, int date);
    Task<int> EditAddress(int custId, string country, string state, string postCode, string city, string line1, string line2);
    Task<int> SwitchKyc(int custId, int status);
    Task<int> SwitchRisk(int custId, int status);
    Task<int> SwitchPhoneVerified(int custId, int status);
    Task<int> SwitchEmailVerified(int custId, int status);
    Task<string> ResetPassword(int custId);
    Task<int> SetCustStatus(int custId, int status, string reason);
    Task<int> SwitchEnableRebateAsync(int custId, int status);
    Task<int> ResetBankInfoAsync(int custId);
    Task<int> ResetCryptoInfoAsync(int custId);
}