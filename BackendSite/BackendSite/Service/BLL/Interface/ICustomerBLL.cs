using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface;

public interface ICustomerBLL
{
    Task<CustomerBasicInfo> GetCustomerBasicInfo(int custId);
}