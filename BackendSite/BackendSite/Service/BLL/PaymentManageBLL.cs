using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;

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

        public void UpdatePayList(int PayId, int Status)
        {
            paymentManageService.UpdatePayList(PayId, Status);
        }

        public void UpdatePayChannel(int ChannelId, int Sort, int Status, int PointLevel)
        {
            paymentManageService.UpdatePayChannel(ChannelId, Sort, Status, PointLevel);
        }

        public void UpdatePayCurrency(int ChannelId, int CurrencyId, decimal MinAmount, decimal MaxAmount, int AmountRule, string AmountShow)
        {
            AmountShow = string.IsNullOrEmpty(AmountShow) ? null : AmountShow;
            paymentManageService.UpdatePayCurrency(ChannelId, CurrencyId, MinAmount, MaxAmount, AmountRule, AmountShow);
        }

        public void UpdatePayCurrency(int ChannelId, int CurrencyId, decimal MinAmount, decimal MaxAmount)
        {
            paymentManageService.UpdatePayCurrencyAmount(ChannelId, CurrencyId, MinAmount, MaxAmount);
        }

        public IEnumerable<PayListBankInfoSys> GetBankSysInfoList(int PayId)
        {
            return paymentManageService.GetBankSysInfoList(PayId).OrderBy(x=>x.CurrencyId);
        }

        public void UpdateBankInfo(int PayId, int SysId, int Status)
        {
            paymentManageService.UpdateBankInfo(PayId, SysId, Status);
        }
    }
}
