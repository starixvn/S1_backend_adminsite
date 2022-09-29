using System.Text;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL;

public class MemberDashboardBll : IMemberDashboardBLL
{
    private readonly ICustomerService _customerService;
    private readonly IDeviceService _deviceService;
    private readonly ICacheHelper _cacheHelper;
    private readonly IOptions<Dictionary<string, SiteConfig>> _siteConfig;
    private readonly CodeGenerator _codeGenerator;
    private readonly Tools _tools;
    private readonly CourierAdapter _courierAdapter;
    private readonly string _bonusApiUrl;
    private const string VIPLOGOPATH = "/desktop/img/vip/";

    public MemberDashboardBll(ICustomerService customerService, ICacheHelper cacheHelper, IDeviceService deviceService, CodeGenerator codeGenerator, Tools tools, CourierAdapter courierAdapter, IConfiguration configuration, IOptions<Dictionary<string, SiteConfig>> siteConfig)
    {
        _customerService = customerService;
        _cacheHelper = cacheHelper;
        _deviceService = deviceService;
        _codeGenerator = codeGenerator;
        _tools = tools;
        _courierAdapter = courierAdapter;
        _siteConfig = siteConfig;
        _bonusApiUrl = configuration.GetValue<string>("BonusService");
    }

    public async Task<CustomerBasicInfo> GetCustomerBasicInfoAsync(int custId)
    {
        var customer = await _customerService.GetCustomerBasicInfoAsync(custId);
        var isOnline = await _cacheHelper.KeyExistsAsync(custId.ToString(), RedisDataBase.Token);
        var custLineProfile = await _customerService.GetCustLineProfileAsync(custId, null);
        var customerBasicInfo = new CustomerBasicInfo()
        {
            CustId = customer.CustId,
            VipLevel = customer.VipLevel,
            VipLogo = $"{_siteConfig.Value[customer.SiteName].BannerURL}{VIPLOGOPATH}vip{customer.VipLevel}.png",
            IsOnline = isOnline,
            UserName = customer.UserName,
            AccountName = customer.AccountName,
            PhoneVerified = customer.PhoneVerified,
            EmailVerified = customer.EmailVerified,
            ActStatus = customer.ActStatus,
            IsLineBinding = custLineProfile != null,
            CustLineProfile = custLineProfile,
            Status = customer.Status,
            DepositPointLevel = customer.PointLevel,
            Currency = customer.CurrencyName,
            Identity = customer.Identity,
            AffiliatesName = customer.AffiliatesName,
            AffiliatesCode = customer.AffiliatesCode,
            RegisteredTime = customer.RegTime,
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
            TagList = customer.TagList == null ? new List<CustomerTag>() : await _customerService.GetCustomerTagsAsync(customer.TagList)
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

    public async Task<IEnumerable<MemberLoginDevice>> GetDeviceListByCustIdAsync(int custId)
    {
        var custInfo = await _customerService.GetCustomerBasicInfoAsync(custId);

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

    public async Task<IEnumerable<CustomerLoginIp>> GetIpListByCustIdAsync(int custId)
    {
        return await _customerService.GetCustomerIpListByCustIdAsync(custId);
    }

    public async Task<int> EditFullNameAsync(int custId, string fullName)
    {
        return await _customerService.EditFullNameAsync(custId, fullName);
    }

    public async Task<int> EditEmailAsync(int custId, string email)
    {
        return await _customerService.EditEmailAsync(custId, email);
    }

    public async Task<int> EditPhoneNoAsync(int custId, string phoneNo)
    {
        return await _customerService.SetPhoneNoAsync(custId, phoneNo);
    }

    public async Task<int> EditBirthdayAsync(int custId, int year, int month, int date)
    {
        var isValid = DateTime.TryParse($"{year}-{month}-{date}", out _);
        if (!isValid)
            return 0;
        
        return await _customerService.EditBirthdayAsync(custId, year, month, date);
    }

    public async Task<int> EditAddressAsync(int custId, string country, string state, string postCode, string city, string line1, string line2)
    {
        return await _customerService.EditAddressAsync(custId, country, state, postCode, city, line1, line2);
    }

    public async Task<int> SwitchKycAsync(int custId, int status)
    {
        return await _customerService.SwitchKycAsync(custId, status);
    }

    public async Task<int> SwitchRiskAsync(int custId, int status)
    {
        return await _customerService.SwitchRiskAsync(custId, status);
    }

    public async Task<int> SwitchPhoneVerifiedAsync(int custId, int status)
    {
        return await _customerService.SwitchPhoneVerifiedAsync(custId, status);
    }

    public async Task<int> SwitchEmailVerifiedAsync(int custId, int status)
    {
        return await _customerService.SwitchEmailVerifiedAsync(custId, status);
    }

    public async Task<string> ResetPasswordAsync(int custId)
    {
        _codeGenerator.Maximum = 8;
        _codeGenerator.Minimum = 6;
        _codeGenerator.RepeatCharacters = true;
        var newPassword = _codeGenerator.Generate();
        var md5NewPassword = _tools.MD5Encrypt(newPassword);
        return await _customerService.ResetPasswordAsync(custId, md5NewPassword) > 0 ? md5NewPassword : "";
    }

    public async Task<int> SetCustStatusAsync(int custId, int status, string reason)
    {
        return await _customerService.SetCustStatusAsync(custId, status, (int)CustTaskType.LastStatusUpdateTime);
    }

    public async Task<int> SwitchRebatePayoutAsync(int custId, int status)
    {
        return await _customerService.SwitchRebatePayoutAsync(custId, status);
    }

    public async Task<int> ResetBankInfoAsync(int custId)
    {
        return await _customerService.ResetBankInfoAsync(custId);
    }

    public async Task<int> ResetCryptoInfoAsync(int custId)
    {
        return await _customerService.ResetCryptoInfoAsync(custId);
    }

    public async Task<int> ResetEWalletAsync(int custId)
    {
        return await _customerService.ResetEWalletAsync(custId);
    }

    public async Task<ApiResult> SetVerificationRewardAsync(int custId, int otpType)
    {
        var rewardUrl = $"{_bonusApiUrl}SetVerificationReward";
        var verificationReq = new SetVerificationRequest()
        {
            CustId = custId,
            OTPType = otpType
        };

        var apiResult = await _courierAdapter.MyPostAsync<ApiResult, SetVerificationRequest>(rewardUrl, verificationReq);
        return apiResult;
    }

    public async Task<int> LineUnbind(int custId)
    {
        return await _customerService.DeleteLineProfileAsync(custId);
    }
}