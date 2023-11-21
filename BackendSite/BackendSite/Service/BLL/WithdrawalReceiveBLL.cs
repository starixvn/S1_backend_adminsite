using System.Text.RegularExpressions;

using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL
{
    public class WithdrawalReceiveBLL : IWithdrawalReceiveBLL
    {
        private readonly ICacheHelper _redisHelper;
        private readonly WithdrawalService _withdrawalService;

        private const string UnLockWithdrawalScript = @"
        if redis.call(""get"",KEYS[1]) == ARGV[1] then
            return redis.call(""del"",KEYS[1])
        else
            return 0
        end";

        public WithdrawalReceiveBLL(ICacheHelper redisHelper, WithdrawalService withdrawalService)
        {
            _redisHelper = redisHelper;
            _withdrawalService = withdrawalService;
        }

        public async Task<RedisSetAndGetResult> LockWithdrawalTicketAsync(string transId, string userName, int siteId)
        {
            var key = $"lock:withdrawalTransId:{transId}";
            var createLockResult = await _redisHelper.TryCreateLockAsync(key, userName, null);
            if (createLockResult)
            {
                var _ = await _withdrawalService.SetWithdrawalInfoOnUser(transId, onUser: userName);
                _withdrawalService.SetWithdrawalLog(GetWithdrawalId(transId), siteId, "", (int)WithdrawalStatusEnum.Receive, Operator: userName);
            }
            else
            {
                userName = await _redisHelper.StringGetAsync(key);
            }

            return new RedisSetAndGetResult
            {
                Success = createLockResult,
                Value = userName,
            };
        }

        public async Task<bool> UnLockWithdrawalTicketAsync(string transId, int siteId, string userName = "")
        {
            bool unLockResult;
            var key = $"lock:withdrawalTransId:{transId}";
            if (!string.IsNullOrEmpty(userName))
            {
                // 只有原本Lock的人可以自己 UnLock，Lua Script 會判斷 value 是否相同
                unLockResult = await _redisHelper.UnLockAsync(key, userName);
                if (unLockResult)
                {
                    var _ = await _withdrawalService.SetWithdrawalInfoOnUser(transId, onUser: "");
                    _withdrawalService.SetWithdrawalLog(GetWithdrawalId(transId), siteId, "", (int)WithdrawalStatusEnum.UnReceive, Operator: userName);
                }
            }
            else
            {
                unLockResult = _redisHelper.KeyDelete(key);
                var _ = await _withdrawalService.SetWithdrawalInfoOnUser(transId, onUser: "");
            }

            return unLockResult;
        }

        public async Task<bool> OverrideLockWithdrawalTicketUserAsync(string transId, string userName, int siteId)
        {
            var key = $"lock:withdrawalTransId:{transId}";
            var setSuccess = await _redisHelper.StringSetAsync(key, userName);
            if (setSuccess)
            {
                var _ = await _withdrawalService.SetWithdrawalInfoOnUser(transId, onUser: userName);
                _withdrawalService.SetWithdrawalLog(GetWithdrawalId(transId), siteId, "", (int)WithdrawalStatusEnum.Receive, Operator: userName);
            }

            return setSuccess;
        }

        public async Task<bool> LockWithdrawalTicketExistsAsync(string transId)
        {
            var key = $"lock:withdrawalTransId:{transId}";
            return await _redisHelper.KeyExistsAsync(key);
        }

        private int GetWithdrawalId(string transId)
        {
            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var withdrawalId);
            return withdrawalId;
        }
    }
}
