using System.Security.Cryptography;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model;

public class GetCustomerBasicInfoRequest
{
    public int CustId { get; set; }
}

public class CustomerDto
{
    public int CustId { get; set; }
    public int VipLevel { get; set; }
    public string UserName { get; set; }
    public string AccountName { get; set; }
    public bool PhoneVerified { get; set; }
    public bool EmailVerified { get; set; }
    public int ActStatus { get; set; }
    public int Status { get; set; }
    public int PointLevel { get; set; }
    public int CurrencyId { get; set; }
    public string CurrencyName { get; set; }
    public int Identity { get; set; }
    public string AffiliatesName { get; set; }
    public string AffiliatesCode { get; set; }
    public DateTimeOffset RegTime { get; set; }
    public DateTimeOffset? LineBindingTime { get; set; }
    public DateTimeOffset? FirstDepositTime { get; set; }
    public DateTimeOffset LastLoginTime { get; set; }
    public DateTimeOffset? LastDepositTime { get; set; }
    public string BirthDay { get; set; }
    public string BirthMonth { get; set; }
    public string BirthYear { get; set; }
    public string Email { get; set; }
    public string AreaCode { get; set; }
    public string PhoneNo { get; set; }
    public int? DeviceScore { set; get; }
    public bool IsRisk { get; set; }
    public string Iovation { get; set; }
    public bool IsKyc { get; set; }
    public bool IsTest { get; set; }
    public string TagList { get; set; }
    public string AddressCountry { get; set; }
    public string AddressState { get; set; }
    public string AddressPostCode { get; set; }
    public string AddressCity { get; set; }
    public string AddressLineFirst { get; set; }
    public string AddressLineSecond { get; set; }
}

public class CustomerBasicInfo
{
    public int CustId { get; set; }
    public int VipLevel { get; set; }
    public bool IsOnline { get; set; }
    public string UserName { get; set; }
    public string AccountName { get; set; }
    public bool PhoneVerified { get; set; }
    public bool EmailVerified { get; set; }
    public int ActStatus { get; set; }
    public bool IsLineBinding { get; set; }
    public int Status { get; set; }
    public int DepositPointLevel { get; set; }
    public string Currency { get; set; }
    public int Identity { get; set; }
    public string AffiliatesName { get; set; }
    public string AffiliatesCode { get; set; }
    public DateTimeOffset RegisteredTime { get; set; }
    public DateTimeOffset? LineBindingTime { get; set; }
    public DateTimeOffset? FirstDepositTime { get; set; }
    public DateTimeOffset LastLoginTime { get; set; }
    public DateTimeOffset? LastActiveTime { get; set; }
    public string BirthDay { get; set; }
    public string BirthMonth { get; set; }
    public string BirthYear { get; set; }
    public string Email { get; set; }
    public string MobileNumber { get; set; }
    public string Address { get; set; }
    public int? DeviceScore { set; get; }
    public bool IsRisk { get; set; }
    public string Iovation { get; set; }
    public bool IsKyc { get; set; }
    public bool IsTest { get; set; }
    public List<int> TagList { get; set; }
}

public class GetCustInfoWithBalanceRequest
{
    public int SiteId { get; set; }
    public string UserName { get; set; }
    public string SabaId { get; set; }
}

public class GetCustInfoWithBalanceResult
{
    public int SiteId { get; set; }
    public string SiteName { get; set; }
    public int CustId { get; set; }
    public string UserName { get; set; }
    public int CurrencyId { get; set; }
    public CustomerStatusEnum Status { get; set; }
    public decimal Balance { get; set; }
}
        
public class GetCustInfoWithTpBalanceResult : GetCustInfoWithBalanceResult
{
    public IEnumerable<TpWallet> TpWalletList { get; set; }
}

public class UploadCustomerIdPhotoRequest
{
    public int CustId { get; set; }
    public IFormFile Photo { get; set; }
}