using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
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
        private readonly BonusInfoService bonusInfoService;
        private readonly ServerInfoBLL _serverInfoBLL;
        private readonly AuthorizeBLL _authorizeBLL;

        public PendingFundBLL(CourierAdapter courierAdapter,
            IConfiguration configuration,
            Tools tools,
            PendingFundService pendingFundService,
            ServerInfoService serverInfoService,
            WalletService walletService,
            BonusInfoService bonusInfoService,
            ServerInfoBLL serverInfoBLL,
            AuthorizeBLL authorizeBLL)
        {
            this.courierAdapter = courierAdapter;
            this.configuration = configuration;
            this.pendingFundService = pendingFundService;
            this.serverInfoService = serverInfoService;
            this.walletService = walletService;
            this.bonusInfoService = bonusInfoService;
            _serverInfoBLL = serverInfoBLL;
            _authorizeBLL = authorizeBLL;
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

        public class TpBonusTransfer
        {
            public int TpId { set; get; }
            public int CustId { set; get; }
            public decimal Bonus { set; get; }
            public decimal BonusAmount { set; get; }
            public decimal LockedAmount { set; get; }
            public decimal RolloverAmount { set; get; }
            public long ExpiryTime { set; get; }
            public Dictionary<string, string> inputs { set; get; }
        }

        public async Task<ApiResult<TransferResult>> BonusTransferAsync(int custId)
        {
            var apiResult = new ApiResult<TransferResult>();

            var tpBonusTransfer = new TpBonusTransfer
            {
                TpId = 1,
                CustId = custId,
                inputs = new Dictionary<string, string>()
            };

            var myUrl = $"{apiUrl}BonusTransfer";
            apiResult = await courierAdapter.MyPostAsync<TransferResult, TpBonusTransfer>(myUrl, tpBonusTransfer);

            return apiResult;
        }

        public ApiResult BonusCancel(int bonusId)
        {
            var apiResult = new ApiResult();

            var result = bonusInfoService.CancelBonusByBonusId("Pending Bonus",bonusId, out int custId);
            
            if (result > 0)
            {
                var userData = _authorizeBLL.GetAuthData();
                string Menu = "Pending Bonus/Cancel";
                Dictionary<string, dynamic> Info = new Dictionary<string, dynamic>();
                Info.Add("MenuName", "Customer Info");
                Info.Add("Action", "Cancel Bonus");
                _serverInfoBLL.AdminOperateLog(null, custId, Menu, ActionType.Update, Info, userData.UserName);
                apiResult.ErrorCode = 0;
            }

            return apiResult;
        }

        public async Task<ApiResult> BonusTransferAllAsync()
        {
            var apiResult = new ApiResult();
            List<string> errorMsg = new List<string>();
            foreach (var pendingBonus in bonusInfoService.GetPendingBonusList())
            {
                var tpBonusTransfer = new TpBonusTransfer
                {
                    TpId = 1,
                    CustId = pendingBonus.CustId,
                    inputs = new Dictionary<string, string>()
                };

                var myUrl = $"{apiUrl}BonusTransfer";
                apiResult = await courierAdapter.MyPostAsync<TransferResult, TpBonusTransfer>(myUrl, tpBonusTransfer);

                if (apiResult.ErrorCode != 0)
                    errorMsg.Add($"{pendingBonus.UserName}: {apiResult.RawMessage}");
            }

            apiResult.Message = string.Join("\n\r", errorMsg);

            return apiResult;
        }
    }
}