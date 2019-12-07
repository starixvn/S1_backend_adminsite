using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Dapper;
using MySql.Data.MySqlClient;

namespace BackendSite.Service.BLL
{
    public class BankDetailBLL
    {
        private readonly BankDetailService bankDetailService;
        private readonly ServerInfoService serverInfoService;

        public BankDetailBLL(BankDetailService bankDetailService, ServerInfoService serverInfoService)
        {
            this.bankDetailService = bankDetailService;
            this.serverInfoService = serverInfoService;
        }

        public IEnumerable<BankDetail> BankInfoList()
        {
            var serverInfo = serverInfoService.GetModel();
            return bankDetailService.GetBankDetailList(serverInfo.SiteId);
        }

        public IEnumerable<BankDetail> AddBankInfo(BankDetail bankDetail)
        {
            var serverInfo = serverInfoService.GetModel();
            return bankDetailService.AddBankDetail(serverInfo.SiteId, bankDetail);
        }

        public IEnumerable<BankDetail> EditBankInfo(BankDetail bankDetail)
        {
            var serverInfo = serverInfoService.GetModel();
            return bankDetailService.EditBankDetail(serverInfo.SiteId, bankDetail);
        }

        public IEnumerable<BankDetail> BankList()
        {
            var serverInfo = serverInfoService.GetModel();
            IEnumerable<BankDetail> bankDetail = bankDetailService.BankList(serverInfo.SiteId);
            //return bankDetail.Where(x => x.CurrencyId.Equals(serverInfo.ICurrencyList));
            return bankDetail;
        }
    }
}