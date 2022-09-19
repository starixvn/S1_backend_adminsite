using System.Text;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL;

public class MemberDashboardBll : IMemberDashboardBLL
{
    private readonly ICustomerService _customerService;
    private readonly IDeviceService _deviceService;
    private readonly ICacheHelper _cacheHelper;
    private readonly CodeGenerator _codeGenerator;
    private readonly Tools _tools;

    public MemberDashboardBll(ICustomerService customerService, ICacheHelper cacheHelper, IDeviceService deviceService, CodeGenerator codeGenerator, Tools tools)
    {
        _customerService = customerService;
        _cacheHelper = cacheHelper;
        _deviceService = deviceService;
        _codeGenerator = codeGenerator;
        _tools = tools;
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
            IsRisk = customer.IsRisk,
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
            sb.Append($"{country}");

        return sb.ToString();
    }

    public async Task<IEnumerable<MemberLoginDevice>> GetDeviceListByCustId(int custId)
    {
        var custInfo = await _customerService.GetCustomerBasicInfo(custId);

        if (custInfo.SiteId == (int)SiteEnum.NOVA88)
        {
            var iovationList = await _deviceService.GetCustIovationList(custId);
            return iovationList.Select(a => new MemberLoginDevice()
            {
                DeviceId = a.DeviceId,
                Risk = $"{a.Iovation}({a.Score})",
                Frequency = a.Frequency,
                AssociateCount = a.AssociateCount,
                LastUpdateTime = a.LastUpdateTime
            });
        }

        var shield = await _deviceService.GetCustShieldRiskInfoList(custId);
        return shield.Select(a => new MemberLoginDevice()
        {
            DeviceId = a.DeviceId,
            Risk = a.DeviceScore < 75 ? $"High Risk({a.DeviceScore})" : $"Low Risk({a.DeviceScore})",
            Frequency = a.Frequency,
            AssociateCount = a.AssociateCount,
            LastUpdateTime = a.LastUpdateTime
        });
    }

    public async Task<IEnumerable<CustomerLoginIp>> GetIpListByCustId(int custId)
    {
        return await _customerService.GetCustomerIpListByCustId(custId);
    }

    public async Task<int> EditFullName(int custId, string fullName)
    {
        return await _customerService.EditFullName(custId, fullName);
    }

    public async Task<int> EditEmail(int custId, string email)
    {
        return await _customerService.EditEmail(custId, email);
    }

    public async Task<int> EditPhoneNo(int custId, string phoneNo)
    {
        return await _customerService.SetPhoneNo(custId, phoneNo);
    }

    public async Task<int> EditBirthday(int custId, int year, int month, int date)
    {
        var isValid = DateTime.TryParse($"{year}-{month}-{date}", out _);
        if (!isValid)
            return 0;
        
        return await _customerService.EditBirthday(custId, year, month, date);
    }

    public async Task<int> EditAddress(int custId, string country, string state, string postCode, string city, string line1, string line2)
    {
        return await _customerService.EditAddress(custId, country, state, postCode, city, line1, line2);
    }

    public async Task<int> SwitchKyc(int custId, int status)
    {
        return await _customerService.SwitchKyc(custId, status);
    }

    public async Task<int> SwitchRisk(int custId, int status)
    {
        return await _customerService.SwitchRisk(custId, status);
    }

    public async Task<int> SwitchPhoneVerified(int custId, int status)
    {
        return await _customerService.SwitchPhoneVerified(custId, status);
    }

    public async Task<int> SwitchEmailVerified(int custId, int status)
    {
        return await _customerService.SwitchEmailVerified(custId, status);
    }

    public async Task<string> ResetPassword(int custId)
    {
        _codeGenerator.Maximum = 8;
        _codeGenerator.Minimum = 6;
        _codeGenerator.RepeatCharacters = true;
        var newPassword = _codeGenerator.Generate();
        var md5NewPassword = _tools.MD5Encrypt(newPassword);
        return await _customerService.ResetPassword(custId, md5NewPassword) > 0 ? md5NewPassword : "";
    }

    public async Task<int> SetCustStatus(int custId, int status, string reason)
    {
        return await _customerService.SetCustStatus(custId, status, (int)CustTaskType.LastStatusUpdateTime);
    }

    public async Task<int> SwitchEnableRebateAsync(int custId, int status)
    {
        return await _customerService.SwitchEnableRebateAsync(custId, status);
    }

    public async Task<int> ResetBankInfoAsync(int custId)
    {
        return await _customerService.ResetBankInfoAsync(custId);
    }

    public async Task<int> ResetCryptoInfoAsync(int custId)
    {
        return await _customerService.ResetCryptoInfoAsync(custId);
    }
}