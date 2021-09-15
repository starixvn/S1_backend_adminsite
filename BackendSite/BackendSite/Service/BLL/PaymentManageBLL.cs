using System.Collections.Generic;
using System.Linq;
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

        public void UpdatePayList(int PayId, int Status,string Operator)
        {
            paymentManageService.UpdatePayList(PayId, Status, Operator);
        }

        public void UpdatePayChannel(int ChannelId, int Sort, int Status, int PointLevel, string Operator)
        {
            paymentManageService.UpdatePayChannel(ChannelId, Sort, Status, PointLevel, Operator);
        }

        public void UpdatePayCurrency(int ChannelId, int CurrencyId, decimal MinAmount, decimal MaxAmount, int AmountRule, string AmountShow, string Operator)
        {
            AmountShow = string.IsNullOrEmpty(AmountShow) ? null : AmountShow;
            paymentManageService.UpdatePayCurrency(ChannelId, CurrencyId, MinAmount, MaxAmount, AmountRule, AmountShow, Operator);
        }

        public void UpdatePayCurrency(int ChannelId, int CurrencyId, decimal MinAmount, decimal MaxAmount, string Operator)
        {
            paymentManageService.UpdatePayCurrencyAmount(ChannelId, CurrencyId, MinAmount, MaxAmount, Operator);
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
