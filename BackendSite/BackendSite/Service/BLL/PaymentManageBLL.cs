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
        public PaymentManageBLL(PaymentManageService paymentManageService)
        {
            this.paymentManageService = paymentManageService;
        }

        //public IEnumerable<PayList> GetPayInfoList()
        //{
        //    return paymentManageService.GetPayInfoList();
        //}

        public FrontEndDatas GetPayInfoList()
        {
            return new FrontEndDatas()
            {
                ChannelList = paymentManageService.GetPayChannelList(),
                CurrencyList = paymentManageService.GetPayCurrencyList(),
                PayList = paymentManageService.GetPayList()
            };
        }
    }
}
