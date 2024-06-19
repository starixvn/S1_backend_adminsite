using BackendSite.Extensions;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class PaymentManageBLL
    {
        private readonly PaymentManageService _paymentManageService;
        private readonly ICacheHelper _redisHelper;

        public PaymentManageBLL(IServiceProvider provider)
        {
            _paymentManageService = provider.GetRequiredService<PaymentManageService>();
            _redisHelper = provider.GetRequiredService<ICacheHelper>();
        }

        public IEnumerable<PayChannelInfo> GetPayChannel(int siteId)
        {
            var list = _paymentManageService.GetPaySetting(siteId);

            return list.Select(x => new PayChannelInfo(x))
                .Distinct(new PayChannelInfoComparer());
        }

        public IEnumerable<PayListInfo> GetPayList(int siteId)
        {
            IEnumerable<int> _siteIds = Enumerable.Repeat(siteId, 1);
            return _paymentManageService.GetPayList(_siteIds);
        }
        public IEnumerable<PayListInfo> GetPayListByMultiSite(IEnumerable<int> siteIds)
        {
            return _paymentManageService.GetPayList(siteIds);
        }

        public bool SetPayListStatus(int siteId, int payId, PayListStatusEnum status, string @operator)
        {
            var result = _paymentManageService.SetPayListStatus(siteId, payId, status, @operator);

            if (result)
            {
                ClearRedisPaySettingCache($"PaySetting:{siteId}:*", RedisDataBase.Shared);
                _ = _redisHelper.KeyDelete($"PayList:{siteId}", RedisDataBase.Payment);
            }

            return result;
        }

        public bool SetPayListLoginInfo(SetPayListLoginInfoParams input)
        {
            return _paymentManageService.SetPayListLoginInfo(input);
        }

        public IEnumerable<PaySetting> GetPaySettingList(GetPaySettingListParams input,
            int pageNumber, int pageSize, out IPageInfo pageInfo)
        {
            var (startIndex, absPageSize) = pageNumber.GetStartIndex(pageSize);
            var result = _paymentManageService.GetPaySettingList(input, absPageSize, startIndex,
                out var totalPages, out var rowCount);
            pageInfo = new PageInfo();
            pageInfo.ListPagination(rowCount, pageNumber, pageSize);

            return result;
        }

        public IEnumerable<PaySetting> GetPaySetting(int siteId, int? channelId = null)
        {
            return _paymentManageService.GetPaySetting(siteId, channelId);
        }

        public bool UpdatePaySetting(UpdatePaySettingParams input, string @operator)
        {
            var result = _paymentManageService.UpdatePaySetting(input, @operator);

            if (result)
                ClearRedisPaySettingCache($"PaySetting:{input.SiteId}:*", RedisDataBase.Shared);

            return result;
        }

        public bool UpdatePaySettingStatus(UpdatePaySettingStatusParams input, string @operator)
        {
            var result = _paymentManageService.UpdatePaySettingStatus(input, @operator);

            if (result)
                ClearRedisPaySettingCache($"PaySetting:{input.SiteId}:*", RedisDataBase.Shared);

            return result;
        }

        public bool UpdatePaySettingChargeFee(UpdatePaySettingChargeFeeParams input, string @operator)
        {
            var result = _paymentManageService.UpdatePaySettingChargeFee(input, @operator);

            return result;
        }

        public IEnumerable<BankInfoPayForSetting> GetBankInfoPayForSetting(int siteId, int currencyId, int channelId)
        {
            return _paymentManageService.GetBankInfoPayForSetting(siteId, currencyId, channelId);
        }

        public bool UpdateBankInfo(int siteId, int payId, int sysId,
            BankInfoPayStatusEnum status, int channelId, string @operator)
        {
            var result = _paymentManageService.UpdateBankInfo(siteId, payId, sysId, status, channelId, @operator);

            if (result)
            {
                ClearRedisPaySettingCache("PaySetting:OnlineBanking:*", RedisDataBase.Shared);
            }

            return result;
        }

        public IEnumerable<CryptoInfo> GetCryptoInfoList(int siteId)
            => _paymentManageService.GetCryptoInfoList(siteId);

        public bool SetCryptoInfoStatus(int siteId, CryptoCurrencyEnum currencyId, string network, GeneralStatusEnum status, string @operator)
        {
            var result = _paymentManageService.SetCryptoStatus(siteId, currencyId, network, status, @operator);

            if (result)
            {
                _ = _redisHelper.KeyDelete($"PaySetting:SupportCrypto:{siteId}", RedisDataBase.Member);
            }

            return result;
        }

        private void ClearRedisPaySettingCache(string pattern, RedisDataBase db)
        {
            _ = Task.Run(() => _redisHelper.KeyDeleteByPattern(pattern, db))
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<PayGroup>> GetPayGroupWithCache()
        {
            var dbData = await _redisHelper.TryGetOrCreateAsync("PayGroupInfoList", TimeSpan.FromMinutes(5),
                () => _paymentManageService.GetPayGroup(), RedisDataBase.Shared);
            return dbData;
        }
        
        public IEnumerable<WithdrawalBlockSetting> GetWithdrawalBlockSettingList(int siteId)
            => _paymentManageService.GetWithdrawalBlockSettingList(siteId);
        
        public bool SetWithdrawalBlockSetting(SetWithdrawalBlockSettingParams input)
        {
            var result = _paymentManageService.SetWithdrawalBlockSetting(input);

            if (result)
            {
                _ = _redisHelper.KeyDelete($"PaySetting:WithdrawalBlock:{input.SiteId}", RedisDataBase.Member);
            }
            
            return result;
        }
    }
}
