using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using MySql.Data.MySqlClient;

namespace BackendSite.Service.BLL
{
    public class BankDetailBLL
    {
        private readonly BankDetailService bankDetailService;
        private readonly ServerInfoService serverInfoService;
        private readonly LocalBankService localBankService;
        private readonly IMemoryCache cache;

        public BankDetailBLL(IMemoryCache cache, BankDetailService bankDetailService, ServerInfoService serverInfoService, LocalBankService localBankService)
        {
            this.bankDetailService = bankDetailService;
            this.serverInfoService = serverInfoService;
            this.localBankService = localBankService;
            this.cache = cache;
        }

        public IEnumerable<BankDetail> GetLoaclBankList(int siteId)
        {
            return bankDetailService.GetLoaclBankList(siteId);
        }

        public int SetBankCard(int siteId, int SysId, string BankAccountName, string BankAccountNo, decimal MinAmount, decimal MaxAmount, decimal DailyLimitAmount, int PointLevel, int Purpose, int Status, int Sort)
        {
            return bankDetailService.SetBankCard(siteId, SysId, BankAccountName, BankAccountNo, MinAmount, MaxAmount, DailyLimitAmount, PointLevel, Purpose, Status, Sort);
        }
        public int SetBankCardwithIFSC(int siteId, int SysId, string BankAccountName, string BankAccountNo, string IFSC, decimal MinAmount, decimal MaxAmount, decimal DailyLimitAmount, int PointLevel, int Purpose, int Status, int Sort)
        {
            return bankDetailService.SetBankCardwithIFSC(siteId, SysId, BankAccountName, BankAccountNo, IFSC, MinAmount, MaxAmount, DailyLimitAmount, PointLevel, Purpose, Status, Sort);

        }
        public int DelBankCard(int siteId, int bankId)
        {
            return bankDetailService.DelBankCard(siteId, bankId);
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
            string sBankInfoSysListKey = $"SysBankList";

            var sysBankList = cache.Get<IEnumerable<BankInfoSys>>(sBankInfoSysListKey);
            if (sysBankList == null)
            {
                sysBankList = bankDetailService.GetSysBankList();
                if (sysBankList != null)
                {
                    cache.Set(sBankInfoSysListKey, sysBankList, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(10)));
                }
            }

            //return bankDetail.Where(x => x.CurrencyId.Equals(serverInfo.ICurrencyList));
            return sysBankList;
        }

        public IEnumerable<BankInfoOff> GetAllBankInfoOff()
        {
            string sBankInfoOffListKey = $"BankInfoOffList_All";
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
            string sBankInfoOffListKey = $"BankInfoOffList_{siteId}";
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
            string sBankInfoPayListKey = "BankInfoPayForPayout";
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
            string sCryptoPayListKey = "CryptoPayForPayout";
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