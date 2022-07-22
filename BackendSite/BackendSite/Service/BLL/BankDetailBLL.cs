using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BankCardManagement;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class BankDetailBLL
    {
        private readonly BankDetailService bankDetailService;
        private readonly ServerInfoService serverInfoService;
        private readonly LocalBankService localBankService;
        private readonly VipLevelService _vipLevelService;
        private readonly IMemoryCache cache;

        public BankDetailBLL(IServiceProvider provider)
        {
            bankDetailService = provider.GetRequiredService<BankDetailService>();
            serverInfoService = provider.GetRequiredService<ServerInfoService>();
            localBankService = provider.GetRequiredService<LocalBankService>();
            _vipLevelService = provider.GetRequiredService<VipLevelService>();
            cache = provider.GetRequiredService<IMemoryCache>();
        }

        public IEnumerable<BankDetail> GetLoaclBankList(IEnumerable<int> siteIds,
            IEnumerable<int> currencyIdList,
            IEnumerable<int> statusList,
            int pageNumber, int pageSize, out int totalPages, out int rowCount)
        {
            return bankDetailService.GetLoaclBankList(JsonConvert.SerializeObject(siteIds),
                JsonConvert.SerializeObject(currencyIdList),
                JsonConvert.SerializeObject(statusList),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages, out rowCount);
        }
        
        public IEnumerable<OfflineBankingSetting> GetOfflineBankingSettingList(GetOfflineBankingSettingParams input, out int totalPages, out int rowCount)
        {
            var result = bankDetailService.GetOfflineBankingSettingList(input, out totalPages, out rowCount);
            var vip = _vipLevelService.GetVipLevelSetting(input.SiteId);
            
            foreach (var item in result)
            {
                item.VipLevelNames = vip
                    .Where(x => item.VipLevels.Contains(x.Level))
                    .Select(x => x.LevelName);
            }

            return result;
        }

        public int SetBankCard(ParamsWithLog<SetBankCard> input)
        {
            return bankDetailService.SetBankCard(input);
        }

        public int SetBankCardwithIFSC(ParamsWithLog<SetBankCard> input)
        {
            return bankDetailService.SetBankCardwithIFSC(input);
        }

        public int DelBankCard(ParamsWithLog<DeleteBankCard> input)
        {
            return bankDetailService.DelBankCard(input);
        }

        public bool AddBankInfo(BankDetail bankDetail)
        {
            var serverInfo = serverInfoService.GetModel();
            return bankDetailService.AddBankDetail(serverInfo.SiteId, bankDetail);
        }

        public bool EditBankInfo(BankDetail bankDetail)
        {
            var serverInfo = serverInfoService.GetModel();
            return bankDetailService.EditBankDetail(serverInfo.SiteId, bankDetail);
        }

        public IEnumerable<BankInfoSys> GetSysBankList()
        {
            var sBankInfoSysListKey = $"SysBankList";

            var sysBankList = cache.Get<IEnumerable<BankInfoSys>>(sBankInfoSysListKey);
            if (sysBankList == null)
            {
                sysBankList = bankDetailService.GetSysBankList();
                if (sysBankList != null)
                {
                    cache.Set(sBankInfoSysListKey, sysBankList, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(10)));
                }
            }

            return sysBankList;
        }

        public IEnumerable<BankInfoOff> GetAllBankInfoOff()
        {
            var sBankInfoOffListKey = $"BankInfoOffList_All";
            var bankInfoOffList = cache.Get<IEnumerable<BankInfoOff>>(sBankInfoOffListKey);

            if (bankInfoOffList == null)
            {
                bankInfoOffList = localBankService.GetAllBankAccountOff();
                if (bankInfoOffList != null)
                {
                    cache.Set(sBankInfoOffListKey, bankInfoOffList, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(30)));
                }
            }

            return bankInfoOffList;
        }

        public IEnumerable<BankInfoOff> GetBankInfoOff(int siteId)
        {
            var sBankInfoOffListKey = $"BankInfoOffList_{siteId}";
            var BankInfoOffList = cache.Get<IEnumerable<BankInfoOff>>(sBankInfoOffListKey);
            if (BankInfoOffList == null)
            {
                BankInfoOffList = localBankService.GetBankAccountOff(siteId);
                if (BankInfoOffList != null)
                {
                    cache.Set(sBankInfoOffListKey, BankInfoOffList, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(30)));
                }
            }

            //return bankDetail.Where(x => x.CurrencyId.Equals(serverInfo.ICurrencyList));
            return BankInfoOffList;
        }
        public IEnumerable<BankInfoPay> GetBankInfoPay()
        {
            var sBankInfoPayListKey = "BankInfoPayForPayout";
            var BankInfoPayList = cache.Get<IEnumerable<BankInfoPay>>(sBankInfoPayListKey);
            if (BankInfoPayList == null)
            {
                BankInfoPayList = localBankService.GetBankInfoPayList();
                if (BankInfoPayList != null)
                {
                    cache.Set(sBankInfoPayListKey, BankInfoPayList, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(2)));
                }
            }

            //return bankDetail.Where(x => x.CurrencyId.Equals(serverInfo.ICurrencyList));
            return BankInfoPayList;
        }
        public IEnumerable<BankInfoPay> GetCryptoPay()
        {
            var sCryptoPayListKey = "CryptoPayForPayout";
            var CryptoPayList = cache.Get<IEnumerable<BankInfoPay>>(sCryptoPayListKey);
            if (CryptoPayList == null)
            {
                CryptoPayList = localBankService.GetCryptoPayList();
                if (CryptoPayList != null)
                {
                    cache.Set(sCryptoPayListKey, CryptoPayList, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(2)));
                }
            }

            //return bankDetail.Where(x => x.CurrencyId.Equals(serverInfo.ICurrencyList));
            return CryptoPayList;
        }

        public IEnumerable<BankInfoOff> GetBankAccountOffByCurrency(int currencyId)
        {
            return localBankService.GetBankAccountOffByCurrency(currencyId);
        }
    }
}