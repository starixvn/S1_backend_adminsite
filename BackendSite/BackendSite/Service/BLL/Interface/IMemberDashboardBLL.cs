using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface;

public interface IMemberDashboardBLL
{
    Task<GetMemberListResponse> GetList(GetTempMemberListRequest request);
    Task<IEnumerable<MemberExportData>> GetExportData(ExportMemberRequest request);
    Task<CustomerBasicInfo> GetCustomerBasicInfoAsync(int custId);
    Task<IEnumerable<MemberLoginDevice>> GetDeviceListByCustIdAsync(int custId);
    Task<IEnumerable<CustomerLoginIp>> GetIpListByCustIdAsync(int custId);
    Task<int> EditFullNameAsync(int custId, string fullName);
    Task<int> EditEmailAsync(int custId, string email);
    Task<int> EditPhoneNoAsync(int custId, string phoneNo);
    Task<int> EditBirthdayAsync(int custId, int year, int month, int date);
    Task<int> EditAddressAsync(int custId, string country, string state, string postCode, string city, string line1, string line2);
    Task<int> SwitchKycAsync(int custId, int status);
    Task<int> SwitchRiskAsync(int custId, int status);
    Task<int> SwitchPhoneVerifiedAsync(int custId, int status);
    Task<int> SwitchEmailVerifiedAsync(int custId, int status);
    Task<string> ResetPasswordAsync(int custId);
    Task<int> SetCustStatusAsync(int custId, int status, string reason);
    Task<int> SwitchRebatePayoutAsync(int custId, int status);
    Task<int> ResetBankInfoAsync(int custId);
    Task<int> ResetCryptoInfoAsync(int custId);
    Task<int> ResetEWalletAsync(int custId);
    Task<ApiResult> SetVerificationRewardAsync(int custId, int otpType);
    Task<int> LineUnbind(int custId);
    Task<int> DeleteOnUserAsync(int custId);
}