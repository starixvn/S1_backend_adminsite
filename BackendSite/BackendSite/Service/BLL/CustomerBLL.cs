using System.Text;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL;

public class CustomerBLL : ICustomerBLL
{
    private readonly ICustomerService _customerService;
    private readonly ICacheHelper _cacheHelper;

    public CustomerBLL(ICustomerService customerService, ICacheHelper cacheHelper)
    {
        _customerService = customerService;
        _cacheHelper = cacheHelper;
    }

    public async Task<CustomerBasicInfo> GetCustomerBasicInfo(int custId)
    {
        var customer = await _customerService.GetCustomerBasicInfo(custId);
        var isOnline = await _cacheHelper.KeyExistsAsync(custId.ToString(), RedisDataBase.Token);
        var customerBasicInfo = new CustomerBasicInfo()
        {
            CustId = customer.CustId,
            VipLevel = customer.VipLevel,
            IsOnline = isOnline,
            UserName = customer.UserName,
            AccountName = customer.AccountName,
            PhoneVerified = customer.PhoneVerified,
            EmailVerified = customer.EmailVerified,
            ActStatus = customer.ActStatus,
            IsLineBinding = customer.LineBindingTime.HasValue,
            Status = customer.Status,
            DepositPointLevel = customer.PointLevel,
            Currency = customer.CurrencyName,
            Identity = customer.Identity,
            AffiliatesName = customer.AffiliatesName,
            AffiliatesCode = customer.AffiliatesCode,
            RegisteredTime = customer.RegTime,
            LineBindingTime = customer.LineBindingTime,
            FirstDepositTime = customer.FirstDepositTime,
            LastLoginTime = customer.LastLoginTime,
            LastActiveTime = customer.LastDepositTime,
            BirthDay = customer.BirthDay,
            BirthMonth = customer.BirthMonth,
            BirthYear = customer.BirthYear,
            Email = customer.Email,
            MobileNumber = $"({customer.AreaCode}) {customer.PhoneNo}",
            Address = CreateAddress(customer.AddressLineSecond, customer.AddressLineFirst, customer.AddressCity, customer.AddressPostCode, customer.AddressState, customer.AddressCountry),
            DeviceScore = customer.DeviceScore,
            IsRisk = customer.IsRisk,
            Iovation = customer.Iovation,
            IsKyc = customer.IsKyc,
            IsTest = customer.IsTest,
            TagList = string.IsNullOrEmpty(customer.TagList) ? null : JsonConvert.DeserializeObject<List<int>>(customer.TagList)
        };
        return customerBasicInfo;
    }

    private static string CreateAddress(string line2, string line1, string city, string postCode, string state, string country)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(line2))
            sb.Append($"{line2},");
        if (!string.IsNullOrEmpty(line1))
            sb.Append($"{line1},");
        if (!string.IsNullOrEmpty(city))
            sb.Append($"{city},");
        if (!string.IsNullOrEmpty(postCode))
            sb.Append($"{postCode},");
        if (!string.IsNullOrEmpty(state))
            sb.Append($"{state},");
        if (!string.IsNullOrEmpty(country))
            sb.Append($"{country},");

        return sb.ToString();
    }

    public async Task<IEnumerable<CustomerLoginIp>> GetIpListByCustId(int custId)
    {
        return await _customerService.GetCustomerIpListByCustId(custId);
    }
}