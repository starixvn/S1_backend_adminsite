using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface;

public interface IMemberCommand
{
    Task<int> AddCustTagAsync(int custId, int tagId, string @operator);
    Task<int> RemoveCustTagAsync(int custId, int tagId, string @operator);
    Task<int> EditFullNameAsync(int custId, string fullName, string @operator);
    Task<int> EditEmailAsync(int custId, string email, string @operator);
    Task<int> EditPhoneNoAsync(int custId, string phoneNo, string @operator);
    Task<int> EditBirthdayAsync(int custId, int year, int month, int date, string @operator);
    Task<int> EditAddressAsync(int custId, string country, string state, string postCode, string city, string line1, string line2, string @operator);
    Task<int> SwitchKycAsync(int custId, int status, string @operator);
    Task<int> SwitchRiskAsync(int custId, int status, string @operator);
    Task<int> SwitchPhoneVerifiedAsync(int custId, int status, string @operator);
    Task<int> SwitchEmailVerifiedAsync(int custId, int status, string @operator);
    Task<string> ResetPasswordAsync(int custId);
    Task<int> SetCustStatusAsync(int custId, int status, string reason);
    Task<int> SwitchRebatePayoutAsync(int custId, int status);
    Task<int> ResetBankInfoAsync(int custId);
    Task<int> ResetCryptoInfoAsync(int custId);
    Task<int> ResetEWalletAsync(int custId);
    Task<ApiResult> SetVerificationRewardAsync(int custId, int otpType);
    Task<int> LineUnbind(int custId, string @operator);
    Task<int> DeleteOnUserAsync(int custId);
}