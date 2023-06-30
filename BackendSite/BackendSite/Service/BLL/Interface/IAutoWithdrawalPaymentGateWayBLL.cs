using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface;

public interface IAutoWithdrawalPaymentGateWayBLL
{
    AutoWithdrawalPaymentGateWayBLL.AutoWithdrawalPaymentGateWayDto GetPaymentGateWay(WithdrawalRequest withdrawalData, int siteId);
}