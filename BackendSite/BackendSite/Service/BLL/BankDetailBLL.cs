
using BackendSite.Extensions;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.BankCardManagement;
using BackendSite.Service.Model.enums;

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

        public async Task<(IEnumerable<OfflineBankingSetting> Datas, IPageInfo PageInfo)> GetOfflineBankingSettingListWithPageInfo
            (GetOfflineBankingSettingParams input, int pageNumber, int pageSize)
        {
            IPageInfo pageInfo = new PageInfo();
            var result = await GetOfflineBankingSettingListWithCache(input);
            
            var (startIndex, _) = pageNumber.GetStartIndex(pageSize);
            pageInfo.SetPageInfo(result.Count, pageNumber, pageSize);
            
            var fragment = result
                .Skip(startIndex)
                .Take(pageSize);

            return (fragment, pageInfo);
        }
        
        private async Task<List<OfflineBankingSetting>> GetOfflineBankingSettingListWithCache(GetOfflineBankingSettingParams input)
        {
            var key = "LocalPaySetting:OfflineBankingSetting:" + JsonConvert.SerializeObject(input);
            
            return await _redisHelper.TryGetOrCreateAsync(key, TimeSpan.FromMinutes(10),
                () => GetOfflineBankingSettingList(input), RedisDataBase.Shared);
        }
        
        private async Task<List<OfflineBankingSetting>> GetOfflineBankingSettingList(GetOfflineBankingSettingParams input)
        {
            var ch = new[]
            {
                TransferChannelEnum.HeadOffice,
                TransferChannelEnum.Finance,
                TransferChannelEnum.MC,
                TransferChannelEnum.CryptoWallet
            };
            
            var list = await _bankDetailService.GetOfflineBankingSettingListAsync(input);
            var vip = await _vipLevelService.GetVipLevelSettingAsync(input.SiteId);
            var result = list.Where(x =>
                    (x.FinancialAccountChannel == TransferChannelEnum.LocalBank
                     || (x.AccountPurposes.Contains(TransferAccountPurposeEnum.Withdrawal) &&
                         ch.Contains(x.FinancialAccountChannel)))
                    && (input.Purposes.All(p => x.AccountPurposes.Contains(p)) ||
                        (!input.Purposes.Any() && !x.FinancialAccountPurposes.Equals("[3]")))) // 沒搜尋條件Purposes時排除只支援Transfer的卡
                .ToList();
            
            result.ForEach(item => item.VipLevelNames = vip
                .Where(x => item.VipLevels.Contains(x.Level))
                .Select(x => x.LevelName));
           
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
            var success = result > 0;

            if (success)
            {
                _ = await _redisHelper.KeyDeleteByPatternAsync("LocalPaySetting:*");
            }

            return success;
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
            const string key = "Admin:BankInfoSysList";
            
            return _redisHelper.TryGetOrCreate(key, TimeSpan.FromMinutes(5),
                () => _bankDetailService.GetBankSysInfoList());
        }
        
        public bool SetBankInfoSys(SetBankInfoSysModel input, string @operator)
        {
            var before = GetSysBankList().First(x => x.SysId == input.SysId);
            var result = _bankDetailService.SetBankSysInfo(input, before, @operator);
            _ = _redisHelper.KeyDeleteByPattern("PaySetting:*", RedisDataBase.Shared);
            _ = _redisHelper.KeyDelete("Admin:BankInfoSysList");

            return result;
        }

        public IEnumerable<BankInfoSys> GetSysBankList(int currencyId)
        {
            var sysBankList = GetSysBankList().Where(x => (int)x.CurrencyId == currencyId);

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
                var cust = _custInfoBll.GetCustInfoWithCache(custId.Value);
                siteId = cust.SiteId;
            }

            var result = _localBankService.GetBankInfoPayList(siteId)
                .Where(x => x.SysId == sysId
                            && x.CurrencyId == currencyId
                            && (amount == null || (x.MaxAmount == 0 || amount <= x.MaxAmount) && amount >= x.MinAmount)
                            && (pointLevel == -1 || pointLevel >= x.PointLevel))
                .OrderBy(x => x.SSort)
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

            var eWalletPayList = _localBankService.GetEWalletPayList(siteId)
                .Where(x => x.SysId == sysId
                             && x.CurrencyId == currencyId
                             && (amount == null || (x.MaxAmount == 0 || amount <= x.MaxAmount) && amount >= x.MinAmount)
                             && (pointLevel == -1 || pointLevel >= x.PointLevel))
                .OrderBy(x => x.SSort)
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
                .OrderBy(x => x.SSort)
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
