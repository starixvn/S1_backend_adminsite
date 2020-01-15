using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class CustInfoBLL
    {
        private readonly CustInfoService custInfoService;
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly IMemoryCache cache;
        private readonly TpBLL tpBLL;
        private readonly WalletService walletService;

        public CustInfoBLL(IMemoryCache cache, WalletService walletService, CustInfoService custInfoService, ServerInfoBLL serverInfoBLL, TpBLL tpBLL)
        {
            this.custInfoService = custInfoService;
            this.serverInfoBLL = serverInfoBLL;
            this.cache = cache;
            this.tpBLL = tpBLL;
            this.walletService = walletService;
        }

        public Customer GetCustInfoWithCache(int custId)
        {
            string key = $"CustInfo_{custId}";
            var customer = cache.Get<Customer>(key);
            if (customer == null)
            {
                customer = custInfoService.GetCustInfo(custId);
                if (customer != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));
                    cache.Set(key, customer, cacheEntryOptions);
                }
            }

            return customer;
        }
        public int GetVipLevelWithCache(int custId)
        {
            string key = $"VipLevelCache_{custId}";
            var vipLevelCache = cache.Get<int?>(key) ?? -1;
            if (vipLevelCache != -1)
            {
                return vipLevelCache;
            }

            vipLevelCache = custInfoService.GetVipLevel(custId);
            if (vipLevelCache != 0)
            {
                cache.Set(key, vipLevelCache, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(30)));
            }

            return vipLevelCache;
        }
        public IEnumerable<CustInfoDetail> GetCustInfoList(string userName, string transId, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return custInfoService.GetCustInfoList(userName, transId, pageSize, ref pageNumber, out totalPages, out rowCount);
        }

        public async Task<CustInfoDetail> GetCustInfoDetailAsync(int custId, string userName)
        {
            var custInfoDetail = custInfoService.GetCustInfoDetail(custId, userName);
            if (custInfoDetail != null)
                custInfoDetail.tpWallet = await tpBLL.GetTpWalletAsync(custInfoDetail.CustId);
            return custInfoDetail;
        }

        public decimal GetBalance(int custId)
        {
            return walletService.GetBalance(custId);
        }
        public int SetCustStatusSwitch(int custId, int Op)
        {
            return custInfoService.SetCustStatusSwitch(custId, Op);
        }

        public CustPhotoId GetCustPhotoId(int custId)
        {
            return custInfoService.GetCustPhotoId(custId);
        }
    }
}
