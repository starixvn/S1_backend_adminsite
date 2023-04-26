using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL.Interface;

public interface IWithdrawalAccountBLL
{
    /// <summary>
    /// 提款單狀態變更，Set 帳號提領成功次數
    /// </summary>
    /// <param name="withdrawalRequest"></param>
    /// <param name="changeToStatus"></param>
    /// <returns></returns>
    Task WithdrawalAccountCounter(WithdrawalRequest withdrawalRequest, WithdrawalStatusEnum changeToStatus);

    WithdrawalAccountTypeEnum MapWithdrawalAccountType(string accountType);
}