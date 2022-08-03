using System.Collections.Generic;
using System.Linq;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
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

        public void UpdateBankInfo(int PayId, int SysId, int Status, string Operator)
        {
            paymentManageService.UpdateBankInfo(PayId, SysId, Status, Operator);
        }
    }
}
