using System.Security.Cryptography;

namespace BackendSite.Service.Model;

public class GetCustomerBasicInfoRequest
{
    public int CustId { get; set; }
}

public class CustomerDto
{
    public string AccountName { get; set; }
    public bool IsPhoneVerified { get; set; }
    public bool IsEmailVerified { get; set; }
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
    public string TagList { get; set; }
}