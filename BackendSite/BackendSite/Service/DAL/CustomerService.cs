using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model;

namespace BackendSite.Service.DAL;

public class CustomerService : ICustomerService
{
    public CustomerDto GetCustomerBasicInfo(int custId)
    {
        return new CustomerDto()
        {
            AccountName = "Doris Cheng",
            IsPhoneVerified = false,
            IsEmailVerified = true,
            ActStatus = 1,
            IsLineBinding = false,
            Status = 1,
            DepositPointLevel = 8,
            Currency = "MYR",
            Identity = 1,
            AffiliatesCode = "2gazko2b",
            AffiliatesName = "N123456",
            RegisteredTime = new DateTimeOffset(new DateTime(2022,6,1,10,22,50)),
            LineBindingTime = null,
            FirstDepositTime = null,
            LastLoginTime = new DateTimeOffset(new DateTime(2022,8,19,10,22,50)),
            LastActiveTime = new DateTimeOffset(new DateTime(2022,8,19,10,22,50)),
            BirthDay = null,
            BirthMonth = null,
            BirthYear = null,
            MobileNumber = "1128701701",
            Email = "doristest@gmail.com",
            Address = "asdsadassdaw",
            DeviceScore = 120,
            IsRisk = false,
            IsKyc = false,
            IsTest = true,
            Iovation = "A",
            TagList = null
        };
    }
}