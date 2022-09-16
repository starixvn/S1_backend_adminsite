using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;

namespace BackendSite.Service.DAL;

public class CustomerService : ICustomerService
{
    private readonly MySqlClient _sqlClienct;

    public CustomerService(IEnumerable<MySqlClient> sqlCliencts)
    {
        _sqlClienct = sqlCliencts.FirstOrDefault(a=>a.DataBase == DataBase.Report);
    }

    public async Task<CustomerDto> GetCustomerBasicInfo(int custId)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        return await _sqlClienct.QueryFirstAsync<CustomerDto>("Admin_CustomerBasicInfo_Get", p);
    }

    public async Task<IEnumerable<CustomerLoginIp>> GetCustomerIpListByCustId(int custId)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        return await _sqlClienct.QueryAsync<CustomerLoginIp>("Admin_CustLoginIp_Get", p);
    }
}