using System.Data;

using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;

using MySql.Data.MySqlClient;

using Newtonsoft.Json;

namespace BackendSite.Service.DAL;

public class MemberService : IMemberService
{
    private readonly MySqlClient _reportDbClient;
    private readonly MySqlClient _mainDbClient;
    private readonly CustInfoService _custInfoService;

    public MemberService(IEnumerable<MySqlClient> sqlCliencts, IServiceProvider provider)
    {
        _reportDbClient = sqlCliencts.FirstOrDefault(a => a.DataBase == DataBase.Report);
        _mainDbClient = sqlCliencts.FirstOrDefault(a => a.DataBase == DataBase.Main);
        _custInfoService = provider.GetRequiredService<CustInfoService>();
    }

    public async Task<GetMemberListPageData<MemberListDto>> GetList(int siteId, IEnumerable<int> currencyList,
        IEnumerable<int> statusList, int identity, string userName, IEnumerable<int> tags, IEnumerable<int> vipList,
        string accountName, string affiliateName, string tpUserName, string loginIp, string email, string phoneNo,
        string bankAccountNumber, string eWalletAccountNumber, string cryptoAddress, string affCode, int transType, string transId, int shieldLevel, int iovationLevel,
        string deviceId, IEnumerable<int> userTypes, int dateType, DateTime startDate, DateTime endDate, int pageNumber, int pageSize, bool excludeTag)
    {
        var p = new DynamicParameters();
        p.Add("_SiteId", siteId);
        p.Add("_CurrencyList", JsonConvert.SerializeObject(currencyList));
        p.Add("_StatusList", JsonConvert.SerializeObject(statusList));
        p.Add("_Identity", identity);
        p.Add("_UserName", userName);
        p.Add("_TagList", JsonConvert.SerializeObject(tags));
        p.Add("_AccountName", accountName);
        p.Add("_AffiliateName", affiliateName);
        p.Add("_TpUserName", tpUserName);
        p.Add("_LoginIp", loginIp);
        p.Add("_VipList", JsonConvert.SerializeObject(vipList));
        p.Add("_Email", email);
        p.Add("_PhoneNo", phoneNo);
        p.Add("_BankAccountNumber", bankAccountNumber);
        p.Add("_EWalletAccountNumber", eWalletAccountNumber);
        p.Add("_CryptoAddress", cryptoAddress);
        p.Add("_AffCode", affCode);
        p.Add("_TransType", transType);
        p.Add("_TransId", transId);
        p.Add("_ShieldLevel", shieldLevel);
        p.Add("_IovationLevel", iovationLevel);
        p.Add("_DeviceId", deviceId);
        p.Add("_UserTypes", JsonConvert.SerializeObject(userTypes));
        p.Add("_DateType", dateType);
        p.Add("_StartDate", startDate);
        p.Add("_EndDate", endDate);
        p.Add("_ExcludeTag", excludeTag ? 1 : 0);
        p.Add("_PageSize", Math.Abs(pageSize));
        p.Add("_StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), DbType.Int32, ParameterDirection.Input);
        p.Add("_PageNumber", Math.Abs(pageNumber), DbType.Int32, ParameterDirection.InputOutput);
        p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
        p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
        var result = await _reportDbClient.QueryAsync<MemberListDto>("Admin_CustInfoSearch_Get_V17", p);
        var searchData = new GetMemberListPageData<MemberListDto>()
        {
            Data = result,
            PageNumber = p.Get<int?>("_PageNumber") ?? 0,
            TotalPages = p.Get<int?>("_TotalPages") ?? 0,
            RowCount = p.Get<int?>("_RowCount") ?? 0
        };

        return searchData;
    }

    public async Task<IEnumerable<MemberExportDataDto>> GetExportData(int siteId, IEnumerable<int> currencyList,
        IEnumerable<int> statusList, int identity, string userName, IEnumerable<int> tags, IEnumerable<int> vipList,
        string accountName, string affiliateName, string tpUserName, string loginIp, string email, string phoneNo,
        string bankAccountNumber, string eWalletAccountNumber, string cryptoAddress, string affCode, int transType, string transId, int shieldLevel, int iovationLevel,
        string deviceId, IEnumerable<int> userTypes, int dateType, DateTime startDate, DateTime endDate, bool excludeTag)
    {
        var p = new DynamicParameters();
        p.Add("_SiteId", siteId);
        p.Add("_CurrencyList", JsonConvert.SerializeObject(currencyList));
        p.Add("_StatusList", JsonConvert.SerializeObject(statusList));
        p.Add("_Identity", identity);
        p.Add("_UserName", userName);
        p.Add("_TagList", JsonConvert.SerializeObject(tags));
        p.Add("_AccountName", accountName);
        p.Add("_AffiliateName", affiliateName);
        p.Add("_TpUserName", tpUserName);
        p.Add("_LoginIp", loginIp);
        p.Add("_VipList", JsonConvert.SerializeObject(vipList));
        p.Add("_Email", email);
        p.Add("_PhoneNo", phoneNo);
        p.Add("_BankAccountNumber", bankAccountNumber);
        p.Add("_EWalletAccountNumber", eWalletAccountNumber);
        p.Add("_CryptoAddress", cryptoAddress);
        p.Add("_AffCode", affCode);
        p.Add("_TransType", transType);
        p.Add("_TransId", transId);
        p.Add("_ShieldLevel", shieldLevel);
        p.Add("_IovationLevel", iovationLevel);
        p.Add("_DeviceId", deviceId);
        p.Add("_UserTypes", JsonConvert.SerializeObject(userTypes));
        p.Add("_DateType", dateType);
        p.Add("_StartDate", startDate);
        p.Add("_EndDate", endDate);
        p.Add("_ExcludeTag", excludeTag ? 1 : 0);
        var result = await _reportDbClient.QueryAsync<MemberExportDataDto>("Admin_CustInfoSearchForExport_Get_V13", p);
        return result;
    }

    public async Task<CustomerDto> GetCustomerBasicInfoAsync(int custId)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        return await _reportDbClient.RetryQueryFirstAsync<CustomerDto>("Admin_CustomerBasicInfo_Get_V2", p);
    }

    public async Task<IEnumerable<CustomerTag>> GetCustomerTagsAsync(string tagList)
    {
        var p = new DynamicParameters();
        p.Add("_TagList", tagList);
        return await _reportDbClient.QueryAsync<CustomerTag>("Admin_CustTag_Get", p);
    }

    public async Task<string> GetCustomerTagsAsync(int custId)
    {
        var p = new DynamicParameters(new { _CustId = custId });
        return await _reportDbClient.QueryFirstAsync<string>("Admin_CustomerTagList_Get", p);
    }

    public async Task<IEnumerable<CustomerLoginIp>> GetCustomerIpListByCustIdAsync(int custId)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        return await _reportDbClient.QueryAsync<CustomerLoginIp>("Admin_CustLoginIp_Get", p);
    }

    public async Task<CustLineProfileModel> GetCustLineProfileAsync(int custId, string lineUserId)
    {
        var p = new DynamicParameters(new { _CustId = custId, _userId = lineUserId });
        return await _reportDbClient.QueryFirstAsync<CustLineProfileModel>("Member_CustLineProfile_Get", p);
    }

    public async Task<LoginLogPageData> GetLoginLogAsync(int custId, DateTime startDate, DateTime endDate, int pageSize, int pageNumber)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        p.Add("_StartDate", startDate);
        p.Add("_EndDate", endDate);
        p.Add("_PageSize", Math.Abs(pageSize));
        p.Add("_StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), DbType.Int32, ParameterDirection.Input);
        p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
        p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
        var loginLog = await _reportDbClient.QueryAsync<LoginLogDto>("Admin_LoginLogByCust_Get_V3", p);
        var loginLogData = new LoginLogPageData
        {
            LoginLog = loginLog,
            PageNumber = pageNumber,
            TotalPages = p.Get<int?>("_TotalPages") ?? 0,
            RowCount = p.Get<int?>("_RowCount") ?? 0
        };
        return loginLogData;
    }
    public async Task<CustomerBalanceInfo> GetCustomerBalanceInfoAsync(int custId)
    {
        var p = new DynamicParameters(new { CustId = custId });
        return await _reportDbClient.QueryFirstAsync<CustomerBalanceInfo>("Admin_CustomerBalanceInfo_Get", p);
    }

    public async Task<IEnumerable<CustomerAssociateBankInfo>> GetCustomerAssociateBankUserAsync(int custId)
    {
        var p = new DynamicParameters(new { _CustId = custId });
        return await _reportDbClient.QueryAsync<CustomerAssociateBankInfo>("Admin_AssociateBankAccountNo_Get", p);
    }

    public async Task<bool> IsExistsPhoneNo(int custId, int siteId, string areaCode, string phoneNo)
    {
        return await _reportDbClient.ExecuteScalarAsync<int>("Admin_ExistsPhoneNo_Get",
            new { _CustId = custId, _AreaCode = areaCode, _PhoneNo = phoneNo, _SiteId = siteId }) != 0;
    }

    public async Task<bool> IsExistsEmail(int custId, string email, int siteId)
    {
        return await _reportDbClient.ExecuteScalarAsync<int>("Admin_ExistsEmail_Get",
            new { _CustId = custId, _Email = email, _SiteId = siteId }) != 0;
    }

    public async Task<int> SetCustTag(int custid, List<int> tagList)
    {
        var p = new DynamicParameters();
        p.Add("CustId", custid);
        p.Add("TagList", JsonConvert.SerializeObject(tagList));
        return await _mainDbClient.ExecuteAsync("Admin_CustInofoTagList_Set", p);
    }

    public async Task<IEnumerable<SimilarAccountInfo>> GetSimilarAccountNameAsync(int siteId, IEnumerable<string> accountNames)
    {
        var p = new DynamicParameters(new
        {
            _SiteId = siteId,
            _AccountNames = JsonConvert.SerializeObject(accountNames)
        });

        return await _mainDbClient.QueryAsync<SimilarAccountInfo>("Admin_SimilarAccountName_Get", p);
    }

    public async Task<bool> EditFullNameAsync(int custId, int siteId, string fullName, IEnumerable<SimilarAccountInfo> duplicateAccounts)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        p.Add("_SiteId", siteId);
        p.Add("_FullName", fullName);

        if (!duplicateAccounts.Any())
        {
            return await _mainDbClient.ExecuteAsync("Admin_CustBankInfoAccountName_Set_V2", p) > 0;
        }

        await using var conn = _mainDbClient.GetMySqlConnection();
        await conn.OpenAsync();
        await using var trans = await conn.BeginTransactionAsync();

        var success = await _mainDbClient.ExecuteAsync("Admin_CustBankInfoAccountName_Set_V2", trans, p) > 0;

        if (success)
        {
            var strTags = await GetCustomerTagsAsync(custId);
            var tags = string.IsNullOrEmpty(strTags)
                ? new List<int>()
                : JsonConvert.DeserializeObject<List<int>>(strTags);

            var custTags = duplicateAccounts
                .Select(x => new CustTagList { CustId = x.CustId, TagList = JsonConvert.DeserializeObject<IEnumerable<int>>(x.TagList ?? "[]") })
                .Where(x => !x.TagList.Contains((int)CustTagEnum.Duplicate))
                .ToList();

            if (!tags.Contains((int)CustTagEnum.Duplicate))
            {
                custTags.Add(new CustTagList { CustId = custId, TagList = tags });
            }

            if (!custTags.Any())
            {
                await _mainDbClient.CommitAndDisposeConnectionAsync(conn, trans);
                return true;
            }

            foreach (var item in custTags)
            {
                item.TagList = item.TagList.Append((int)CustTagEnum.Duplicate);
            }

            success = await SetCustInfoTagList(custTags, trans);
            if (success)
            {
                await _mainDbClient.CommitAndDisposeConnectionAsync(conn, trans);
                return true;
            }
        }

        await _mainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);
        return false;
    }

    public async Task<int> EditEmailAsync(int custId, string email)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        p.Add("_Email", email);
        return await _mainDbClient.ExecuteAsync("Admin_CustProfileEmail_Set", p);
    }

    public async Task<int> SetPhoneNoAsync(int custId, string phoneNo)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        p.Add("_PhoneNo", phoneNo);
        return await _mainDbClient.ExecuteAsync("Admin_CustProfilePhoneNo_Set", p);
    }

    public async Task<int> EditBirthdayAsync(int custId, int year, int month, int date)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        p.Add("_BirthYear", year);
        p.Add("_BirthMonth", month);
        p.Add("_BirthDate", date);
        return await _mainDbClient.ExecuteAsync("Admin_CustProfileBirthday_Set", p);
    }

    public async Task<int> EditAddressAsync(int custId, string country, string state, string postCode, string city, string line1, string line2)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        p.Add("_Country", country);
        p.Add("_State", state);
        p.Add("_PostCode", postCode);
        p.Add("_City", city);
        p.Add("_Line1", line1);
        p.Add("_Line2", line2);
        return await _mainDbClient.ExecuteAsync("Admin_CustProfileAddress_Set", p);
    }

    public async Task<int> SwitchKycAsync(int custId, int status)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        p.Add("_Status", status);
        return await _mainDbClient.ExecuteAsync("Admin_CustomerKyc_Set", p);
    }

    public async Task<int> SwitchRiskAsync(int custId, int status)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        p.Add("_Status", status);
        return await _mainDbClient.ExecuteAsync("Admin_CustomerRisk_Set", p);
    }

    public async Task<int> SwitchPhoneVerifiedAsync(int custId, int status)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        p.Add("_Status", status);
        return await _mainDbClient.ExecuteAsync("Admin_CustProfilePhoneVerified_Set", p);
    }

    public async Task<int> SwitchEmailVerifiedAsync(int custId, int status)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        p.Add("_Status", status);
        return await _mainDbClient.ExecuteAsync("Admin_CustProfileEmailVerified_Set", p);
    }

    public async Task<int> ResetPasswordAsync(int custId, string newPassword)
    {
        var p = new DynamicParameters();
        p.Add("CustId", custId);
        p.Add("Md5NewPassword", newPassword);
        return await _mainDbClient.ExecuteAsync("Admin_ResetPwd_Set", p);
    }

    public async Task<int> SetCustStatusAsync(int custId, int status, int taskId)
    {
        var p = new DynamicParameters();
        p.Add("CustId", custId);
        p.Add("Status", status);
        p.Add("TaskId", taskId);
        return await _mainDbClient.ExecuteAsync("Admin_CustStatus_Set_V2", p);
    }

    public async Task<int> SetCustTemporaryStatusAsync(int custId, int status ,DateTime expiredTime , string @operator)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        p.Add("_Status", status);
        p.Add("_ExpiredTime", expiredTime);
        p.Add("_Operator", @operator);
        return await _mainDbClient.ExecuteAsync("Admin_CustTemporaryStatus_Ins", p);
    }
    
    public async Task<int> DeleteOnUserAsync(int custId)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        return await _mainDbClient.ExecuteAsync("Admin_OnUser_Del", p);
    }

    public async Task<int> SwitchRebatePayoutAsync(int custId, int status)
    {
        var p = new DynamicParameters(new { _CustId = custId, _EnableRebate = status });
        return await _mainDbClient.ExecuteAsync("Admin_CustInfoEnableRebate_Set", p);
    }
    public async Task<int> ResetBankInfoAsync(int custId)
    {
        var p = new DynamicParameters(new { CustId = custId });
        return await _mainDbClient.ExecuteAsync("Admin_ResetCustBankInfo_Set", p);
    }

    public async Task<int> ResetCryptoInfoAsync(int custId)
    {
        var p = new DynamicParameters(new { CustId = custId });
        return await _mainDbClient.ExecuteAsync("Admin_ResetCryptoWallet_Set", p);
    }

    public async Task<int> ResetEWalletAsync(int custId)
    {
        var p = new DynamicParameters(new { CustId = custId });
        return await _mainDbClient.ExecuteAsync("Admin_ResetCustEWallet_Set", p);
    }

    public async Task<int> DeleteLineProfileAsync(int custId)
    {
        var p = new DynamicParameters(new { _CustId = custId });
        return await _mainDbClient.ExecuteAsync("Admin_CustLineProfile_Del", p);
    }

    public async Task<bool> SetCustInfoTagList(IEnumerable<CustTagList> tagList, MySqlTransaction trans)
    {
        var parameter = new DynamicParameters(new
        {
            _Infos = JsonConvert.SerializeObject(tagList)
        });

        return await _mainDbClient.ExecuteAsync("Service_CustInfoTagList_Set", trans, parameter) > 0;
    }

    public async Task<IEnumerable<CustRealNameChangeLog>> GetCustRealNameChangeLogAsync(int custId)
    {
        var p = new DynamicParameters(new { CustId = custId });
        return await _reportDbClient.QueryAsync<CustRealNameChangeLog>("Admin_CustRealNameChangeLog_Get", p);
    }

    public async Task<CustPromotionWhiteList> GetCustPromotionWhiteListAsync(int custId)
    {
        var p = new DynamicParameters(new { CustId = custId });
        return await _reportDbClient.QueryFirstAsync<CustPromotionWhiteList>("Service_CustPromotionWhiteList_Get", p);
    }

    public async Task<int> SetCustPromotionWhiteListAsync(CustPromotionWhiteList input)
    {
        var p = new DynamicParameters(input);
        return await _mainDbClient.ExecuteAsync("Admin_CustPromotionWhiteList_Set", p);
    }

    public async Task<IEnumerable<PromotionReceiveFailLog>> GetPromotionReceiveFailLogAsync(int custId, DateTime startTime, DateTime endTime)
    {
        var p = new DynamicParameters(new { CustId = custId, StartTime = startTime, EndTime = endTime });
        return await _reportDbClient.QueryAsync<PromotionReceiveFailLog>("Admin_PromotionReceiveFailLog_Get", p);
    }

    public async Task<(int IsSuccess, int CustId)> RegisterMemberAsync(int siteId, string userName, string userPwd, string email, int currencyId,
        string userLang, MemberTypeEnum memberType, string areaCode, string phoneNo, string affCode, string signupUrl, string realName)
    {
        string nullValue = null;
        var custId = 0;
        var isSuccess = -1;
        using var conn = _mainDbClient.GetMySqlConnection();
        await conn.OpenAsync();
        await using var trans = await conn.BeginTransactionAsync();

        try
        {
            var p = new DynamicParameters(new
            {
                SiteId = siteId,
                UserName = userName,
                Userpwd = userPwd,
                CurrencyId = currencyId,
                UserLang = userLang,
                AgentId = 0,
                AllAgentId = nullValue,
                UserLevel = 1,
                MemberType = (int)memberType
            });
            p.Add("CustId", dbType: DbType.String, direction: ParameterDirection.Output);

            isSuccess = _mainDbClient.Execute("CustInfo_Ins_V2", trans, p);

            if (isSuccess > 0)
            {
                var strCustId = p.Get<string>("CustId") ?? "";
                custId = int.Parse(strCustId);

                if (custId == 0)
                {
                    trans.Rollback();
                    await conn.CloseAsync();
                    await conn.DisposeAsync();
                    return (IsSuccess: -5, CustId: custId);
                }
            }
            else
            {
                trans.Rollback();
                await conn.CloseAsync();
                await conn.DisposeAsync();
                return (IsSuccess: -5, CustId: custId);
            }

            if (!string.IsNullOrEmpty(email) ||
                 (!string.IsNullOrEmpty(phoneNo) && !string.IsNullOrEmpty(areaCode)))
            {
                if (string.IsNullOrEmpty(email))
                {
                    email = null;
                }
                else if (string.IsNullOrEmpty(phoneNo) || string.IsNullOrEmpty(areaCode))
                {
                    phoneNo = null;
                    areaCode = null;
                }

                if (await IsExistsEmail(custId, email, siteId))
                {
                    trans.Rollback();
                    await conn.CloseAsync();
                    await conn.DisposeAsync();
                    return (IsSuccess: -9, CustId: custId);
                }

                if (await IsExistsPhoneNo(custId, siteId, areaCode, phoneNo))
                {
                    trans.Rollback();
                    await conn.CloseAsync();
                    await conn.DisposeAsync();
                    return (IsSuccess: -10, CustId: custId);
                }
                var profileDic = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(realName)) profileDic.Add("AccountName", realName);

                var profile = profileDic.Count <= 0 ? null : JsonConvert.SerializeObject(profileDic);

                isSuccess = _mainDbClient.Execute("Member_CustProfile_Ins", trans, new DynamicParameters(new
                {
                    CustId = custId,
                    SiteId = siteId,
                    Email = email,
                    Profile = profile,
                    PhoneNo = phoneNo,
                    AreaCode = areaCode
                }));
            }

            if (!string.IsNullOrEmpty(realName))
            {
                SetCustBankInfo(custId, siteId, realName, true, trans);
            }

            if (isSuccess > 0 && !string.IsNullOrEmpty(signupUrl))
            {
                isSuccess = _mainDbClient.Execute("Member_CustRegInfo_Ins_V2", trans,
                    new DynamicParameters(new { CustId = custId, siteId, SignupUrl = signupUrl, ReferralUrl = signupUrl }));
            }

            if (isSuccess > 0 && !string.IsNullOrEmpty(affCode))
            {
                isSuccess = _mainDbClient.Execute("Member_VNAffiliate_Ins", trans,
                    new DynamicParameters(new { CustId = custId, SiteId = siteId, AffId = 0, AffCode = affCode }));
            }

            if (isSuccess > 0)
                await trans.CommitAsync();
            else
                await trans.RollbackAsync();

            await conn.CloseAsync();
            await conn.DisposeAsync();
        }
        catch (Exception ex)
        {
            trans.Rollback();
            await conn.CloseAsync();
            await conn.DisposeAsync();
            isSuccess = -1;
        }
        return (IsSuccess: isSuccess, CustId: custId);
    }

    public bool SetCustBankInfo(int custId, int siteId, string accountName, bool isRegister = false, IDbTransaction trans = null)
    {
        var result = false;
        var p = new DynamicParameters(new
        {
            CustId = custId,
            SiteId = siteId,
            AccountName = accountName
        });

        var duplicateList =
            GetDuplicateAccountNameInfo(siteId, custId, accountName);

        if (!duplicateList.Any())
        {
            return trans == null
                ? _mainDbClient.Execute("Service_CustBankInfo_SetV3", p) > 0
                : _mainDbClient.Execute("Service_CustBankInfo_SetV3", trans, p) > 0;
        }

        using var conn = _mainDbClient.GetMySqlConnection();
        if (trans == null)
        {
            conn.Open();
            trans = conn.BeginTransaction();
        }

        try
        {
            var success = _mainDbClient.Execute("Service_CustBankInfo_SetV3", trans, p) > 0;

            if (success)
            {
                var tags = new List<int>();

                if (!isRegister)
                {
                    var custInfo = _custInfoService.GetCustInfo(custId, true);
                    tags = string.IsNullOrEmpty(custInfo.TagList)
                        ? new List<int>()
                        : System.Text.Json.JsonSerializer.Deserialize<List<int>>(custInfo.TagList);
                }

                var custTags = duplicateList
                    .Where(x => !x.TagList.Contains((int)CustTagEnum.Duplicate))
                    .ToList();

                if (!tags.Contains((int)CustTagEnum.Duplicate))
                {
                    custTags.Add(new CustAccountNameInfo { CustId = custId, TagList = tags });
                }

                if (!custTags.Any())
                {
                    result = true;
                    return true;
                }

                foreach (var item in custTags)
                {
                    item.TagList = item.TagList.Append((int)CustTagEnum.Duplicate);
                }

                success = SetCustInfoTagList(custTags, trans);
                if (success)
                {
                    result = true;
                    return true;
                }
            }

            return false;
        }
        finally
        {
            if (!isRegister)
            {
                if (result)
                {
                    _mainDbClient.CommitAndDisposeConnection(conn, trans);
                }
                else
                {
                    _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                }
            }
            else
            {
                conn.Close();
                conn.Dispose();
            }
        }
    }

    private IEnumerable<CustAccountNameInfo> GetDuplicateAccountNameInfo(int siteId, int custId, string accountName)
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
        var similarAccList = this.GetSimilarAccountName(siteId, patterns);
        var result = similarAccList
            .Where(x => x.CustId != custId
                        && patterns.Any(y => y.Equals(x.AccountName, StringComparison.CurrentCultureIgnoreCase)));

        foreach (var info in result)
        {
            info.TagList = string.IsNullOrEmpty(info.StrTagList)
                ? Array.Empty<int>() : JsonConvert.DeserializeObject<IEnumerable<int>>(info.StrTagList);
        }

        return result;
    }

    public IEnumerable<CustAccountNameInfo> GetSimilarAccountName(int siteId, IEnumerable<string> accountNames)
    {
        return _mainDbClient.Query<CustAccountNameInfo>("Member_SimilarAccountName_Get",
            new
            {
                _SiteId = siteId,
                _AccountNames = System.Text.Json.JsonSerializer.Serialize(accountNames)
            });
    }

    public bool SetCustInfoTagList(IEnumerable<CustTagList> tagList, IDbTransaction trans)
    {
        var result = 0;
        var parameter = new DynamicParameters(new
        {
            _Infos = System.Text.Json.JsonSerializer.Serialize(tagList)
        });

        return _mainDbClient.Execute("Service_CustInfoTagList_Set", trans, parameter) > 0;
    }

    public async Task<bool> SetCustTpGameTypeSettingAsync(int custId, IEnumerable<int> deleteTpGameTypes, IEnumerable<int> addTpGameTypes, CustTpGameTypeSettingTypeEnum type)
    {
        await using var conn = _mainDbClient.GetMySqlConnection();
        await conn.OpenAsync();
        await using var trans = await conn.BeginTransactionAsync();


        _mainDbClient.Execute("Admin_CustTpGameTypeSetting_Del", trans, new DynamicParameters(
                new
                {
                    _CustId = custId,
                    _Type = (int)type,
                    _GameTypes = JsonConvert.SerializeObject(deleteTpGameTypes)
                }));

        _mainDbClient.Execute("Admin_CustTpGameTypeSetting_Ins", trans, new DynamicParameters(
                new
                {
                    _CustId = custId,
                    _Type = (int)type,
                    _GameTypes = JsonConvert.SerializeObject(addTpGameTypes)
                }));

        await trans.CommitAsync();
        await conn.CloseAsync();
        await conn.DisposeAsync();
        return true;
    }

    public async Task<IEnumerable<CustTpGameTypeSettingDto>> GetCustTpGameTypeSettingAsync(int custId)
    {
        var p = new DynamicParameters(new { _CustId = custId });
        return await _reportDbClient.QueryAsync<CustTpGameTypeSettingDto>("Admin_CustTpGameTypeSetting_Get", p);
    }
    public async Task<CustomerForRisk> GetCustId(int? custId, string userName, int? siteId)
    {
        return await _reportDbClient.QueryFirstAsync<CustomerForRisk>("Admin_CustInfo_Get",
            new { CustId = custId, UserName = userName, SiteId = siteId });
    }

}