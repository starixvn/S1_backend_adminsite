using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

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
        private readonly BonusInfoBLL _bonusInfoBLL;


        public PendingFundBLL(CourierAdapter courierAdapter,
            IConfiguration configuration,
            Tools tools,
            PendingFundService pendingFundService,
            ServerInfoService serverInfoService,
            WalletService walletService,
            BonusInfoService bonusInfoService,
            ServerInfoBLL serverInfoBLL,
            AuthorizeBLL authorizeBLL,
            BonusInfoBLL bonusInfoBLL)
        {
            this.courierAdapter = courierAdapter;
            this.configuration = configuration;
            this.pendingFundService = pendingFundService;
            this.serverInfoService = serverInfoService;
            this.walletService = walletService;
            this.bonusInfoService = bonusInfoService;
            _serverInfoBLL = serverInfoBLL;
            _authorizeBLL = authorizeBLL;
            _bonusInfoBLL = bonusInfoBLL;
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

        public class TransferMultiResult
        {
            public int BonusId { get; set; }

            public ApiResult Result { get; set; }
        }

        public async Task<ApiResult<TransferResult>> BonusTransferAsync(int custId, int tpId)
        {
            var tpBonusTransfer = new TpBonusTransfer
            {
                TpId = tpId,
                CustId = custId,
                inputs = new Dictionary<string, string>()
            };

            var myUrl = $"{apiUrl}BonusTransfer";
            var apiResult = await courierAdapter.MyPostAsync<TransferResult, TpBonusTransfer>(myUrl, tpBonusTransfer);
            return apiResult;
        }

        public ApiResult BonusCancel(int bonusId)
        {
            var apiResult = new ApiResult();

            var result = _bonusInfoBLL.CancelBonusByBonusId(bonusId, out var custId);
            if (result > 0)
            {
                var userData = _authorizeBLL.GetAuthData();
                var Menu = "Pending Bonus/Cancel";
                var Info = new Dictionary<string, dynamic>();
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
            var errorMsg = new List<string>();
            foreach (var pendingBonus in bonusInfoService.GetPendingBonusList(0,10000,out _,out _))
            {
                var tpBonusTransfer = new TpBonusTransfer
                {
                    TpId = pendingBonus.TpId,
                    CustId = pendingBonus.CustId,
                    inputs = new Dictionary<string, string>()
                };

                var myUrl = $"{apiUrl}BonusTransfer";
                apiResult = await courierAdapter.MyPostAsync<TransferResult, TpBonusTransfer>(myUrl, tpBonusTransfer);

                if (apiResult.ErrorCode != 0)
                    errorMsg.Add($"{pendingBonus.UserName}: {(string.IsNullOrWhiteSpace(apiResult.RawMessage) ? apiResult.Message : apiResult.RawMessage)}");
            }

            apiResult.Message = string.Join("\n\r", errorMsg);

            return apiResult;
        }

        public async Task<List<TransferMultiResult>> TransferMultiAsync(List<int> bonusIdList)
        {
            var apiResult = new ApiResult();
            var result = new List<TransferMultiResult>();
            var pendingBonusList = bonusInfoService.GetPendingBonusList(0, 10000, out _, out _);
            var pendingBonusIdList = bonusInfoService.GetPendingBonusList(0, 10000, out _, out _).Select(x => x.BonusId);

            foreach (var item in bonusIdList)
            {
                if (pendingBonusIdList.Contains(item))
                {
                    var processItem = pendingBonusList.Where(x => x.BonusId == item).FirstOrDefault();

                    var tpBonusTransfer = new TpBonusTransfer
                    {
                        TpId = processItem.TpId,
                        CustId = processItem.CustId,
                        inputs = new Dictionary<string, string>()
                    };

                    var myUrl = $"{apiUrl}BonusTransfer";

                    apiResult = await courierAdapter.MyPostAsync<TransferResult, TpBonusTransfer>(myUrl, tpBonusTransfer);

                    if (apiResult.ErrorCode == 0)
                    {
                        result.Add(new TransferMultiResult()
                        {
                            BonusId = item,
                            Result = new ApiResult()
                            {
                                ErrorCode = (int)ErrorCodeEnum.Success,
                                Message = $"BonusId:{item}, UserName:{processItem.UserName}, Message:Transfer Successful !"
                            }
                        });
                    }
                    else
                    {
                        result.Add(new TransferMultiResult()
                        {
                            BonusId = item,
                            Result = new ApiResult()
                            {
                                ErrorCode = (int)ErrorCodeEnum.Fail,
                                Message = $"BonusId:{item}, UserName:{processItem.UserName}, Message:{(string.IsNullOrWhiteSpace(apiResult.RawMessage) ? apiResult.Message : apiResult.RawMessage)}"
                            }
                        });
                    }
                }
                else
                {
                    result.Add(new TransferMultiResult()
                    {
                        BonusId = item,
                        Result = new ApiResult()
                        {
                            ErrorCode = (int)ErrorCodeEnum.Fail,
                            Message = $"BonusId:{item}, Message:Not Found."
                        }
                    });
                }
            }

            return result;
        }
    }
}