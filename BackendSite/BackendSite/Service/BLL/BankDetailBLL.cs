using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BankCardManagement;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class BankDetailBLL
    {
        private readonly BankDetailService _bankDetailService;
        private readonly LocalBankService _localBankService;
        private readonly VipLevelService _vipLevelService;
        private readonly CustInfoBLL _custInfoBll;
        private readonly ICacheHelper _redisHelper;

        public BankDetailBLL(IServiceProvider provider)
        {
            _bankDetailService = provider.GetRequiredService<BankDetailService>();
            _localBankService = provider.GetRequiredService<LocalBankService>();
            _vipLevelService = provider.GetRequiredService<VipLevelService>();
            _redisHelper = provider.GetRequiredService<ICacheHelper>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
        }

        public IEnumerable<BankDetail> GetLoaclBankList(IEnumerable<int> siteIds,
            IEnumerable<int> currencyIdList,
            IEnumerable<int> statusList,
            int pageNumber, int pageSize, out int totalPages, out int rowCount)
        {
            return _bankDetailService.GetLoaclBankList(JsonConvert.SerializeObject(siteIds),
                JsonConvert.SerializeObject(currencyIdList),
                JsonConvert.SerializeObject(statusList),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages, out rowCount);
        }
        
        public async Task<(IEnumerable<OfflineBankingSetting> Datas, int TotalPages, int RowCount)> GetOfflineBankingSettingList
            (GetOfflineBankingSettingParams input)
        {
            var result = await _bankDetailService.GetOfflineBankingSettingListAsync(input);
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
            var result = await _bankDetailService.GetOfflineBankingSettingByIdAsync(id);
            
            return result;
        }
        
        public async Task<bool> SetOfflineBankingSettingAsync(SetOfflineBankingSettingParams input)
        {
            var result = await _bankDetailService.SetOfflineBankingSettingAsync(input);
            
            return result > 0;
        }

        public async Task<IEnumerable<LocalPaySetting>> GetLocalPaySettingAsync()
        {
            return await _bankDetailService.GetLocalPaySettingAsync();
        }

        public int SetBankCard(ParamsWithLog<SetBankCard> input)
        {
            return _bankDetailService.SetBankCard(input);
        }

        public int SetBankCardwithIFSC(ParamsWithLog<SetBankCard> input)
        {
            return _bankDetailService.SetBankCardwithIFSC(input);
        }

        public int DelBankCard(ParamsWithLog<DeleteBankCard> input)
        {
            return _bankDetailService.DelBankCard(input);
        }

        public IEnumerable<BankInfoSys> GetSysBankList()
        {
            var sBankInfoSysListKey = $"SysBankList";

            return _redisHelper.TryGetOrCreate(sBankInfoSysListKey, TimeSpan.FromHours(10),
                () => _bankDetailService.GetSysBankList());
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
            var key = $"OfflineBankAccount:Site{siteId}:Currency{currencyId}";
            
            return _redisHelper.TryGetOrCreate(key, TimeSpan.FromSeconds(30),
                () => _localBankService.GetOfflineBankAccount(siteId, currencyId));
        }
        
        public IEnumerable<BankCodeModel> GetBankCodeList(int siteId)
        {
            return _localBankService.GetBankCodeList(siteId);
        }
        
        public IEnumerable<PayInfo> GetBankInfoPayList(int siteId, int currencyId, int sysId, decimal? amount, int? custId)
        {
            var pointLevel = -1;
            if (custId != null)
            {
                pointLevel = _custInfoBll.GetPointLevelWithCache(custId.Value);
            }
            
            var result = _localBankService.GetBankInfoPayList(siteId)
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
            var bankInfoPayList = _localBankService.GetBankInfoPayList(siteId);
            return bankInfoPayList;
        }
        
        public IEnumerable<PayInfo> GetEWalletPayList(int siteId, int currencyId, int sysId, decimal? amount, int? custId)
        {
            var pointLevel = -1;
            if (custId != null)
            {
                pointLevel = _custInfoBll.GetPointLevelWithCache(custId.Value);
            }
            
            var eWalletPayList = localBankService.GetEWalletPayList()
                .Where(x =>  x.SysId == sysId
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
            
            return eWalletPayList;
        }
        
        public IEnumerable<PayInfo> GetCryptoPayList(int siteId, int currencyId, string channelName, decimal? amount, int? custId)
        {
            var pointLevel = -1;
            if (custId != null)
            {
                pointLevel = _custInfoBll.GetPointLevelWithCache(custId.Value);
            }
            
            var result = _localBankService.GetCryptoPayList(siteId)
                .Where(x => x.ChannelName.Equals(channelName, StringComparison.OrdinalIgnoreCase)
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
                        LoginAccount = d.LoginAccount,
                        LoginPassword = d.LoginPassword,
                        LoginPin = d.LoginPin,
                        LoginUrl = d.LoginUrl,
                        TransferPin = d.TransferPin
                    };
                });
            
            return result;
        }

        public IEnumerable<BankInfoOff> GetBankAccountOffByCurrency(int currencyId)
        {
            return _localBankService.GetBankAccountOffByCurrency(currencyId);
        }
    }
}
