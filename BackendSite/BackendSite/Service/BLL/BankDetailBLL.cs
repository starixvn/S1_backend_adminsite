using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
            var results = bankDetailService.GetBankDetailList(serverInfo.SiteId);
            return results;
        }

        public IEnumerable<BankDetail> AddBankInfo(BankDetail bankDetail)
        {
            var serverInfo = serverInfoService.GetModel();
            var results = bankDetailService.AddBankDetail(serverInfo.SiteId, bankDetail);
            return results;
        }

        public IEnumerable<BankDetail> EditBankInfo(BankDetail bankDetail)
        {
            var serverInfo = serverInfoService.GetModel();
            var results = bankDetailService.EditBankDetail(serverInfo.SiteId, bankDetail);
            return results;
        }

        public IEnumerable<BankDetail> DisableBankInfo(BankDetail bankDetail, int direction)
        {
            var serverInfo = serverInfoService.GetModel();
            var results = bankDetailService.DisableBankDetail(serverInfo.SiteId, bankDetail, direction);
            return results;
        }
    }
}