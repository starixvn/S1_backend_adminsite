using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL;

public class CustomerBLL : ICustomerBLL
{
    private readonly ICustomerService _customerService;

    public CustomerBLL(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<CustomerDto> GetCustomerBasicInfo(int custId)
    {
        var customerBasicInfo = await _customerService.GetCustomerBasicInfo(custId);

        return customerBasicInfo;
    }
}