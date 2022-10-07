using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface;

public interface ICustomerService
{
    Task<GetMemberListData> GetList(int siteId, string currencyList, string statusList, string userName, IEnumerable<int> tags, string accountName, string tpUserName, string loginIp, string email, string phoneNo, string bankAccountNumber, string affCode, int transType, string transId, int shieldLevel, int iovationLevel, string deviceId, int dateType, DateTime startDate, DateTime endDate, int pageNumber, int pageSize);
    Task<IEnumerable<MemberExportDataDto>> GetExportData(int siteId, string currencyList, string statusList, string userName, IEnumerable<int> tags, string accountName, string tpUserName, string loginIp, string email, string phoneNo, string bankAccountNumber, string affCode, int transType, string transId, int shieldLevel, int iovationLevel, string deviceId, int dateType, DateTime startDate, DateTime endDate);
    Task<CustomerDto> GetCustomerBasicInfoAsync(int custId);
    Task<IEnumerable<CustomerTag>> GetCustomerTagsAsync(string tagList);
    Task<IEnumerable<CustomerLoginIp>> GetCustomerIpListByCustIdAsync(int custId);
    Task<CustLineProfileModel> GetCustLineProfileAsync(int custId, string lineUserId);
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
    Task<CustomerBalanceInfo> GetCustomerBalanceInfoAsync(int custId);
    Task<int> DeleteLineProfileAsync(int custId);
}