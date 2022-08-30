using System.Security.Cryptography;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model;
/// <summary>
/// 客戶ID要求
/// </summary>
/// <remarks>
/// 只需要傳參數CustId
/// </remarks>
public class CustIdRequest
{
    public int SiteId { get; set; }
    public int CurrencyId { get; set; }
    public int CustId { get; set; }
}
public class CustInfoListRequest : ApiRequestPageModel
{
    private DateTimeOffset _startDate;
    private DateTimeOffset _endDate;
    private DateTimeOffset _tagStartDate;
    private DateTimeOffset _tagEndDate;
    private DateTimeOffset _statusStartDate;
    private DateTimeOffset _statusEndDate;
    public IEnumerable<int> SiteIds { get; set; }
    public IEnumerable<int> CurrencyIds { get; set; }
    public DateTimeOffset? StartDate
    {
        get => _startDate;
        set
        {
            if (value != null)
                _startDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
    }
    public DateTimeOffset? EndDate
    {
        get => _endDate;
        set
        {
            if (value != null)
                _endDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
    }
    public DateTimeOffset? TagStartDate
    {
        get => _tagStartDate;
        set
        {
            if (value != null)
                _tagStartDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
    }
    public DateTimeOffset? TagEndDate
    {
        get => _tagEndDate;
        set
        {
            if (value != null)
                _tagEndDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
    }
    public DateTimeOffset? StatusStartDate
    {
        get => _statusStartDate;
        set
        {
            if (value != null)
                _statusStartDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
    }
    public DateTimeOffset? StatusEndDate
    {
        get => _statusEndDate;
        set
        {
            if (value != null)
                _statusEndDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
    }
    public int? BirDate { get; set; }
    public int? BirMonth { get; set; }
    public int? BirYear { get; set; }
    public string UserName { get; set; }
    public string AccountName { get; set; }
    public string LoginIp { get; set; }
    public string TransId { get; set; }
    public string TpUserName { get; set; }
    public string Email { get; set; }
    public string PhoneNo { get; set; }
    public string AffCode { get; set; }
    public string BankAccountNumber { get; set; }
    public IEnumerable<int> Statuses { get; set; }
    public IEnumerable<int> SelectTag { get; set; }
    public bool ExportTransferWallet { get; set; } = false;
}

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

public class GetIpListRequest
{
    public int CustId { get; set; }
}

public class CustomerIpList
{
    public string Ip { get; set; }
    public int Count { get; set; }
}

public class CustomerIpDto
{
    public string Ip { get; set; }
    public int Count { get; set; }
}