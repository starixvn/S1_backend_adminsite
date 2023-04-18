using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface;

public interface IWithdrawalReceiveBLL
{
    Task<RedisSetAndGetResult> LockWithdrawalTicketAsync(string transId, string userName, int siteId, bool writeLog = true);
    Task<bool> UnLockWithdrawalTicketAsync(string transId, int siteId, string userName = "");
    Task<bool> OverrideLockWithdrawalTicketUserAsync(string transId, string userName, int siteId);
    Task<bool> LockWithdrawalTicketExistsAsync(string transId);
}