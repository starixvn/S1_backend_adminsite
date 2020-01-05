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

        public IEnumerable<BankDetail> BankInfoList()
        {
            var serverInfo = serverInfoService.GetModel();
            return bankDetailService.GetBankDetailList(serverInfo.SiteId);
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

        public IEnumerable<BankDetail> BankList()
        {
            var serverInfo = serverInfoService.GetModel();
            IEnumerable<BankDetail> bankDetail = bankDetailService.BankList(serverInfo.SiteId);
            //return bankDetail.Where(x => x.CurrencyId.Equals(serverInfo.ICurrencyList));
            return bankDetail;
        }

        public IEnumerable<BankInfoOff> GetBankInfoOff()
        {
            var serverInfo = serverInfoService.GetModel();
            string sBankInfoOffListKey = $"BankInfoOffList_{serverInfo.SiteId}";
            var BankInfoOffList = cache.Get<IEnumerable<BankInfoOff>>(sBankInfoOffListKey);
            if (BankInfoOffList == null)
            {
                BankInfoOffList = localBankService.GetBankAccountOff(serverInfo.SiteId);
                if (BankInfoOffList != null)
                {
                    cache.Set(sBankInfoOffListKey, BankInfoOffList, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(2)));
                }
            }

            //return bankDetail.Where(x => x.CurrencyId.Equals(serverInfo.ICurrencyList));
            return BankInfoOffList;
        }
        public IEnumerable<BankInfoPay> GetBankInfoPay()
        {
            var serverInfo = serverInfoService.GetModel();
            string sBankInfoPayListKey = $"BankInfoPayList_{serverInfo.SiteId}";
            var BankInfoOffList = cache.Get<IEnumerable<BankInfoPay>>(sBankInfoPayListKey);
            if (BankInfoOffList == null)
            {
                BankInfoOffList = localBankService.GetBankInfoPayList();
                if (BankInfoOffList != null)
                {
                    cache.Set(sBankInfoPayListKey, BankInfoOffList, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(2)));
                }
            }

            //return bankDetail.Where(x => x.CurrencyId.Equals(serverInfo.ICurrencyList));
            return BankInfoOffList;
        }
    }
}