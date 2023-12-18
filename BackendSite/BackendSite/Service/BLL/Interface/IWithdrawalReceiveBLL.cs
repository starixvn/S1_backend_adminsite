using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface;

public interface IWithdrawalReceiveBLL
{
    /// <summary>
    /// 提款單上鎖，並同時更新 db onUser欄位
    /// </summary>
    /// <param name="transId"></param>
    /// <param name="userName"></param>
    /// <param name="siteId"></param>
    /// <param name="writeLog"></param>
    /// <returns></returns>
    Task<RedisSetAndGetResult> LockWithdrawalTicketAsync(string transId, string userName, int siteId);

    /// <summary>
    /// 提款單解鎖，並同時更新 db onUser欄位
    /// </summary>
    /// <param name="transId"></param>
    /// <param name="siteId"></param>
    /// <param name="userName"></param>
    /// <returns></returns>
    Task<bool> UnLockWithdrawalTicketAsync(string transId, int siteId, string userName = "");

    /// <summary>
    /// 提款單蓋單，覆蓋 Redis key 的 value，並同時更新 db onUser欄位 
    /// </summary>
    /// <param name="transId"></param>
    /// <param name="userName"></param>
    /// <param name="siteId"></param>
    /// <returns></returns>
    Task<bool> OverrideLockWithdrawalTicketUserAsync(string transId, string userName, int siteId);

    /// <summary>
    /// 判斷提款單 Lock 是否存在
    /// </summary>
    /// <param name="transId"></param>
    /// <returns></returns>
    Task<bool> LockWithdrawalTicketExistsAsync(string transId);
}