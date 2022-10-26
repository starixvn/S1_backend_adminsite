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

    public async Task<GetMemberListResponse> GetList(GetTempMemberListRequest request)
    {
        var startTime = request.StartDate.ToUniversalTime().DateTime;
        var endTime = request.EndDate.ToUniversalTime().DateTime;
        var currencyList = string.Join(',', request.CurrencyId.ToArray());
        var statusList = string.Join(',', request.Status.ToArray());
        var vipList = request.VipLevel != null ? string.Join(',', request.VipLevel.ToArray()) : null;
        var transType = GetTransType(request.TransId);
        var memberList = await _customerService.GetList(request.SiteId, currencyList, statusList, request.Identity, request.UserName, request.Tags, vipList, request.AccountName, request.TpUserName, request.LoginIp, request.Email, request.PhoneNo, request.BankAccountNumber, request.AffiliateCode, transType, request.TransId, request.ShieldLevel, request.IovationLevel, request.DeviceId, request.DateType, startTime, endTime, request.PageNumber, request.PageSize);
        var response = new GetMemberListResponse()
        {
            Data = memberList.Data.Select(a => new MemberList()
            {
                SiteName = a.SiteName,
                CurrencyId = a.CurrencyId,
                IsoCode = a.IsoCode,
                CustId = a.CustId,
                UserName = a.UserName,
                SabaId = a.SabaId,
                AffiliateCode = a.AffiliateCode,
                TagList = string.IsNullOrEmpty(a.TagList) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(a.TagList),
                Identity = 1,
                UserType = 1,
                PointLevel = 1,
                Agent = a.Agent,
                Status = a.Status,
                IsTest = a.IsTest,
                IP = a.IP,
                LevelName = a.LevelName,
                RegTime = new DateTimeOffset(a.RegTime, TimeSpan.Zero),
                LastActiveTime = Convert(a.ReloadTime, TimeSpan.Zero),
                LastTagUpdateTime = Convert(a.LastTagUpdateTime, TimeSpan.Zero),
                LastStatusUpdateTime = Convert(a.LastStatusUpdateTime, TimeSpan.Zero),
                Balance = a.Balance,
                Bonus = a.Bonus,
                DeviceScore = a.DeviceScore,
                DeviceId = a.ShieldId
            }),
            PageNumber = memberList.PageNumber,
            TotalPages = memberList.TotalPages,
            RowCount = memberList.RowCount
        };
        return response;
    }

    public async Task<IEnumerable<MemberExportData>> GetExportData(ExportMemberRequest request)
    {
        var startTime = request.StartDate.ToUniversalTime().DateTime;
        var endTime = request.EndDate.ToUniversalTime().DateTime;
        var currencyList = string.Join(',', request.CurrencyId.ToArray());
        var statusList = string.Join(',', request.Status.ToArray());
        var vipList = request.VipLevel != null ? string.Join(',', request.VipLevel.ToArray()) : null;
        var transType = GetTransType(request.TransId);
        var memberList = await _customerService.GetExportData(request.SiteId, currencyList, statusList,
            request.Identity, request.UserName, request.Tags, vipList, request.AccountName, request.TpUserName,
            request.LoginIp, request.Email, request.PhoneNo, request.BankAccountNumber, request.AffiliateCode,
            transType, request.TransId, request.ShieldLevel, request.IovationLevel, request.DeviceId, request.DateType,
            startTime, endTime);

        var exportData = memberList.Select(a => new MemberExportData()
        {
            SiteName = a.SiteName,
            UserName = a.UserName,
            CustId = a.CustId,
            Currency = a.IsoCode,
            BirthDate = a.BirthDate,
            BirthMonth = a.BirthMonth,
            BirthYear = a.BirthYear,
            IsTest = a.IsTest,
            IsKyc = a.IsKyc,
            IsRisk = a.IsRisk,
            SeamlessWallet = a.SeamlessWallet,
            BonusWallet = a.BonusWallet,
            Email = a.Email,
            RealName = a.RealName,
            CountryCode = a.CountryCode,
            AreaCode = a.AreaCode,
            PhoneNo = a.PhoneNo,
            LevelName = a.LevelName,
            Agent = a.Agent,
            RegTime = new DateTimeOffset(a.RegTime, TimeSpan.Zero),
            Status = a.Status,
            SabaId = a.SabaId,
            SignupUrl = a.SignupUrl,
            AffiliateId = a.AffiliateId,
            AffiliateCode = a.AffiliateCode,
            LastActivityIp = a.LastActivityIp
        });
        return exportData;
    }

    private int GetTransType(string transId)
    {
        if (string.IsNullOrEmpty(transId)) 
            return 0;
        if (transId.StartsWith("W"))
            return 1;
        if (transId.StartsWith("D"))
            return 2;
        if (transId.StartsWith("T") || transId.StartsWith("AT") || transId.StartsWith("BT"))
            return 3;
        return 0;
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
            EnableRebate = customer.EnableRebate,
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
                Risk = a.Iovation,
                Score = a.Score,
                Frequency = a.Frequency,
                DeviceType = a.DeviceType,
                AssociateCount = a.AssociateCount,
                LastUpdateTime = a.LastUpdateTime
            });
        }

        var shield = await _deviceService.GetCustShieldRiskInfoList(custId);
        return shield.Select(a => new MemberLoginDevice()
        {
            DeviceId = a.DeviceId,
            Risk = a.DeviceScore < 75 ? "High Risk" : "Low Risk",
            Score = a.DeviceScore,
            Frequency = a.Frequency,
            DeviceType = a.Platform,
            AssociateCount = a.AssociateCount,
            LastUpdateTime = a.LastUpdateTime
        });
    }

    public async Task<IEnumerable<CustomerLoginIp>> GetIpListByCustIdAsync(int custId)
    {
        return await _customerService.GetCustomerIpListByCustIdAsync(custId);
    }

    public async Task<int> AddCustTagAsync(int custId, int tagId)
    {
        var tagListStr = await _customerService.GetCustomerTagsAsync(custId);
        var tagList = new List<int>();
        if (!string.IsNullOrEmpty(tagListStr))
        {
            tagList = JsonConvert.DeserializeObject<List<int>>(tagListStr);
        }
        tagList.Add(tagId);
        return await _customerService.SetCustTag(custId, tagList);
    }

    public async Task<int> RemoveCustTagAsync(int custId, int tagId)
    {
        var tagListStr = await _customerService.GetCustomerTagsAsync(custId);
        if (string.IsNullOrEmpty(tagListStr))
        {
            return 0;
        }
        var tagList = JsonConvert.DeserializeObject<List<int>>(tagListStr);
        var idx = tagList.FindIndex(id => id == tagId);
        if (idx == -1)
        {
            return 0;
        }
        tagList.RemoveAt(idx);
        return await _customerService.SetCustTag(custId, tagList);
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

    public async Task<CustomerBalanceInfo> GetCustomerBalanceInfoAsync(int custId)
    {
        return await _customerService.GetCustomerBalanceInfoAsync(custId);
    }

    public async Task<int> LineUnbind(int custId)
    {
        return await _customerService.DeleteLineProfileAsync(custId);
    }

    public async Task<int> DeleteOnUserAsync(int custId)
    {
        return await _customerService.DeleteOnUserAsync(custId);
    }

    public async Task<GetLoginLogResponse> GetLoginLogAsync(int custId, DateTime startTime, DateTime endTime, int pageSize, int pageNumber)
    {
        var data = await _customerService.GetLoginLogAsync(custId, startTime, endTime, pageSize, pageNumber);
        var response = new GetLoginLogResponse()
        {
            LoginLog = data.LoginLog.Select(a => new MemberLoginLog()
            {
                Id = a.Id,
                LoginTime = new DateTimeOffset(a.LoginTime, TimeSpan.Zero),
                LoginIp = a.LoginIp,
                FromUrl = a.FromUrl,
                Location = a.Location,
                UserAgent = a.UserAgent
            }),
            PageNumber = data.PageNumber,
            TotalPages = data.TotalPages,
            RowCount = data.RowCount
        };
        return response;
    }

    private DateTimeOffset? Convert(DateTime? dateTime, TimeSpan ts)
    {
        if (dateTime.HasValue)
            return new DateTimeOffset(dateTime.Value, ts);
        return null;
    }
}