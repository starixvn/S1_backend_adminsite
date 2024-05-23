using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL.Interface;

public interface IMemberCommand
{
    Task<int> AddCustTagAsync(int custId, int tagId, string @operator);
    Task<int> RemoveCustTagAsync(int custId, int tagId, string @operator);
    Task<IEnumerable<SimilarAccountInfo>> GetDuplicateAccountInfoAsync(int siteId, int custId, string accountName);
    Task<bool> EditFullNameAsync(int custId, string fullName, string @operator);
    Task<int> EditEmailAsync(int custId, string email, string @operator);
    Task<int> EditPhoneNoAsync(int custId, string phoneNo, string @operator);
    Task<int> EditBirthdayAsync(int custId, int year, int month, int date, string @operator);
    Task<int> EditAddressAsync(int custId, string country, string state, string postCode, string city, string line1, string line2, string @operator);
    Task<int> SwitchKycAsync(int custId, int status, string @operator);
    Task<int> SwitchRiskAsync(int custId, int status, string @operator);
    Task<int> SwitchPhoneVerifiedAsync(int custId, int status, string @operator);
    Task<int> SwitchEmailVerifiedAsync(int custId, int status, string @operator);
    Task<string> ResetPasswordAsync(int custId, string @operator);
    Task<int> SetCustStatusAsync(int custId, CustomerStatusEnum status, string reason, string @operator);
    Task<int> SwitchRebatePayoutAsync(int custId, int status, string @operator);
    Task<int> ResetBankInfoAsync(int custId, string @operator);
    Task<int> ResetCryptoInfoAsync(int custId, string @operator);
    Task<int> ResetEWalletAsync(int custId, string @operator);
    Task<ApiResult> SetVerificationRewardAsync(int custId, OTPTypeEnum otpType, string @operator);
    Task<int> LineUnbind(int custId, string @operator);
    Task<int> DeleteOnUserAsync(int custId, string @operator);
    Task<bool> SetCustPromotionWhiteListAsync(int custId, bool ipEnable, bool deviceEnable, bool realNameEnable, string @operator);
    Task<int> RegisterMemberAsync(int siteId, string userName, string userPwd, string email, int currencyId,
        MemberTypeEnum memberType, string areaCode, string phoneNo, string affCode, string signupUrl,string realName,
        string @operator);
    Task<bool> SetCustTpGameTypeSettingAsync(int custId, CustTpGameTypeSettingGetType setDataType, IEnumerable<int> tpGameTypes, string @operator);
}