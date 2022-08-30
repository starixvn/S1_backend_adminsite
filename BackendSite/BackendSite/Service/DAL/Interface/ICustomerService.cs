using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface;

public interface ICustomerService
{
    Task<CustomerDto> GetCustomerBasicInfo(int custId);
    Task<IEnumerable<CustomerIpList>> GetCustomerIpListByCustId(int custId);
}