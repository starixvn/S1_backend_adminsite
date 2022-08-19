using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface;

public interface ICustomerService
{
    CustomerDto GetCustomerBasicInfo(int custId);
}