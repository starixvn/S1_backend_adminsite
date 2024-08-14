using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Newtonsoft.Json;

namespace BackendSite.Service.BLL;

public class MemberCommand : IMemberCommand
{
    private readonly IMemberService _memberService;
    private readonly VerificationRewardService _verificationRewardService;
    private readonly CodeGenerator _codeGenerator;
    private readonly Tools _tools;
    private readonly CustInfoBLL _custInfoBll;
    private readonly IAdminLogService _adminLogService;
    private readonly IBonusApiService _bonusApi;
    private readonly ILogger<MemberCommand> _logger;
    private readonly ServerInfoBLL _ServerInfoBll;

    public MemberCommand(IMemberService memberService,
        CodeGenerator codeGenerator,
        Tools tools,
        IAdminLogService adminLogService,
        ILogger<MemberCommand> logger,
        IBonusApiService bonusApi,
        CustInfoBLL custInfoBll,
        VerificationRewardService verificationRewardService, ServerInfoBLL serverInfoBll)
    {
        _memberService = memberService;
        _codeGenerator = codeGenerator;
        _tools = tools;
        _adminLogService = adminLogService;
        _logger = logger;
        _bonusApi = bonusApi;
        _custInfoBll = custInfoBll;
        _verificationRewardService = verificationRewardService;
        _ServerInfoBll = serverInfoBll;
    }

    public async Task<int> AddCustTagAsync(int custId, int tagId, string @operator)
    {
        var tagListStr = await _memberService.GetCustomerTagsAsync(custId);
        var tagList = !string.IsNullOrEmpty(tagListStr) ? JsonConvert.DeserializeObject<List<int>>(tagListStr) : new List<int>();
        var beforeTag = await _memberService.GetCustomerTagsAsync(JsonConvert.SerializeObject(tagList));

        if (!tagList.Contains(tagId))
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

    private async Task<IEnumerable<SimilarAccountInfo>> GetSimilarAccountNameAsync(int siteId, IEnumerable<string> accountNames)
    {
        return await _memberService.GetSimilarAccountNameAsync(siteId, accountNames);
    }

    public async Task<IEnumerable<SimilarAccountInfo>> GetDuplicateAccountInfoAsync(int siteId, int custId, string accountName)
    {
        /* 相同定義：
         * * 大小寫不同
         * * 去空格相同
         * 舉例：
         * Cheong wai Choon
         * CheongwaiChoon（相同）
         * cheong Wai Choon（相同）
         * Ho thai nhat
         * Hồ thái nhật（不同）
         * HoThaiNhat（相同）
         */

        var patterns = new[]
        {
            accountName,
            accountName.Replace(" ", string.Empty)
        };
        var similarAccList = await GetSimilarAccountNameAsync(siteId, patterns);
        var result = similarAccList
            .Where(x => x.CustId != custId
                && patterns.Any(y => y.Equals(x.AccountName, StringComparison.CurrentCultureIgnoreCase)));

        return result;
    }

    public async Task<bool> EditFullNameAsync(int custId, string fullName, string @operator)
    {
        var customer = _custInfoBll.GetCustInfoWithCache(custId);
        var beforeName = customer.AccountName;
        var duplicateAccounts = await GetDuplicateAccountInfoAsync(customer.SiteId, custId, fullName);

        var result = await _memberService.EditFullNameAsync(custId, customer.SiteId, fullName, duplicateAccounts);
        if (!result) return false;

        await CreateAdminLog(
            @operator: @operator,
            siteId: customer.SiteId,
            currencyId: customer.CurrencyId,
            beforeLog: JsonConvert.SerializeObject(new { customer.UserName, AccountName = beforeName }),
            afterLog: JsonConvert.SerializeObject(new { customer.UserName, AccountName = fullName }),
            action: ActionType.Update,
            custId: custId,
            message: $"Edit FullName \"{fullName}\"",
            menuEnum: AdminAdjustmentLogMenuEnum.SetCustomerFullName);

        return true;
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
        if (result <= 0) return 0;

        await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId,
            JsonConvert.SerializeObject(new { customer.UserName, Birthday = beforeBirth }),
            JsonConvert.SerializeObject(new { customer.UserName, Birthday = birth }), ActionType.Update,
            custId, $"Edit Birthday \"{birth}\"");
        return result;
    }

    public async Task<int> EditAddressAsync(int custId, string country, string state, string postCode, string city, string line1, string line2, string @operator)
    {
        var customer = await _memberService.GetCustomerBasicInfoAsync(custId);
        var beforeAddress = AddressFormat.Format(customer.AddressLineSecond, customer.AddressLineFirst, customer.AddressCity,
            customer.AddressPostCode, customer.AddressState, customer.AddressCountry);

        var result = await _memberService.EditAddressAsync(custId, country, state, postCode, city, line1, line2);
        if (result <= 0) return 0;

        var address = AddressFormat.Format(line2, line1, city, postCode, state, country);
        await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId,
            JsonConvert.SerializeObject(new { customer.UserName, Address = beforeAddress }),
            JsonConvert.SerializeObject(new { customer.UserName, Address = address }), ActionType.Update,
            custId, $"Edit Address \"{address}\"");
        return result;
    }

    public async Task<int> SwitchKycAsync(int custId, int status, string @operator)
    {
        var customer = await _memberService.GetCustomerBasicInfoAsync(custId);
        var beforeStatus = customer.IsKyc;

        var result = await _memberService.SwitchKycAsync(custId, status);
        if (result <= 0) return 0;

        var afterStatus = status != 0;
        await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId,
            JsonConvert.SerializeObject(new { customer.UserName, IsKyc = beforeStatus.ToString() }),
            JsonConvert.SerializeObject(new { customer.UserName, IsKyc = afterStatus.ToString() }), ActionType.Update,
            custId, $"Edit IsKyc \"{afterStatus}\"");
        return result;
    }

    public async Task<int> SwitchRiskAsync(int custId, int status, string @operator)
    {
        var customer = await _memberService.GetCustomerBasicInfoAsync(custId);
        var beforeStatus = customer.IsRisk;

        var result = await _memberService.SwitchRiskAsync(custId, status);
        if (result <= 0) return 0;

        var afterStatus = status != 0;
        await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId,
            JsonConvert.SerializeObject(new { customer.UserName, IsRisk = beforeStatus.ToString() }),
            JsonConvert.SerializeObject(new { customer.UserName, IsRisk = afterStatus.ToString() }), ActionType.Update,
            custId, $"Edit IsRisk \"{afterStatus}\"");
        return result;
    }

    public async Task<int> SwitchPhoneVerifiedAsync(int custId, int status, string @operator)
    {
        var customer = await _memberService.GetCustomerBasicInfoAsync(custId);
        var beforeStatus = customer.PhoneVerified;

        var result = await _memberService.SwitchPhoneVerifiedAsync(custId, status);
        if (result <= 0) return 0;

        var afterStatus = status != 0;
        await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId,
            JsonConvert.SerializeObject(new { customer.UserName, PhoneVerified = beforeStatus.ToString() }),
            JsonConvert.SerializeObject(new { customer.UserName, PhoneVerified = afterStatus.ToString() }), ActionType.Update,
            custId, $"Edit PhoneVerified \"{afterStatus}\"");
        return result;
    }

    public async Task<int> SwitchEmailVerifiedAsync(int custId, int status, string @operator)
    {
        var customer = await _memberService.GetCustomerBasicInfoAsync(custId);
        var beforeStatus = customer.EmailVerified;

        var result = await _memberService.SwitchEmailVerifiedAsync(custId, status);
        if (result <= 0) return 0;

        var afterStatus = status != 0;
        await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId,
            JsonConvert.SerializeObject(new { customer.UserName, EmailVerified = beforeStatus.ToString() }),
            JsonConvert.SerializeObject(new { customer.UserName, EmailVerified = afterStatus.ToString() }), ActionType.Update,
            custId, $"Edit EmailVerified \"{afterStatus}\"");
        return result;
    }

    public async Task<string> ResetPasswordAsync(int custId, string @operator)
    {
        var customer = _custInfoBll.GetCustInfoWithCache(custId);

        _codeGenerator.Maximum = 8;
        _codeGenerator.Minimum = 6;
        _codeGenerator.RepeatCharacters = true;
        var newPassword = _codeGenerator.Generate();
        var md5NewPassword = _tools.MD5Encrypt(newPassword);

        var result = await _memberService.ResetPasswordAsync(custId, md5NewPassword);
        if (result <= 0) return "";
        await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId,
            JsonConvert.SerializeObject(new { customer.UserName, Password = "*****" }),
            JsonConvert.SerializeObject(new { customer.UserName, Password = "*****" }), ActionType.Update,
            custId, $"Reset Password *****");
        return newPassword;
    }

    public async Task<int> SetCustStatusAsync(int custId, LockCustomerStatusEnum status, string reason, string @operator)
    {
        var customer = await _memberService.GetCustomerBasicInfoAsync(custId);
        var beforeStatus = (LockCustomerStatusEnum)customer.Status;

        var setStatus = status;
        var daysToAdd = status switch
        {
            LockCustomerStatusEnum.FreezeOneWeek => 7,
            LockCustomerStatusEnum.FreezeOneMonth => 30,
            LockCustomerStatusEnum.FreezeThreeMonth => 90,
            _ => 0
        };
        
        if (daysToAdd > 0 )
        {
            setStatus = LockCustomerStatusEnum.Suspend;
            var expiredTime =  DateTime.UtcNow.AddDays(daysToAdd);
            await _memberService.SetCustTemporaryStatusAsync(custId, (int)status, expiredTime, @operator);
            await AddCustTagAsync(custId, 35, @operator);
        }
        
        var result = await _memberService.SetCustStatusAsync(custId, (int)setStatus, (int)CustTaskType.LastStatusUpdateTime);
        if (result <= 0) return 0;

        var afterStatus = status;
        await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId,
            JsonConvert.SerializeObject(new { customer.UserName, Status = beforeStatus.ToString() }),
            JsonConvert.SerializeObject(new { customer.UserName, Status = afterStatus.ToString() }), ActionType.Update,
            custId, $"Change Customer Status \"{afterStatus.ToString()}\", Reason:{reason}");
        return result;
    }

    public async Task<int> SwitchRebatePayoutAsync(int custId, int status, string @operator)
    {
        var customer = await _memberService.GetCustomerBasicInfoAsync(custId);
        var beforeStatus = (EnableRebateEnum)customer.EnableRebate;
        var result = await _memberService.SwitchRebatePayoutAsync(custId, status);
        if (result <= 0) return 0;

        var afterStatus = (EnableRebateEnum)status;
        await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId,
            JsonConvert.SerializeObject(new { customer.UserName, EnableRebate = beforeStatus.ToString() }),
            JsonConvert.SerializeObject(new { customer.UserName, EnableRebate = afterStatus.ToString() }), ActionType.Update,
            custId, $"Enable Rebate \"{afterStatus.ToString()}\"");
        return result;
    }

    public async Task<int> ResetBankInfoAsync(int custId, string @operator)
    {
        var customer = await _memberService.GetCustomerBasicInfoAsync(custId);
        var bankInfo = await _custInfoBll.GetCustBankDetailAsync(custId);
        var result = await _memberService.ResetBankInfoAsync(custId);
        if (result <= 0) return 0;

        await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId,
            JsonConvert.SerializeObject(new { customer.UserName, BankInfo = bankInfo }),
            JsonConvert.SerializeObject(new { customer.UserName, BankInfo = "" }), ActionType.Update,
            custId, $"Reset BankInfo", AdminAdjustmentLogMenuEnum.ResetCustomerBankInfo);
        return result;
    }

    public async Task<int> ResetCryptoInfoAsync(int custId, string @operator)
    {
        var cryptoInfo = await _custInfoBll.GetCustCryptoInfoListAsync(custId);
        var customer = await _memberService.GetCustomerBasicInfoAsync(custId);
        var result = await _memberService.ResetCryptoInfoAsync(custId);
        if (result <= 0) return 0;

        await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId,
            JsonConvert.SerializeObject(new { customer.UserName, CryptoInfo = cryptoInfo }),
            JsonConvert.SerializeObject(new { customer.UserName, CryptoInfo = "" }), ActionType.Update,
            custId, $"Reset CryptoInfo");
        return result;
    }

    public async Task<int> ResetEWalletAsync(int custId, string @operator)
    {
        var customer = await _memberService.GetCustomerBasicInfoAsync(custId);
        var eWallet = await _custInfoBll.GetCustEWalletDetailAsync(custId);
        var result = await _memberService.ResetEWalletAsync(custId);
        if (result <= 0) return 0;

        await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId,
            JsonConvert.SerializeObject(new { customer.UserName, EWalletInfo = eWallet }),
            JsonConvert.SerializeObject(new { customer.UserName, EWalletInfo = "" }), ActionType.Update,
            custId, $"Reset EWalletInfo");
        return result;
    }

    public async Task<ApiResult> SetVerificationRewardAsync(int custId, OTPTypeEnum otpType, string @operator)
    {
        var customer = _custInfoBll.GetCustInfoWithCache(custId);
        var result = await _bonusApi.SetVerificationRewardAsync(custId, (int)otpType);
        if (result.ErrorCode != 0) return result;

        var rewardSetting = await _verificationRewardService.GetVerificationReward(customer.SiteId, customer.CurrencyId);
        var reward = otpType == OTPTypeEnum.SMS ? rewardSetting.PhoneReward : rewardSetting.EmailReward;
        await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId,
            JsonConvert.SerializeObject(new { customer.UserName, OtpType = otpType.ToString(), Reward = 0, }),
            JsonConvert.SerializeObject(new { customer.UserName, OtpType = otpType.ToString(), Reward = reward }), ActionType.Update,
            custId, $"{otpType.ToString()} Verification Reward \"{reward}\"");
        return result;
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

    public async Task<int> DeleteOnUserAsync(int custId, string @operator)
    {
        var customer = _custInfoBll.GetCustInfoWithCache(custId);
        var result = await _memberService.DeleteOnUserAsync(custId);
        if (result <= 0) return 0;

        await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId,
            JsonConvert.SerializeObject(new { customer.UserName, OnlineStatus = "Online" }),
            JsonConvert.SerializeObject(new { customer.UserName, OnlineStatus = "Offline" }), ActionType.Update,
            custId, "Kick Out");
        return result;
    }

    public async Task<bool> SetCustPromotionWhiteListAsync(int custId, bool ipEnable, bool deviceEnable, bool realNameEnable, string @operator)
    {
        var customer = _custInfoBll.GetCustInfoWithCache(custId);
        var beforeWhiteList = await _memberService.GetCustPromotionWhiteListAsync(custId);
        var result = await _memberService.SetCustPromotionWhiteListAsync(new CustPromotionWhiteList()
        {
            CustId = custId,
            IpEnable = ipEnable,
            DeviceEnable = deviceEnable,
            RealNameEnable = realNameEnable,
            SiteId = customer.SiteId,
            UserName = customer.UserName,
            Operator = @operator
        });

        if (result < 1)
        {
            return false;
        }

        await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId,
            JsonConvert.SerializeObject(new
            {
                customer.UserName,
                IpEnable = beforeWhiteList?.IpEnable ?? false,
                DeviceEnable = beforeWhiteList?.DeviceEnable ?? false,
                RealNameEnable = beforeWhiteList?.RealNameEnable ?? false,
            }),
            JsonConvert.SerializeObject(new
            {
                customer.UserName,
                IpEnable = ipEnable,
                DeviceEnable = deviceEnable,
                RealNameEnable = realNameEnable,
            }), ActionType.Update,
            custId, $"Update Promotion WhiteList.");

        return true;
    }

    private async Task CreateAdminLog(string @operator, int siteId, int currencyId, string beforeLog, string afterLog, ActionType action, int custId, string message, AdminAdjustmentLogMenuEnum menuEnum = AdminAdjustmentLogMenuEnum.SetCustomerInfo)
    {
        var adminLog = new AdminLog()
        {
            Operator = @operator,
            SiteId = siteId,
            CurrencyId = currencyId,
            BeforeDetailInfo = beforeLog,
            AfterDetailInfo = afterLog,
            ActionType = action,
            MenuEnum = menuEnum,
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
            _logger.LogError(e, "InsertAdminLog failed. params: " + JsonConvert.SerializeObject(adminLog));
        }
    }

    public async Task<int> RegisterMemberAsync(int siteId, string userName, string userPwd, string email, int currencyId,
        MemberTypeEnum memberType, string areaCode, string phoneNo, string affCode, string signupUrl,string realName, string @operator)
    {
        var result = await _memberService.RegisterMemberAsync(siteId,
                userName,
                userPwd,
                email,
                currencyId,
                _ServerInfoBll.GetServerInfoWithSiteId(siteId).DefLang,
                memberType,
                areaCode,
                phoneNo,
                affCode,
                signupUrl,
                realName);

        if (result.IsSuccess > 0) //Success
        {
            await CreateAdminLog(@operator, siteId, currencyId, null,
                JsonConvert.SerializeObject(new { UserName = userName }), ActionType.Create,
                result.CustId, "Member Create.", AdminAdjustmentLogMenuEnum.RegisterMember);
        }
        return result.IsSuccess;
    }

    public async Task<bool> SetCustTpGameTypeSettingAsync(int custId, CustTpGameTypeSettingGetType setType, IEnumerable<int> tpGameTypes, string @operator)
    {
        var oldData = await _memberService.GetCustTpGameTypeSettingAsync(custId);
        var deleteGameType = Enumerable.Empty<int>();
        var addGameType = Enumerable.Empty<int>();
        switch (setType)
        {
            case CustTpGameTypeSettingGetType.IsPlay: //DB只記錄不可玩
                oldData = oldData.Where(x => x.NoPlay == false); //ex.舊1,2,3可以玩，新2,3,4可以玩 => 1變成不可玩，4變成可玩
                deleteGameType = tpGameTypes.Where(x => !oldData.Select(x => x.GameType).Contains(x)); //刪除 4
                addGameType = oldData.Select(x => x.GameType).Where(x => !tpGameTypes.Contains(x)); //加入 1
                break;
            case CustTpGameTypeSettingGetType.NoPlay: //DB只記錄不可玩
                oldData = oldData.Where(x => x.NoPlay == true); //ex.舊1,2,3不可玩，新2,3,4不可玩 => 1變成可玩，4變成不可玩
                deleteGameType = oldData.Select(x => x.GameType).Where(x => !tpGameTypes.Contains(x)); //刪除 1
                addGameType = tpGameTypes.Where(x => !oldData.Select(x => x.GameType).Contains(x)); //加入 4
                break;
            case CustTpGameTypeSettingGetType.IsShow: //DB只記錄不顯示
                oldData = oldData.Where(x => x.NoShow == false); //ex.舊1,2,3可顯示，新2,3,4可顯示 => 1變成不顯示，4變成顯示
                deleteGameType = tpGameTypes.Where(x => !oldData.Select(x => x.GameType).Contains(x)); //刪除 4
                addGameType = oldData.Select(x => x.GameType).Where(x => !tpGameTypes.Contains(x)); //加入 1
                break;
            case CustTpGameTypeSettingGetType.NoShow: //DB只記錄不顯示
                oldData = oldData.Where(x => x.NoShow == true); //ex.舊1,2,3不顯示，新2,3,4不顯示 => 1變成可顯示，4變成不顯示
                deleteGameType = oldData.Select(x => x.GameType).Where(x => !tpGameTypes.Contains(x)); //刪除 1
                addGameType = tpGameTypes.Where(x => !oldData.Select(x => x.GameType).Contains(x)); //加入 4
                break;
        }

        var type = _custInfoBll.ConvertCustTpGameTypeSettingType(setType);
        var result = await _memberService.SetCustTpGameTypeSettingAsync(custId, deleteGameType, addGameType, type);

        if (result) //Success
        {
            var customer = _custInfoBll.GetCustInfoWithCache(custId);
            await CreateAdminLog(@operator, customer.SiteId, customer.CurrencyId, JsonConvert.SerializeObject(oldData.Select(x => x.GameType)),
                JsonConvert.SerializeObject(tpGameTypes), ActionType.Update,
                custId, $"CustTpGameTypeSetting-{setType} Set.", AdminAdjustmentLogMenuEnum.SetCustTpGameTypeSetting);
        }
        return result;
    }
}