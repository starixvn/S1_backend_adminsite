using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace BackendSite.Service.BLL
{
    public class PendingFundBLL
    {
        private readonly CourierAdapter courierAdapter;
        private readonly IConfiguration configuration;
        private readonly Tools tools;
        private readonly PendingFundService pendingFundService;
        private readonly ServerInfoService serverInfoService;
        private readonly string apiUrl;
        private readonly WalletService walletService;

        public PendingFundBLL(CourierAdapter courierAdapter, IConfiguration configuration, Tools tools, PendingFundService pendingFundService, ServerInfoService serverInfoService, WalletService walletService)
        {
            this.courierAdapter = courierAdapter;
            this.configuration = configuration;
            this.pendingFundService = pendingFundService;
            this.serverInfoService = serverInfoService;
            this.walletService = walletService;
            apiUrl = configuration.GetValue<string>("TpService");
        }

        public IEnumerable<PendingFund> PendingFundList()
        {
            var serverInfo = serverInfoService.GetModel();
            return pendingFundService.PendingFundList(serverInfo.SiteId);
        }

        public async Task<ApiResult<TransferResult>> ProcessAsync(PendingFund pendingFund)
        {
            var serverInfo = serverInfoService.GetModel();
            pendingFund.Balance = walletService.GetBalance(pendingFund.CustId);
            var apiResult = new ApiResult<TransferResult>();

            if (pendingFund.Balance > 0)
            {
                var tpTransfer = new TpTransfer
                {
                    TpId = 1,
                    CustId = pendingFund.CustId,
                    Amount = pendingFund.Balance,
                    direction = TpDirection.ToTp,
                    inputs = new Dictionary<string, string>()
                };
                var myUrl = $"{apiUrl}Transfer";
                apiResult = await courierAdapter.MyPostAsync<TransferResult, TpTransfer>(myUrl, tpTransfer);
            }
            else
            {
                apiResult.Message = "Balance less than 0";
            }

            return apiResult;
        }
    }
}
