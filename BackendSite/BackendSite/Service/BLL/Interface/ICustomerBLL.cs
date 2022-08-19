using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface;

public interface ICustomerBLL
{
    CustomerDto GetCustomerBasicInfo(int custId);
}