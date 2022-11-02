using System.Text;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL;

public class MemberCommand : IMemberCommand
{
    private readonly IMemberService _memberService;
    private readonly IDeviceService _deviceService;
    private readonly ICacheHelper _cacheHelper;
    private readonly Dictionary<string, SiteConfig> _siteConfig;
    private readonly PermissionBLL _permissionBll;
    private readonly AdminBLL _adminBll;
    private readonly MarketingService _marketingService;
    private readonly IAffiliateApiService _affiliateApi;

    private readonly CodeGenerator _codeGenerator;
    private readonly Tools _tools;
    private readonly IAdminLogService _adminLogService;
    private readonly IBonusApiService _bonusApi;
    private readonly ILogger<MemberCommand> _logger;


    public MemberCommand(IMemberService memberService, ICacheHelper cacheHelper, IDeviceService deviceService, CodeGenerator codeGenerator, Tools tools, PermissionBLL permissionBll, AdminBLL adminBll, IAdminLogService adminLogService, ILogger<MemberCommand> logger, MarketingService marketingService, IAffiliateApiService affiliateApi, IBonusApiService bonusApi, IOptions<Dictionary<string, SiteConfig>> siteConfig)
    {
        _memberService = memberService;
        _cacheHelper = cacheHelper;
        _deviceService = deviceService;
        _codeGenerator = codeGenerator;
        _tools = tools;
        _permissionBll = permissionBll;
        _adminBll = adminBll;
        _adminLogService = adminLogService;
        _logger = logger;
        _marketingService = marketingService;
        _affiliateApi = affiliateApi;
        _bonusApi = bonusApi;
        _siteConfig = siteConfig.Value;
    }
    
    public async Task<int> AddCustTagAsync(int custId, int tagId, string @operator)
    {
        var tagListStr = await _memberService.GetCustomerTagsAsync(custId);
        var tagList = new List<int>();
        if (!string.IsNullOrEmpty(tagListStr))
        {
            tagList = JsonConvert.DeserializeObject<List<int>>(tagListStr);
        }
        var beforeTag = await _memberService.GetCustomerTagsAsync(JsonConvert.SerializeObject(tagList));
        
        tagList.Add(tagId);
        var result = await _memberService.SetCustTag(custId, tagList);
        if (result <= 0) return result;
        
        var tempTag = await _memberService.GetCustomerTagsAsync(JsonConvert.SerializeObject(tagList));
        var afterTag = tempTag.ToList();
        var newTag = afterTag.FirstOrDefault(a => a.Id == tagId)?.TagName;
        var customer = await _memberService.GetCustomerBasicInfoAsync(custId);
        var beforeTagName = beforeTag.Select(a => a.TagName);
        var afterTagName = afterTag.Select(a => a.TagName);
        var logParam = new AdminLog()
        {
            Operator = @operator,
            SiteId = customer.SiteId,
            CurrencyId = customer.CurrencyId,
            BeforeDetailInfo = JsonConvert.SerializeObject(new
            {
                customer.UserName,
                Tag = JsonConvert.SerializeObject(beforeTagName)
            }),
            AfterDetailInfo = JsonConvert.SerializeObject(new
            {
                customer.UserName,
                Tag = JsonConvert.SerializeObject(afterTagName)
            }),
            ActionType = ActionType.Create,
            MenuEnum = AdminAdjustmentLogMenuEnum.SetCustomerInfo,
            CustId = custId,
            Menu = "CustManage/CustInfoList",
            Note = $"Add Tag {newTag}",
            DetailInfo = JsonConvert.SerializeObject(new
            {
                Action = $"Add Tag {newTag}",
                MenuName = "Member Setting",
            })
        };
        if (await _adminLogService.Insert(logParam) > 0)
        {
            _logger.LogError("InsertAdminLog failed. params: " + JsonConvert.SerializeObject(logParam));
        }
        return result;
    }

    public async Task<int> RemoveCustTagAsync(int custId, int tagId)
    {
        var tagListStr = await _memberService.GetCustomerTagsAsync(custId);
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
        return await _memberService.SetCustTag(custId, tagList);
    }

    public async Task<int> EditFullNameAsync(int custId, string fullName)
    {
        return await _memberService.EditFullNameAsync(custId, fullName);
    }

    public async Task<int> EditEmailAsync(int custId, string email)
    {
        return await _memberService.EditEmailAsync(custId, email);
    }

    public async Task<int> EditPhoneNoAsync(int custId, string phoneNo)
    {
        return await _memberService.SetPhoneNoAsync(custId, phoneNo);
    }

    public async Task<int> EditBirthdayAsync(int custId, int year, int month, int date)
    {
        var isValid = DateTime.TryParse($"{year}-{month}-{date}", out _);
        if (!isValid)
            return 0;
        
        return await _memberService.EditBirthdayAsync(custId, year, month, date);
    }

    public async Task<int> EditAddressAsync(int custId, string country, string state, string postCode, string city, string line1, string line2)
    {
        return await _memberService.EditAddressAsync(custId, country, state, postCode, city, line1, line2);
    }

    public async Task<int> SwitchKycAsync(int custId, int status)
    {
        return await _memberService.SwitchKycAsync(custId, status);
    }

    public async Task<int> SwitchRiskAsync(int custId, int status)
    {
        return await _memberService.SwitchRiskAsync(custId, status);
    }

    public async Task<int> SwitchPhoneVerifiedAsync(int custId, int status)
    {
        return await _memberService.SwitchPhoneVerifiedAsync(custId, status);
    }

    public async Task<int> SwitchEmailVerifiedAsync(int custId, int status)
    {
        return await _memberService.SwitchEmailVerifiedAsync(custId, status);
    }

    public async Task<string> ResetPasswordAsync(int custId)
    {
        _codeGenerator.Maximum = 8;
        _codeGenerator.Minimum = 6;
        _codeGenerator.RepeatCharacters = true;
        var newPassword = _codeGenerator.Generate();
        var md5NewPassword = _tools.MD5Encrypt(newPassword);
        return await _memberService.ResetPasswordAsync(custId, md5NewPassword) > 0 ? newPassword : "";
    }

    public async Task<int> SetCustStatusAsync(int custId, int status, string reason)
    {
        return await _memberService.SetCustStatusAsync(custId, status, (int)CustTaskType.LastStatusUpdateTime);
    }

    public async Task<int> SwitchRebatePayoutAsync(int custId, int status)
    {
        return await _memberService.SwitchRebatePayoutAsync(custId, status);
    }

    public async Task<int> ResetBankInfoAsync(int custId)
    {
        return await _memberService.ResetBankInfoAsync(custId);
    }

    public async Task<int> ResetCryptoInfoAsync(int custId)
    {
        return await _memberService.ResetCryptoInfoAsync(custId);
    }

    public async Task<int> ResetEWalletAsync(int custId)
    {
        return await _memberService.ResetEWalletAsync(custId);
    }

    public async Task<ApiResult> SetVerificationRewardAsync(int custId, int otpType)
    {
        return await _bonusApi.SetVerificationRewardAsync(custId, otpType);
    }

    public async Task<int> LineUnbind(int custId)
    {
        return await _memberService.DeleteLineProfileAsync(custId);
    }

    public async Task<int> DeleteOnUserAsync(int custId)
    {
        return await _memberService.DeleteOnUserAsync(custId);
    }
}