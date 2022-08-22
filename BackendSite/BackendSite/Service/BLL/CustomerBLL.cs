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
            IsOnline = isOnline,
            UserName = customer.UserName,
            AccountName = customer.AccountName,
            IsPhoneVerified = customer.IsPhoneVerified,
            IsEmailVerified = customer.IsEmailVerified,
            ActStatus = customer.ActStatus,
            IsLineBinding = customer.IsLineBinding,
            Status = customer.Status,
            DepositPointLevel = customer.DepositPointLevel,
            Currency = customer.Currency,
            Identity = customer.Identity,
            AffiliatesName = customer.AffiliatesName,
            AffiliatesCode = customer.AffiliatesCode,
            RegisteredTime = customer.RegisteredTime,
            LineBindingTime = customer.LineBindingTime,
            FirstDepositTime = customer.FirstDepositTime,
            LastLoginTime = customer.LastLoginTime,
            LastActiveTime = customer.LastActiveTime,
            BirthDay = customer.BirthDay,
            BirthMonth = customer.BirthMonth,
            BirthYear = customer.BirthYear,
            Email = customer.Email,
            MobileNumber = customer.MobileNumber,
            Address = customer.Address,
            DeviceScore = customer.DeviceScore,
            IsRisk = customer.IsRisk,
            Iovation = customer.Iovation,
            IsKyc = customer.IsKyc,
            IsTest = customer.IsTest,
            TagList = string.IsNullOrEmpty(customer.TagList) ? null : JsonConvert.DeserializeObject<List<int>>(customer.TagList)
        };
        return customerBasicInfo;
    }
}