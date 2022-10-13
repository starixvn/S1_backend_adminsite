using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface;

public interface ICustomerService
{
    Task<GetMemberListData> GetList(int siteId, string currencyList, string statusList, int identity, string userName, IEnumerable<int> tags, string vipList, string accountName, string tpUserName, string loginIp, string email, string phoneNo, string bankAccountNumber, string affCode, int transType, string transId, int shieldLevel, int iovationLevel, string deviceId, int dateType, DateTime startDate, DateTime endDate, int pageNumber, int pageSize);
    Task<IEnumerable<MemberExportDataDto>> GetExportData(int siteId, string currencyList, string statusList, string userName, IEnumerable<int> tags, string accountName, string tpUserName, string loginIp, string email, string phoneNo, string bankAccountNumber, string affCode, int transType, string transId, int shieldLevel, int iovationLevel, string deviceId, int dateType, DateTime startDate, DateTime endDate);
    Task<CustomerDto> GetCustomerBasicInfoAsync(int custId);
    Task<IEnumerable<CustomerTag>> GetCustomerTagsAsync(string tagList);
    Task<string> GetCustomerTagsAsync(int custId);
    Task<IEnumerable<CustomerLoginIp>> GetCustomerIpListByCustIdAsync(int custId);
    Task<CustLineProfileModel> GetCustLineProfileAsync(int custId, string lineUserId);
    Task<LoginLogPageData> GetLoginLogAsync(int custId, DateTime startDate, DateTime endDate, int pageSize, int pageNumber);
    Task<int> SetCustTag(int custid, List<int> tagList);
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
    Task<int> DeleteOnUserAsync(int custId);
    Task<int> SwitchRebatePayoutAsync(int custId, int status);
    Task<int> ResetBankInfoAsync(int custId);
    Task<int> ResetCryptoInfoAsync(int custId);
    Task<int> ResetEWalletAsync(int custId);
    Task<int> DeleteLineProfileAsync(int custId);
}