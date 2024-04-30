using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.DAL.Interface;

public interface IMemberService
{
    Task<GetMemberListPageData<MemberListDto>> GetList(int siteId, IEnumerable<int> currencyList,
        IEnumerable<int> statusList, int identity, string userName, IEnumerable<int> tags, IEnumerable<int> vipList,
        string accountName, string affiliateName, string tpUserName, string loginIp, string email, string phoneNo,
        string bankAccountNumber, string eWalletAccountNumber, string cryptoAddress, string affCode, int transType, string transId, int shieldLevel, int iovationLevel,
        string deviceId, IEnumerable<int> userTypes, int dateType, DateTime startDate, DateTime endDate, int pageNumber,
        int pageSize, bool excludeTag);
    Task<IEnumerable<MemberExportDataDto>> GetExportData(int siteId, IEnumerable<int> currencyList,
        IEnumerable<int> statusList, int identity, string userName, IEnumerable<int> tags, IEnumerable<int> vipList,
        string accountName, string affiliateName, string tpUserName, string loginIp, string email, string phoneNo,
        string bankAccountNumber, string eWalletAccountNumber, string cryptoAddress, string affCode, int transType, string transId, int shieldLevel, int iovationLevel,
        string deviceId, IEnumerable<int> userTypes, int dateType, DateTime startDate, DateTime endDate, bool excludeTag);
    Task<CustomerDto> GetCustomerBasicInfoAsync(int custId);
    Task<IEnumerable<CustomerTag>> GetCustomerTagsAsync(string tagList);
    Task<string> GetCustomerTagsAsync(int custId);
    Task<IEnumerable<CustomerLoginIp>> GetCustomerIpListByCustIdAsync(int custId);
    Task<CustLineProfileModel> GetCustLineProfileAsync(int custId, string lineUserId);
    Task<LoginLogPageData> GetLoginLogAsync(int custId, DateTime startDate, DateTime endDate, int pageSize, int pageNumber);
    Task<bool> IsExistsPhoneNo(int custId, int siteId, string areaCode, string phoneNo);
    Task<bool> IsExistsEmail(int custId, string email, int siteId);
    Task<int> SetCustTag(int custid, List<int> tagList);
    Task<IEnumerable<SimilarAccountInfo>> GetSimilarAccountNameAsync(int siteId, IEnumerable<string> accountNames);
    Task<bool> EditFullNameAsync(int custId, int siteId, string fullName, IEnumerable<SimilarAccountInfo> duplicateAccounts);
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
    Task<CustomerBalanceInfo> GetCustomerBalanceInfoAsync(int custId);
    Task<int> DeleteLineProfileAsync(int custId);
    Task<IEnumerable<CustomerAssociateBankInfo>> GetCustomerAssociateBankUserAsync(int custId);
    Task<IEnumerable<CustRealNameChangeLog>> GetCustRealNameChangeLogAsync(int custId);
    Task<CustPromotionWhiteList> GetCustPromotionWhiteListAsync(int custId);
    Task<int> SetCustPromotionWhiteListAsync(CustPromotionWhiteList input);
    Task<IEnumerable<PromotionReceiveFailLog>> GetPromotionReceiveFailLogAsync(int custId, DateTime startTime, DateTime endTime);
    Task<(int IsSuccess, int CustId)> RegisterMemberAsync(int siteId, string userName, string userPwd, string email, int currencyId,
        string userLang, MemberTypeEnum memberType, string areaCode, string phoneNo, string affCode, string signupUrl,string realName);
    Task<IEnumerable<CustTpGameTypeSettingDto>> GetCustTpGameTypeSettingAsync(int custId);
    Task<bool> SetCustTpGameTypeSettingAsync(int custId, IEnumerable<int> deleteTpGameTypes, IEnumerable<int> addTpGameTypes, CustTpGameTypeSettingTypeEnum type);
    Task<int> SwitchEnableTurnoverRewardPointAsync(int custId, int status);
    Task<CustomerForRisk> GetCustId(int? custId, string userName, int? siteId);
}