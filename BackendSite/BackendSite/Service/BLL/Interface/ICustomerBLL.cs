using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface;

public interface ICustomerBLL
{
    Task<CustomerDto> GetCustomerBasicInfo(int custId);
}