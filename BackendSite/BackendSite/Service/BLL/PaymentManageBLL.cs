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

        public FrontEndDatas GetPayInfoList()
        {
            return new FrontEndDatas()
            {
                ChannelList = _paymentManageService.GetPayChannelList(),
                CurrencyList = _paymentManageService.GetPayCurrencyList(),
                PayList = _paymentManageService.GetPayList()
            };
        }

        public IEnumerable<PayChannelInfo> GetPayChannel(int siteId)
        {
            var list =_paymentManageService.GetPaySetting(siteId);

            return list.Select(x => new PayChannelInfo(x))
                .Distinct(new PayChannelInfoComparer());
        }

        public bool UpdatePayList(ParamsWithLog<UpdatePayInfo> input)
        {
            return _paymentManageService.UpdatePayList(input);
        }

        public bool UpdatePayChannel(ParamsWithLog<UpdatePayChannel> input)
        {
            return _paymentManageService.UpdatePayChannel(input);
        }

        public bool UpdatePayCurrency(ParamsWithLog<UpdatePayCurrency> input)
        {
            string amountShow = null;
            if (input.After.AmountRule == AmountRuleEnum.Fixed)
            {
                amountShow = JsonConvert.SerializeObject(input.After.FixedAmountList);
            }
            return _paymentManageService.UpdatePayCurrency(input, amountShow);
        }

        public bool UpdatePayCurrencyAmount(ParamsWithLog<UpdatePayCurrency> input)
        {
            return _paymentManageService.UpdatePayCurrencyAmount(input);
        }

        public IEnumerable<PayListBankInfoSys> GetBankSysInfoList(int PayId)
        {
            return _paymentManageService.GetBankSysInfoList(PayId).OrderBy(x=>x.CurrencyId);
        }

        public IEnumerable<PayListInfo> GetPayList(int siteId)
        {
            return _paymentManageService.GetPayList(siteId);
        }
        
        public bool SetPayListStatus(int siteId, int payId, PayListStatusEnum status)
        {
            return _paymentManageService.SetPayListStatus(siteId, payId, status);
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
            var key = $"Admin:PaySetting:{siteId}:{channelId}";
            
            return _redisHelper.TryGetOrCreate(key, TimeSpan.FromSeconds(30),
                () => _paymentManageService.GetPaySetting(siteId, channelId));
        }

        public bool UpdatePaySetting(UpdatePaySettingParams input, string @operator)
        {
            return _paymentManageService.UpdatePaySetting(input, @operator);
        }

        public bool UpdatePaySettingStatus(UpdatePaySettingStatusParams input, string @operator)
        {
            return _paymentManageService.UpdatePaySettingStatus(input, @operator);
        }
        
        public IEnumerable<PayListBankInfoSys> GetBankSysInfoList(int siteId, int payId)
        {
            return _paymentManageService.GetBankSysInfoList(siteId, payId)
                .OrderBy(x=>x.CurrencyId);
        }

        public void UpdateBankInfo(int PayId, int SysId, int Status, string Operator)
        {
            _paymentManageService.UpdateBankInfo(PayId, SysId, Status, Operator);
        }
        
        public bool UpdateBankInfo(int siteId, int payId, int sysId, BankInfoPayStatusEnum status, string  @operator)
        {
            return _paymentManageService.UpdateBankInfo(siteId, payId, sysId, status,  @operator);
        }
    }
}
