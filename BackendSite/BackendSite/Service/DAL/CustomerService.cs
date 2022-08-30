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

    public async Task<IEnumerable<CustomerIpList>> GetCustomerIpListByCustId(int custId)
    {
        return new List<CustomerIpList>()
        {
            new CustomerIpList() { Ip = "103.22.180.117", Count = 2 },
            new CustomerIpList() { Ip = "125.227.148.102", Count = 4 },
            new CustomerIpList() { Ip = "220.137.228.229", Count = 1 },
            new CustomerIpList() { Ip = "36.225.120.190", Count = 3 },
        };
    }
}