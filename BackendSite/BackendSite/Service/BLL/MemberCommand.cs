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
    private readonly CodeGenerator _codeGenerator;
    private readonly Tools _tools;
    private readonly CustInfoBLL _custInfoBll; 
    private readonly IAdminLogService _adminLogService;
    private readonly IBonusApiService _bonusApi;
    private readonly ILogger<MemberCommand> _logger;

    public MemberCommand(IMemberService memberService, CodeGenerator codeGenerator, Tools tools, IAdminLogService adminLogService, ILogger<MemberCommand> logger, IBonusApiService bonusApi, CustInfoBLL custInfoBll)
    {
        _memberService = memberService;
        _codeGenerator = codeGenerator;
        _tools = tools;
        _adminLogService = adminLogService;
        _logger = logger;
        _bonusApi = bonusApi;
        _custInfoBll = custInfoBll;
    }
    
    public async Task<int> AddCustTagAsync(int custId, int tagId, string @operator)
    {
        var tagListStr = await _memberService.GetCustomerTagsAsync(custId);
        var tagList = !string.IsNullOrEmpty(tagListStr) ? JsonConvert.DeserializeObject<List<int>>(tagListStr) : new List<int>();
        var beforeTag = await _memberService.GetCustomerTagsAsync(JsonConvert.SerializeObject(tagList));

        tagList.Add(tagId);
        var result = await _memberService.SetCustTag(custId, tagList);
        if (result <= 0) return result;
        
        var tempTag = await _memberService.GetCustomerTagsAsync(JsonConvert.SerializeObject(tagList));
        var afterTag = tempTag.ToList();
        var newTag = afterTag.FirstOrDefault(a => a.Id == tagId)?.TagName;
        var customer = _custInfoBll.GetCustInfoWithCache(custId);
        var beforeTagName = beforeTag.Select(a => a.TagName);
        var afterTagName = afterTag.Select(a => a.TagName);
        
        await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId,
            JsonConvert.SerializeObject(new { customer.UserName, Tag = JsonConvert.SerializeObject(beforeTagName) }),
            JsonConvert.SerializeObject(new { customer.UserName, Tag = JsonConvert.SerializeObject(afterTagName) }), ActionType.Create,
            custId, $"Add Tag \"{newTag}\"");
        return result;
    }

    public async Task<int> RemoveCustTagAsync(int custId, int tagId, string @operator)
    {
        var tagListStr = await _memberService.GetCustomerTagsAsync(custId);
        if (string.IsNullOrEmpty(tagListStr)) return 0;
        var tagList = JsonConvert.DeserializeObject<List<int>>(tagListStr);
        
        var tempTag = await _memberService.GetCustomerTagsAsync(JsonConvert.SerializeObject(tagList));
        var beforeTag = tempTag.ToList();
        var idx = tagList.FindIndex(id => id == tagId);
        if (idx == -1) return 0;
        
        tagList.RemoveAt(idx);
        var result = await _memberService.SetCustTag(custId, tagList);
        if (result <= 0) return result;
        
        var afterTag = await _memberService.GetCustomerTagsAsync(JsonConvert.SerializeObject(tagList));
        var newTag = beforeTag.FirstOrDefault(a => a.Id == tagId)?.TagName;
        var customer = _custInfoBll.GetCustInfoWithCache(custId);
        var beforeTagName = beforeTag.Select(a => a.TagName);
        var afterTagName = afterTag.Select(a => a.TagName);
        
        await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId,
            JsonConvert.SerializeObject(new { customer.UserName, Tag = JsonConvert.SerializeObject(beforeTagName) }),
            JsonConvert.SerializeObject(new { customer.UserName, Tag = JsonConvert.SerializeObject(afterTagName) }), ActionType.Delete,
            custId, $"Remove Tag \"{newTag}\"");

        return result;
    }

    public async Task<int> EditFullNameAsync(int custId, string fullName, string @operator)
    {
        var customer = _custInfoBll.GetCustInfoWithCache(custId);
        var beforeName = customer.AccountName;
        
        var result = await _memberService.EditFullNameAsync(custId, fullName);
        if (result <= 0) return 0;
        
        await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId,
            JsonConvert.SerializeObject(new { customer.UserName, AccountName = beforeName }),
            JsonConvert.SerializeObject(new { customer.UserName, AccountName = fullName }), ActionType.Update,
            custId, $"Edit FullName \"{fullName}\"");
        return result;
    }

    public async Task<int> EditEmailAsync(int custId, string email, string @operator)
    {
        var customer = await _memberService.GetCustomerBasicInfoAsync(custId);

        if (await _memberService.IsExistsEmail(custId, email, customer.SiteId)) 
            return -1;

        var beforeEmail = customer.Email;
        var result = await _memberService.EditEmailAsync(custId, email);
        if (result <= 0) 
            return 0;

        await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId,
            JsonConvert.SerializeObject(new { customer.UserName, Email = beforeEmail }),
            JsonConvert.SerializeObject(new { customer.UserName, Email = email }), ActionType.Update,
            custId, $"Edit Email \"{email}\"");
        
        return result;
    }

    public async Task<int> EditPhoneNoAsync(int custId, string phoneNo, string @operator)
    {
        var customer = _custInfoBll.GetCustInfoWithCache(custId);

        if (await _memberService.IsExistsPhoneNo(custId, customer.SiteId, customer.AreaCode, phoneNo)) return -1;

        var beforePhone = customer.PhoneNo;
        var result = await _memberService.SetPhoneNoAsync(custId, phoneNo);
        if (result <= 0) return 0;
        
        await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId,
            JsonConvert.SerializeObject(new { customer.UserName, PhoneNo = beforePhone }),
            JsonConvert.SerializeObject(new { customer.UserName, PhoneNo = phoneNo }), ActionType.Update,
            custId, $"Edit PhoneNo \"{phoneNo}\"");
        
        return result; 
    }

    public async Task<int> EditBirthdayAsync(int custId, int year, int month, int date, string @operator)
    {
        var birth = $"{month}-{date}-{year}";
        var isValid = DateTime.TryParse(birth, out _);
        if (!isValid)
            return 0;

        var customer = await _memberService.GetCustomerBasicInfoAsync(custId);
        var beforeBirth = $"{customer.BirthMonth}-{customer.BirthDay}-{customer.BirthYear}";
        var result = await _memberService.EditBirthdayAsync(custId, year, month, date);
        
        await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId,
            JsonConvert.SerializeObject(new { customer.UserName, Birthday = beforeBirth }),
            JsonConvert.SerializeObject(new { customer.UserName, Birthday = birth }), ActionType.Update,
            custId, $"Edit Birthday \"{birth}\"");
        return result;
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

    public async Task<int> LineUnbind(int custId, string @operator)
    {
        var customer = _custInfoBll.GetCustInfoWithCache(custId);
        
        var result = await _memberService.DeleteLineProfileAsync(custId);
        if (result <= 0) return 0;
        
        await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId, 
            JsonConvert.SerializeObject(new { customer.UserName, LineStatus = "Binding" }),
            JsonConvert.SerializeObject(new { customer.UserName, LineStatus = "Unbind" }), ActionType.Update, 
            custId, "Line Unbind");
        
        return result;
    }

    public async Task<int> DeleteOnUserAsync(int custId)
    {
        return await _memberService.DeleteOnUserAsync(custId);
    }

    private async Task CreateAdminLog(string @operator, int siteId, int currencyId, string beforeLog, string afterLog, ActionType action, int custId, string message)
    {
        var adminLog = new AdminLog()
        {
            Operator = @operator,
            SiteId = siteId,
            CurrencyId = currencyId,
            BeforeDetailInfo = beforeLog,
            AfterDetailInfo = afterLog,
            ActionType = action,
            MenuEnum = AdminAdjustmentLogMenuEnum.SetCustomerInfo,
            CustId = custId,
            Menu = "CustManage/CustInfoList",
            Note = message,
            DetailInfo = JsonConvert.SerializeObject(new
            {
                Action = message,
                MenuName = "Member Setting",
            })
        };
        try
        {
            await _adminLogService.Insert(adminLog);
        }
        catch (Exception e)
        {
            _logger.LogError(e,"InsertAdminLog failed. params: " + JsonConvert.SerializeObject(adminLog));
        }
    }
}