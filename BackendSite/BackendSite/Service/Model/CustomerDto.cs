using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
namespace BackendSite.Service.Model;

public class GetTempMemberListRequest : ApiRequestPageModel
{
    public int SiteId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public int DateType { get; set; }
    public IEnumerable<int> CurrencyId { get; set; }
    public IEnumerable<int> Status { get; set; }
    public int Identity { get; set; }
    public string UserName { get; set; }
    public string TpUserName { get; set; }
    public string AffiliateName { get; set; }
    public string LoginIp { get; set; }
    public string Email { get; set; }
    public string PhoneNo { get; set; }
    public string AccountName { get; set; }
    public string BankAccountNumber { get; set; }
    public string EWalletAccountNumber { get; set; }
    public string CryptoAddress { get; set; }
    public string AffiliateCode { get; set; }
    public IEnumerable<int> Tags { get; set; }
    public IEnumerable<int> VipLevel { get; set; }
    public string TransId { get; set; }
    public int ShieldLevel { get; set; }
    public int IovationLevel { get; set; }
    public string DeviceId { get; set; }
    public IEnumerable<MemberTypeEnum> UserTypes { get; set; }
    public bool ExcludeTag { get; set; }
}

public class MemberListDto
{
    public string SiteName { get; set; }
    public int CurrencyId { get; set; }
    public string IsoCode { get; set; }
    public int CustId { get; set; }
    public string UserName { get; set; }
    public string SabaId { get; set; }
    public string AffCode { get; set; }
    public string TagList { get; set; }
    public int UserLevel { get; set; }
    public string Agent { get; set; }
    public int Status { get; set; }
    public bool IsTest { get; set; }
    public bool IsKyc { get; set; }
    public bool IsRisk { get; set; }
    public string IP { get; set; }
    public int DepositCnt { get; set; }
    public int WithdrawCnt { get; set; }
    public int Identity { get; set; }
    public int UserType { get; set; }
    public string AffiliateName { get; set; }
    public string LevelName { get; set; }
    public DateTime RegTime { get; set; }
    public DateTime? ReloadTime { get; set; }
    public DateTime? LastTagUpdateTime { get; set; }
    public DateTime? LastStatusUpdateTime { get; set; }
    public decimal Balance { get; set; }
    public decimal Bonus { get; set; }
    public string DeviceScore { get; set; }
    public string ShieldId { get; set; }
}

public class GetMemberListPageData<T>
{
    public IEnumerable<T> Data { get; set; }
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public int RowCount { get; set; }
}

public class MemberList
{
    public string SiteName { get; set; }
    public int CurrencyId { get; set; }
    public string IsoCode { get; set; }
    public int CustId { get; set; }
    public string UserName { get; set; }
    public string SabaId { get; set; }
    public string AffiliateCode { get; set; }
    public List<int> TagList { get; set; }
    public int PointLevel { get; set; }
    public int UserType { get; set; }
    public int Identity { get; set; }
    public string AffiliateName { get; set; }
    public string Agent { get; set; }
    public int Status { get; set; }
    public bool IsTest { get; set; }
    public string IP { get; set; }
    public string LevelName { get; set; }
    public DateTimeOffset RegTime { get; set; }
    public DateTimeOffset? LastActiveTime { get; set; }
    public DateTimeOffset? LastTagUpdateTime { get; set; }
    public DateTimeOffset? LastStatusUpdateTime { get; set; }
    public decimal Balance { get; set; }
    public decimal Bonus { get; set; }
    public string DeviceScore { get; set; }
    public string DeviceId { get; set; }
}

public class ExportMemberRequest
{
    public int SiteId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public int DateType { get; set; }
    public IEnumerable<int> CurrencyId { get; set; }
    public IEnumerable<int> Status { get; set; }
    public int Identity { get; set; }
    public string UserName { get; set; }
    public string TpUserName { get; set; }
    public string AffiliateName { get; set; }
    public string LoginIp { get; set; }
    public string Email { get; set; }
    public string PhoneNo { get; set; }
    public string AccountName { get; set; }
    public string BankAccountNumber { get; set; }
    public string EWalletAccountNumber { get; set; }
    public string CryptoAddress { get; set; }
    public string AffiliateCode { get; set; }
    public IEnumerable<int> Tags { get; set; }
    public IEnumerable<int> VipLevel { get; set; }
    public string TransId { get; set; }
    public int ShieldLevel { get; set; }
    public int IovationLevel { get; set; }
    public string DeviceId { get; set; }
    public IEnumerable<MemberTypeEnum> UserTypes { get; set; }
    public bool ExportTransferWallet { get; set; } = false;
    public bool ExcludeTag { get; set; }
}


public class MemberExportDataDto
{
    public string SiteName { get; set; }
    public string IsoCode { get; set; }
    public int CustId { get; set; }
    public string UserName { get; set; }
    public string SabaId { get; set; }
    public string AffiliateId { get; set; }
    public string AffiliateCode { get; set; }
    public string SignupUrl { get; set; }
    public int BirthDate { get; set; }
    public int BirthMonth { get; set; }
    public int BirthYear { get; set; }
    public string Email { get; set; }
    public string RealName { get; set; }
    public string CountryCode { get; set; }
    public string AreaCode { get; set; }
    public string PhoneNo { get; set; }
    public string Agent { get; set; }
    public int Status { get; set; }
    public bool IsTest { get; set; }
    public bool IsKyc { get; set; }
    public bool IsRisk { get; set; }
    public string LastActivityIp { get; set; }
    public string LevelName { get; set; }
    public DateTime RegTime { get; set; }
    public decimal SeamlessWallet { get; set; }
    public decimal BonusWallet { get; set; }
    public string TagList { get; set; }

    public MemberTypeEnum UserType { get; set; }
}

public class MemberExportData
{
    public string SiteName { get; set; }
    public string UserName { get; set; }
    public int CustId { get; set; }
    public int BirthDate { get; set; }
    public int BirthMonth { get; set; }
    public int BirthYear { get; set; }
    public bool IsTest { get; set; }
    public bool IsKyc { get; set; }
    public bool IsRisk { get; set; }
    public decimal PTWallet { get; set; }
    public decimal SeamlessWallet { get; set; }
    public decimal BonusWallet { get; set; }
    public string Email { get; set; }
    public string RealName { get; set; }
    public string CountryCode { get; set; }
    public string AreaCode { get; set; }
    public string PhoneNo { get; set; }
    public string Currency { get; set; }
    public string LevelName { get; set; }
    public string Agent { get; set; }
    public string Status { get; set; }
    public DateTimeOffset RegTime { get; set; }
    public string SabaId { get; set; }
    public string SignupUrl { get; set; }
    public string AffiliateId { get; set; }
    public string AffiliateCode { get; set; }
    public string LastActivityIp { get; set; }
    public string Tag { get; set; }
    public MemberTypeEnum UserType { get; set; }
}

public class GetCustomerBasicInfoRequest
{
    public int CustId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
}

public class GetCustomerAssociateBankUserRequest
{
    public int CustId { get; set; }
}

public class CustomerDto
{
    public int SiteId { get; set; }
    public string SiteName { get; set; }
    public int CustId { get; set; }
    public int VipLevel { get; set; }
    public string LevelName { get; set; }
    public string UserName { get; set; }
    public string UserLang { get; set; }
    public string AccountName { get; set; }
    public string SabaId { get; set; }
    public bool PhoneVerified { get; set; }
    public bool EmailVerified { get; set; }
    public int ActStatus { get; set; }
    public int Status { get; set; }
    public int EnableRebate { get; set; }
    public int PointLevel { get; set; }
    public int CurrencyId { get; set; }
    public string CurrencyName { get; set; }
    public int Identity { get; set; }
    public string Agent { get; set; }
    /// <summary>
    /// 會員透過代理進來的推薦碼
    /// </summary>
    public string AffId { get; set; }
    /// <summary>
    /// 會員透過代理進來的推薦碼
    /// </summary>
    public string AffCode { get; set; }
    /// <summary>
    /// 會員名稱
    /// </summary>
    public string AffPlayerName { get; set; }
    /// <summary>
    /// 會員本身的代理推薦碼
    /// </summary>
    public string AffiliateCode { get; set; }
    public DateTime RegTime { get; set; }
    public DateTime? FirstDepositTime { get; set; }
    public DateTime LastLoginTime { get; set; }
    public DateTime? LastDepositTime { get; set; }
    public string BirthDay { get; set; }
    public string BirthMonth { get; set; }
    public string BirthYear { get; set; }
    public string Email { get; set; }
    public string AreaCode { get; set; }
    public string PhoneNo { get; set; }
    public bool IsRisk { get; set; }
    public bool IsKyc { get; set; }
    public bool IsTest { get; set; }
    public string TagList { get; set; }
    public string AddressCountry { get; set; }
    public string AddressState { get; set; }
    public string AddressPostCode { get; set; }
    public string AddressCity { get; set; }
    public string AddressLineFirst { get; set; }
    public string AddressLineSecond { get; set; }
    public DateTime? PhoneRewardTime { get; set; }
    public DateTime? EmailRewardTime { get; set; }
    public DateTime? PhoneVerifyTime { get; set; }
    public DateTime? EmailVerifyTime { get; set; }
    public int MemberType { get; set; }
    public int Exclusive { get; set; }
}

public class CustomerTag
{
    public int Id { get; set; }
    public string TagName { get; set; }
    public string Description { get; set; }
    public string TagColour { get; set; }
}

public class CustomerBasicInfo
{
    public int CustId { get; set; }
    public int VipLevel { get; set; }
    public string LevelName { get; set; }
    public string VipLogo { get; set; }
    public bool IsOnline { get; set; }
    public string UserName { get; set; }
    public string UserLang { get; set; }
    public string AccountName { get; set; }
    public string SabaId { get; set; }
    public bool PhoneVerified { get; set; }
    public bool EmailVerified { get; set; }
    public int ActStatus { get; set; }
    public bool IsLineBinding { get; set; }
    public CustLineProfileModel CustLineProfile { get; set; }
    public int Status { get; set; }
    public int EnableRebate { get; set; }
    public int DepositPointLevel { get; set; }
    public string Currency { get; set; }
    public int Identity { get; set; }
    public string AgentName { get; set; }
    public string SuperiorAffiliateId { get; set; }
    public string SuperiorAffiliateCode { get; set; }
    public DateTimeOffset RegisteredTime { get; set; }
    public DateTimeOffset? FirstDepositTime { get; set; }
    public DateTimeOffset LastLoginTime { get; set; }
    public DateTimeOffset? LastActiveTime { get; set; }
    public string BirthDay { get; set; }
    public string BirthMonth { get; set; }
    public string BirthYear { get; set; }
    public string Email { get; set; }
    public string MobileNumber { get; set; }
    public string Address { get; set; }
    public bool IsRisk { get; set; }
    public bool IsKyc { get; set; }
    public bool IsTest { get; set; }
    public IEnumerable<CustomerTag> TagList { get; set; }
    public AffInfo AffInfo { get; set; }
    public DateTimeOffset? PhoneRewardTime { get; set; }
    public DateTimeOffset? EmailRewardTime { get; set; }
    public DateTimeOffset? PhoneVerifyTime { get; set; }
    public DateTimeOffset? EmailVerifyTime { get; set; }
    public int MemberType { get; set; }
    public int Exclusive { get; set; }
}

public class AffInfo
{
    public string AffiliateName { get; set; }
    public string AffiliateCode { get; set; }
    public string RevenueName { get; set; }
    public int TotalPlayers { get; set; }
    public decimal RevenueShare { get; set; }
    public decimal TotalEarning { get; set; }
}

public class AffToken
{
    public string access_token { get; set; }
    public string token_type { get; set; }
    public int expires_in { get; set; }
}

public class GetCustPhotoIdRequest
{
    public int CustId { get; set; }
}

public class GetCustPhotoIdResponse
{
    public int Id { get; set; }
    public string PhotoUrl { get; set; }
}

public class UploadCustPhotoIdRequest
{
    public int CustId { get; set; }
    public IFormFile Photo { get; set; }
}

public class DeleteCustPhotoIdRequest
{
    public int CustId { get; set; }
    public int Id { get; set; }
}
public class GetVipLevelSettingRequest
{
    public int SiteId { get; set; }
}

public class GetVipLevelInfoRequest
{
    public int CustId { get; set; }
}

public class GetVipLevelLogRequest
{
    public int CustId { get; set; }
}

public class SetVipLevelRequest
{
    public int CustId { get; set; }
    public int VipLevel { get; set; }
}

public class SetVipLevelManuallyRequest
{
    public int CustId { get; set; }
    public int Manually { get; set; }
}

public class AutoVipDowngradeExclusionRequest
{
    public int CustId { get; set; }
    public bool Enable { get; set; }
}

public class AddTagRequest
{
    public int CustId { get; set; }
    public int TagId { get; set; }
}

public class RemoveTagRequest
{
    public int CustId { get; set; }
    public int TagId { get; set; }
}

public class LineUnbindRequest
{
    public int CustId { get; set; }
}

public class SetFullNameRequest
{
    public int CustId { get; set; }
    public string FullName { get; set; }
}
public class VerificationRequest
{
    public VerificationInfoType VerificationType { get; set; }
    public int CustId { get; set; }
    public string VerificationInfo { get; set; }
}
public class SetEmailRequest
{
    public int CustId { get; set; }
    public string Email { get; set; }
}

public class SetPhoneNoRequest
{
    public int CustId { get; set; }
    public string PhoneNo { get; set; }
}

public class SetBirthdayRequest
{
    public int CustId { get; set; }
    public int BirthDate { get; set; }
    public int BirthMonth { get; set; }
    public int BirthYear { get; set; }
}

public class SetMemberAddressRequest
{
    public int CustId { get; set; }
    public string Country { get; set; }
    public string State { get; set; }
    public string PostCode { get; set; }
    public string City { get; set; }
    public string Line1 { get; set; }
    public string Line2 { get; set; }
}

public class SetMemberKycStatusRequest
{
    public int CustId { get; set; }
    public int Status { get; set; }
}

public class SetMemberRiskStatusRequest
{
    public int CustId { get; set; }
    public int Status { get; set; }
}

public class SwitchMemberPhoneVerifiedRequest
{
    public int CustId { get; set; }
    public int Status { get; set; }
}

public class SwitchMemberEmailVerifiedRequest
{
    public int CustId { get; set; }
    public int Status { get; set; }
}

public class ResetMemberPasswordRequest
{
    public int CustId { get; set; }
}

public class LockMemberRequest
{
    public int CustId { get; set; }
    public LockCustomerStatusEnum Status { get; set; }
    public string Reason { get; set; }
}

public class KickOutMemberRequest
{
    public int CustId { get; set; }
    public string Reason { get; set; }
}

public class CloseMemberRequest
{
    public int CustId { get; set; }
    public string Reason { get; set; }
}

public class SwitchEnableRebateRequest
{
    public int CustId { get; set; }
    public EnableRebateEnum Status { get; set; }
}

public class ResetBankInfoRequest
{
    public int CustId { get; set; }
}

public class ResetCryptoInfoRequest
{
    public int CustId { get; set; }
}

public class ResetEwalletRequest
{
    public int CustId { get; set; }
}

public class PayoutVerificationRewardRequest
{
    public int CustId { get; set; }
    public OTPTypeEnum OTPType { get; set; }
}

public class GetIpListRequest
{
    public int CustId { get; set; }
}

public class MemberLoginDevice
{
    public string DeviceId { get; set; }
    public string Risk { get; set; }
    public int Score { get; set; }
    public int Frequency { get; set; }
    public int AssociateCount { get; set; }
    public string DeviceType { get; set; }
    public DateTimeOffset LastUpdateTime { get; set; }
}

public class MemberLoginDeviceFPS
{
    public string DeviceId { get; set; }
    public string RiskLevel { get; set; }
    public int Frequency { get; set; }
    public int AssociateCount { get; set; }
    public string DeviceType { get; set; }
    public DateTimeOffset LastUpdateTime { get; set; }
}


public class CustomerLoginIp
{
    public string LoginIp { get; set; }
    public int UserCount { get; set; }
}

public class CustomerAssociateBankInfo
{
    public int CustId { get; set; }
    public string UserName { get; set; }
    public string PaymentName { get; set; }
    public string AssociateNo { get; set; }
}

public class CustomerBalanceInfo
{
    public decimal Balance { get; set; }
    public decimal WithdrawalLimit { get; set; }
    public decimal TurnOver { get; set; }
    public decimal DepositSum { get; set; }
    public int DepositCnt { get; set; }
    public decimal WithdrawlSum { get; set; }
    public int WithdrawlCnt { get; set; }
    public decimal EffectiveBetSum { get; set; }
    public decimal BonusSum { get; set; }
    public int BonusCnt { get; set; }
    public decimal TotalAdjustment { get; set; }
    public decimal PaymentFee { get; set; }

    public decimal RemainingTurnOver
    {
        get
        {
            var remainingTurnOver = WithdrawalLimit - TurnOver;
            return remainingTurnOver > 0 ? remainingTurnOver : 0;
        }
    }

    public decimal EligibleWithdrawal
    {
        get
        {
            var eligibleWithdrawal = Balance - RemainingTurnOver;
            return eligibleWithdrawal < 0 ? 0 : eligibleWithdrawal;
        }
    }
    public decimal NetCash
    {
        get
        {
            return DepositSum - WithdrawlSum;
        }
    }
}

public class GetLoginLogRequest : ApiRequestPageModel
{
    public int CustId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
}

public class GetLoginLogResponse
{
    public IEnumerable<MemberLoginLog> LoginLog { get; set; }
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public int RowCount { get; set; }
}
public class MemberLoginLog
{
    public int Id { get; set; }
    public DateTimeOffset LoginTime { get; set; }
    public string LoginIp { get; set; }
    public string Location { get; set; }
    public string FromUrl { get; set; }
    public string UserAgent { get; set; }
}
public class LoginLogDto
{
    public int Id { get; set; }
    public DateTime LoginTime { get; set; }
    public string LoginIp { get; set; }
    public string Location { get; set; }
    public string FromUrl { get; set; }
    public string UserAgent { get; set; }
}

public class LoginLogPageData
{
    public IEnumerable<LoginLogDto> LoginLog { get; set; }
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public int RowCount { get; set; }
}

public class SimilarAccountInfo
{
    public string IsoCode { get; set; }
    public int CustId { get; set; }
    public string UserName { get; set; }
    public string AccountName { get; set; }
    public string TagList { get; set; }
}

public class CustTagList
{
    public int CustId { get; set; }

    public string StrTagList { get; set; }

    public IEnumerable<int> TagList { get; set; }
}

public class CustAccountNameInfo : CustTagList
{
    public string AccountName { get; set; }
}

public class CustRealNameChangeLog
{
    public string BeforeName { get; set; }
    public string AfterName { get; set; }
    public DateTimeOffset UpdateTime { get; set; }
    public string Operator { get; set; }
}

public class GetListByFileRequest
{
    public int SiteId { get; set; }
    public IFormFile File { get; set; }
    public string Memo { get; set; }
}
public class GetCustinfoByImportDataRequest
{
    public int SiteId { get; set; }
}
public class CustPromotionWhiteList
{
    public int CustId { get; set; }
    public int SiteId { get; set; }
    public string UserName { get; set; }
    public bool IpEnable { get; set; }
    public bool DeviceEnable { get; set; }
    public bool RealNameEnable { get; set; }
    public string Operator { get; set; }
}

public class SetCustPromotionWhiteListRequest
{
    public int CustId { get; set; }
    public bool IpEnable { get; set; }
    public bool DeviceEnable { get; set; }
    public bool RealNameEnable { get; set; }
}

public class PromotionReceiveFailLog
{
    public string BonusCode { get; set; }
    public string PromotionTypeName { get; set; }
    public DateTimeOffset CreateTime { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorReason { get; set; }
    public int SiteId { get; set; }
    public int CustId { get; set; }
    public int CurrencyId { get; set; }
}
public class RegisterCustomerRequest
{
    public int SiteId { get; set; }
    public string UserName { get; set; }
    public string Userpwd { get; set; }
    public MemberTypeEnum MemberType { get; set; }
    public string Email { get; set; }
    public int CurrencyId { get; set; }
    public string PhoneNo { get; set; }
    public string AreaCode { get; set; }
    public string AffCode { get; set; }

    public string RealName { get; set; }

}

public class GetTpGameTypeSettingRequest
{
    public int CustId { get; set; }
    public IEnumerable<CustTpGameTypeSettingGetType> GetDataTypes { get; set; }
}

public class CustTpGameTypeSettingForFrontendUse
{
    public int TpId { get; set; }
    public string TpName { get; set; }
    public int GameType { get; set; }
    public string GameTypeName { get; set; }
}

public class CustTpGameTypeSettingDto : CustTpGameTypeSettingForFrontendUse
{
    public bool NoShow { get; set; }
    public bool NoPlay { get; set; }
}


public class SetTpGameTypeSettingRequest
{
    public int CustId { get; set; }
    public CustTpGameTypeSettingGetType SetDataType { get; set; }
    public IEnumerable<int> TpGameTypes { get; set; }
}

public class ResetOtpTimesRequest
{
    public int CustId { get; set; }
}

