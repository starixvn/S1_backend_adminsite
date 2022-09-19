using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface;

public interface ICustomerService
{
    Task<CustomerDto> GetCustomerBasicInfoAsync(int custId);
    Task<IEnumerable<CustomerLoginIp>> GetCustomerIpListByCustIdAsync(int custId);
    Task<int> EditFullNameAsync(int custId, string fullName);
    Task<int> EditEmailAsync(int custId, string email);
    Task<int> SetPhoneNoAsync(int custId, string phoneNo);
    Task<int> EditBirthdayAsync(int custId, int year, int month, int date);
    Task<int> EditAddressAsync(int custId, string country, string state, string postCode, string city, string line1, string line2);
    Task<int> SwitchKycAsync(int custId, int status);
    Task<int> SwitchRiskAsync(int custId, int status);
    Task<int> SwitchPhoneVerifiedAsync(int custId, int status);
    Task<int> SwitchEmailVerifiedAsync(int custId, int status);
    Task<int> ResetPasswordAsync(int custId, string newPassword);
    Task<int> SetCustStatusAsync(int custId, int status, int taskId);
    Task<int> SwitchRebatePayoutAsync(int custId, int status);
    Task<int> ResetBankInfoAsync(int custId);
    Task<int> ResetCryptoInfoAsync(int custId);
    Task<int> ResetEWalletAsync(int custId);
}