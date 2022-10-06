using BackendSite.Extensions;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class PaymentManageBLL
    {
        private readonly PaymentManageService paymentManageService;

        public PaymentManageBLL(PaymentManageService paymentManageService, PermissionBLL permissionBLL, AuthorizeBLL authorizeBLL, AdminUserService adminUserService)
        {
            this.paymentManageService = paymentManageService;
        }

        public FrontEndDatas GetPayInfoList()
        {
            return new FrontEndDatas()
            {
                ChannelList = paymentManageService.GetPayChannelList(),
                CurrencyList = paymentManageService.GetPayCurrencyList(),
                PayList = paymentManageService.GetPayList()
            };
        }

        public IEnumerable<PayList> GetPayList()
        {
            return paymentManageService.GetPayList();
        }

        public IEnumerable<PayChannel> GetPayChannel()
        {
            return paymentManageService.GetPayChannelList();
        }

        public IEnumerable<PayCurrency> GetPayCurrency()
        {
            return paymentManageService.GetPayCurrencyList();
        }

        public bool UpdatePayList(ParamsWithLog<UpdatePayInfo> input)
        {
            return paymentManageService.UpdatePayList(input);
        }

        public bool UpdatePayChannel(ParamsWithLog<UpdatePayChannel> input)
        {
            return paymentManageService.UpdatePayChannel(input);
        }

        public bool UpdatePayCurrency(ParamsWithLog<UpdatePayCurrency> input)
        {
            string amountShow = null;
            if (input.After.AmountRule == AmountRuleEnum.Fixed)
            {
                amountShow = JsonConvert.SerializeObject(input.After.FixedAmountList);
            }
            return paymentManageService.UpdatePayCurrency(input, amountShow);
        }

        public bool UpdatePayCurrencyAmount(ParamsWithLog<UpdatePayCurrency> input)
        {
            return paymentManageService.UpdatePayCurrencyAmount(input);
        }

        public IEnumerable<PayListBankInfoSys> GetBankSysInfoList(int PayId)
        {
            return paymentManageService.GetBankSysInfoList(PayId).OrderBy(x=>x.CurrencyId);
        }

        public IEnumerable<PayListInfo> GetPayList(int siteId)
        {
            return paymentManageService.GetPayList(siteId);
        }
        
        public bool SetPayListStatus(int siteId, int payId, PayListStatusEnum status)
        {
            return paymentManageService.SetPayListStatus(siteId, payId, status);
        }

        public IEnumerable<PaySetting> GetPaySettingList(GetPaySettingListParams input,
            int pageNumber, int pageSize, out IPageInfo pageInfo)
        {
            var (startIndex, absPageSize) = pageNumber.GetStartIndex(pageSize);
            var result = paymentManageService.GetPaySettingList(input, absPageSize, startIndex,
                out var totalPages, out var rowCount);
            pageInfo = new PageInfo();
            pageInfo.ListPagination(rowCount, pageNumber, pageSize);

            return result;
        }
        
        public IEnumerable<PaySetting> GetPaySetting(int siteId, int channelId)
        {
            return paymentManageService.GetPaySetting(siteId, channelId);
        }

        public bool UpdatePaySetting(UpdatePaySettingParams input, string @operator)
        {
            return paymentManageService.UpdatePaySetting(input, @operator);
        }

        public bool UpdatePaySettingStatus(UpdatePaySettingStatusParams input, string @operator)
        {
            return paymentManageService.UpdatePaySettingStatus(input, @operator);
        }
        
        public IEnumerable<PayListBankInfoSys> GetBankSysInfoList(int siteId, int payId)
        {
            return paymentManageService.GetBankSysInfoList(siteId, payId)
                .OrderBy(x=>x.CurrencyId);
        }

        public void UpdateBankInfo(int PayId, int SysId, int Status, string Operator)
        {
            paymentManageService.UpdateBankInfo(PayId, SysId, Status, Operator);
        }
        
        public bool UpdateBankInfo(int siteId, int payId, int sysId, BankInfoPayStatusEnum status, string  @operator)
        {
            return paymentManageService.UpdateBankInfo(siteId, payId, sysId, status,  @operator);
        }
    }
}
