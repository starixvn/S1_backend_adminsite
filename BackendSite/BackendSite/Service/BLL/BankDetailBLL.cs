using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BankCardManagement;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class BankDetailBLL
    {
        private readonly BankDetailService bankDetailService;
        private readonly LocalBankService localBankService;
        private readonly VipLevelService _vipLevelService;
        private readonly CustInfoBLL _custInfoBll;
        private readonly ICacheHelper _redisHelper;

        public BankDetailBLL(IServiceProvider provider)
        {
            bankDetailService = provider.GetRequiredService<BankDetailService>();
            localBankService = provider.GetRequiredService<LocalBankService>();
            _vipLevelService = provider.GetRequiredService<VipLevelService>();
            _redisHelper = provider.GetRequiredService<ICacheHelper>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
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
        
        public async Task<(IEnumerable<OfflineBankingSetting> Datas, int TotalPages, int RowCount)> GetOfflineBankingSettingList
            (GetOfflineBankingSettingParams input)
        {
            var result = await bankDetailService.GetOfflineBankingSettingListAsync(input);
            var vip = await _vipLevelService.GetVipLevelSettingAsync(input.SiteId);
            
            foreach (var item in result.Datas)
            {
                item.VipLevelNames = vip
                    .Where(x => item.VipLevels.Contains(x.Level))
                    .Select(x => x.LevelName);
            }

            return result;
        }
        
        public async Task<OfflineBankingSetting> GetOfflineBankingSettingByIdAsync(int id)
        {
            var result = await bankDetailService.GetOfflineBankingSettingByIdAsync(id);
            
            return result;
        }
        
        public async Task<bool> SetOfflineBankingSettingAsync(SetOfflineBankingSettingParams input)
        {
            var result = await bankDetailService.SetOfflineBankingSettingAsync(input);
            
            return result > 0;
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

        public IEnumerable<BankInfoSys> GetSysBankList()
        {
            var sBankInfoSysListKey = $"SysBankList";

            return _redisHelper.TryGetOrCreate(sBankInfoSysListKey, TimeSpan.FromHours(10),
                () => bankDetailService.GetSysBankList());
        }
        
        public IEnumerable<BankInfoSys> GetSysBankList(int currencyId)
        {
            var sysBankList = GetSysBankList().Where(x => x.CurrencyId == currencyId);

            return sysBankList;
        }
        
        /// <summary>
        /// 取代 GetAllBankInfoOff，GetBankInfoOff，GetBankAccountOffByCurrency
        /// </summary>
        public IEnumerable<OfflineBankAccount> GetOfflineBankAccount(int siteId = 0, int currencyId = 0)
        {
            var key = $"OfflineBankAccount:{siteId}:{currencyId}";
            
            return _redisHelper.TryGetOrCreate(key, TimeSpan.FromSeconds(30),
                () => localBankService.GetOfflineBankAccount(siteId, currencyId));
        }
        
        public IEnumerable<PayInfo> GetBankInfoPayList(int siteId, int currencyId, int sysId, decimal? amount, int? custId)
        {
            var pointLevel = -1;
            if (custId != null)
            {
                pointLevel = _custInfoBll.GetPointLevelWithCache(custId.Value);
            }
            
            var result = localBankService.GetBankInfoPayList(siteId)
                .Where(x => x.SysId == sysId
                            && x.CurrencyId == currencyId
                            && (amount == null || (x.MaxAmount == 0 || amount <= x.MaxAmount) && amount >= x.MinAmount)
                            && (pointLevel == -1 || pointLevel >= x.PointLevel))
                .GroupBy(x => x.PayId)
                .Select(g =>
                {
                    var d = g.First();
                    return new PayInfo
                    {
                        PayId = g.Key,
                        PayName = d.PayName,
                        ChannelId = d.ChannelId,
                        LoginUrl = d.LoginUrl,
                        LoginAccount = d.LoginAccount,
                        LoginPassword = d.LoginPassword,
                        LoginPin = d.TransferPin,
                        TransferPin = d.TransferPin
                    };
                });
            
            return result;
        }
        
        public IEnumerable<BankInfoPay> GetBankInfoPay(int siteId)
        {
            var bankInfoPayList = localBankService.GetBankInfoPayList(siteId);
            return bankInfoPayList;
        }
        
        public IEnumerable<EWalletPay> GetEWalletPayList()
        {
            var eWalletPayList = localBankService.GetEWalletPayList();
            return eWalletPayList;
        }
        
        public IEnumerable<Pay> GetCryptoPayList(int siteId, int currencyId, string channelName, decimal? amount)
        {
            var result = localBankService.GetCryptoPayList(siteId)
                .Where(x => x.ChannelName.Equals(channelName, StringComparison.OrdinalIgnoreCase)
                            && x.CurrencyId == currencyId
                            && (amount == null || (x.MaxAmount == 0 || amount <= x.MaxAmount) && amount >= x.MinAmount))
                .GroupBy(x => x.PayId)
                .Select(g =>
                {
                    var d = g.First();
                    return new Pay
                    {
                        PayId = g.Key,
                        PayName = d.PayName,
                        ChannelId = d.ChannelId
                    };
                });
            
            return result;
        }

        public IEnumerable<BankInfoOff> GetBankAccountOffByCurrency(int currencyId)
        {
            return localBankService.GetBankAccountOffByCurrency(currencyId);
        }
    }
}
