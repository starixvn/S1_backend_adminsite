using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

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
            var bankInfoOffList = localBankService.GetAllBankAccountOff();
            return bankInfoOffList;
        }

        public IEnumerable<BankInfoOff> GetBankInfoOff(int siteId)
        {
            var bankInfoOffList = localBankService.GetBankAccountOff(siteId);
            return bankInfoOffList;
        }
        public IEnumerable<BankInfoPay> GetBankInfoPay()
        {
            var bankInfoPayList = localBankService.GetBankInfoPayList();
            return bankInfoPayList;
        }
        
        public IEnumerable<EWalletPay> GetEWalletPayList()
        {
            var eWalletPayList = localBankService.GetEWalletPayList();
            return eWalletPayList;
        }
        
        public IEnumerable<BankInfoPay> GetCryptoPay()
        {
            var cryptoPayList = localBankService.GetCryptoPayList();
            return cryptoPayList;
        }

        public IEnumerable<BankInfoOff> GetBankAccountOffByCurrency(int currencyId)
        {
            return localBankService.GetBankAccountOffByCurrency(currencyId);
        }
    }
}