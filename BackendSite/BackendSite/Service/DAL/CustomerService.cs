using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;

namespace BackendSite.Service.DAL;

public class CustomerService : ICustomerService
{
    private readonly MySqlClient _sqlClienct;
    private readonly MySqlClient _mainSqlClient;

    public CustomerService(IEnumerable<MySqlClient> sqlCliencts)
    {
        _sqlClienct = sqlCliencts.FirstOrDefault(a=>a.DataBase == DataBase.Report);
        _mainSqlClient = sqlCliencts.FirstOrDefault(a => a.DataBase == DataBase.Main);
    }

    public async Task<CustomerDto> GetCustomerBasicInfoAsync(int custId)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        return await _sqlClienct.QueryFirstAsync<CustomerDto>("Admin_CustomerBasicInfo_Get", p);
    }

    public async Task<IEnumerable<CustomerTag>> GetCustomerTagsAsync(string tagList)
    {
        var p = new DynamicParameters();
        p.Add("_TagList", tagList);
        return await _sqlClienct.QueryAsync<CustomerTag>("Admin_CustTag_Get", p);
    }

    public async Task<IEnumerable<CustomerLoginIp>> GetCustomerIpListByCustIdAsync(int custId)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        return await _sqlClienct.QueryAsync<CustomerLoginIp>("Admin_CustLoginIp_Get", p);
    }

    public async Task<CustLineProfileModel> GetCustLineProfileAsync(int custId, string lineUserId)
    {
        var p = new DynamicParameters(new { _CustId = custId, _userId = lineUserId });
        return await _sqlClienct.QueryFirstAsync<CustLineProfileModel>("Member_CustLineProfile_Get", p);
    }

    public async Task<int> EditFullNameAsync(int custId, string fullName)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        p.Add("_FullName", fullName);
        return await _mainSqlClient.ExecuteAsync("Admin_CustBankInfoAccountName_Set", p);
    }

    public async Task<int> EditEmailAsync(int custId, string email)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        p.Add("_Email", email);
        return await _mainSqlClient.ExecuteAsync("Admin_CustProfileEmail_Set", p);
    }

    public async Task<int> SetPhoneNoAsync(int custId, string phoneNo)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        p.Add("_PhoneNo", phoneNo);
        return await _mainSqlClient.ExecuteAsync("Admin_CustProfilePhoneNo_Set", p);
    }

    public async Task<int> EditBirthdayAsync(int custId, int year, int month, int date)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        p.Add("_BirthYear", year);
        p.Add("_BirthMonth", month);
        p.Add("_BirthDate", date);
        return await _mainSqlClient.ExecuteAsync("Admin_CustProfileBirthday_Set", p);
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
        return await _mainSqlClient.ExecuteAsync("Admin_CustProfileAddress_Set", p);
    }

    public async Task<int> SwitchKycAsync(int custId, int status)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        p.Add("_Status", status);
        return await _mainSqlClient.ExecuteAsync("Admin_CustomerKyc_Set", p);
    }

    public async Task<int> SwitchRiskAsync(int custId, int status)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        p.Add("_Status", status);
        return await _mainSqlClient.ExecuteAsync("Admin_CustomerRisk_Set", p);
    }

    public async Task<int> SwitchPhoneVerifiedAsync(int custId, int status)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        p.Add("_Status", status);
        return await _mainSqlClient.ExecuteAsync("Admin_CustProfilePhoneVerified_Set", p);
    }

    public async Task<int> SwitchEmailVerifiedAsync(int custId, int status)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        p.Add("_Status", status);
        return await _mainSqlClient.ExecuteAsync("Admin_CustProfileEmailVerified_Set", p);
    }

    public async Task<int> ResetPasswordAsync(int custId, string newPassword)
    {
        var p = new DynamicParameters();
        p.Add("CustId", custId);
        p.Add("Md5NewPassword", newPassword);
        return await _mainSqlClient.ExecuteAsync("Admin_ResetPwd_Set", p);
    }

    public async Task<int> SetCustStatusAsync(int custId, int status, int taskId)
    {
        var p = new DynamicParameters();
        p.Add("CustId", custId);
        p.Add("Status", status);
        p.Add("TaskId", taskId);
        return await _mainSqlClient.ExecuteAsync("Admin_CustStatus_Set_V2", p);
    }
    
    public async Task<int> SwitchRebatePayoutAsync(int custId, int status)
    {
        var p = new DynamicParameters(new { _CustId = custId, _EnableRebate = status });
        return await _mainSqlClient.ExecuteAsync("Admin_CustInfoEnableRebate_Set", p);
    }
    public async Task<int> ResetBankInfoAsync(int custId)
    {
        var p = new DynamicParameters(new { CustId = custId });
        return await _mainSqlClient.ExecuteAsync("Admin_ResetCustBankInfo_Set", p);
    }

    public async Task<int> ResetCryptoInfoAsync(int custId)
    {
        var p = new DynamicParameters(new { CustId = custId });
        return await _mainSqlClient.ExecuteAsync("Admin_ResetCustUSDTInfo_Set", p);
    }

    public async Task<int> ResetEWalletAsync(int custId)
    {
        var p = new DynamicParameters(new { CustId = custId });
        return await _mainSqlClient.ExecuteAsync("Admin_ResetCustEWallet_Set", p);
    }
    
    public async Task<int> DeleteLineProfileAsync(int custId)
    {
        var p = new DynamicParameters(new { _CustId = custId });
        return await _mainSqlClient.ExecuteAsync("Admin_CustLineProfile_Del", p);
    }
}