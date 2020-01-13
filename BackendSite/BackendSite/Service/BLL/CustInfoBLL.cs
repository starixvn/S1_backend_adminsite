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

        public CustInfoBLL(IMemoryCache cache, CustInfoService custInfoService, ServerInfoBLL serverInfoBLL)
        {
            this.custInfoService = custInfoService;
            this.serverInfoBLL = serverInfoBLL;
            this.cache = cache;
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
        public IEnumerable<dynamic> GetCustInfoList(string userName, string transId, int pageSize, ref int pageNumber, out int totalPages)
        {
            var serverInfo = serverInfoBLL.GetServerInfo();
            return custInfoService.GetCustInfoList(userName, transId, serverInfo.SiteId, pageSize, ref pageNumber, out totalPages);
        }

        public CustInfoDetail GetCustInfoDetail(int custId, string userName)
        {
            return custInfoService.GetCustInfoDetail(custId, userName);
        }
    }
}
