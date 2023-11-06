using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Option;

namespace BackendSite.Service.BLL
{
    public class PendingFundBLL
    {
        private readonly CourierAdapter courierAdapter;
        private readonly IConfiguration configuration;
        private readonly Tools tools;
        private readonly PendingFundService pendingFundService;
        private readonly ServerInfoService serverInfoService;
        private readonly string _tpServiceApiUrl;
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
            BonusInfoBLL bonusInfoBLL,
            ConfigBLL configBll)
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

            _tpServiceApiUrl = configBll.GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").TpService;
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

            var myUrl = $"{_tpServiceApiUrl}BonusTransfer";
            var apiResult = await courierAdapter.MyPostAsync<TransferResult, TpBonusTransfer>(myUrl, tpBonusTransfer);
            return apiResult;
        }

        public async Task<ApiResult> BonusTransferAllAsync()
        {
            var apiResult = new ApiResult();
            var errorMsg = new List<string>();
            foreach (var pendingBonus in _bonusInfoBLL.GetAllPendingBonusList())
            {
                var tpBonusTransfer = new TpBonusTransfer
                {
                    TpId = pendingBonus.TpId,
                    CustId = pendingBonus.CustId,
                    inputs = new Dictionary<string, string>()
                };

                var myUrl = $"{_tpServiceApiUrl}BonusTransfer";
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
            var pendingBonusList = _bonusInfoBLL.GetAllPendingBonusList();
            var pendingBonusIdList = _bonusInfoBLL.GetAllPendingBonusList().Select(x => x.BonusId);

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

                    var myUrl = $"{_tpServiceApiUrl}BonusTransfer";

                    apiResult = await courierAdapter.MyPostAsync<TransferResult, TpBonusTransfer>(myUrl, tpBonusTransfer);

                    if (apiResult.ErrorCode == 0)
                    {
                        result.Add(new TransferMultiResult()
                        {
                            BonusId = item,
                            Result = new ApiResult()
                            {
                                ErrorCode = (int)ErrorCodeEnum.Success,
                                Message = $"BonusId:{item}, UserName:{processItem.UserName}, Message:Transfer Successful !",
                                RawMessage = "This is TpService/BonusTransfer Success Result."

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
                                Message = $"BonusId:{item}, UserName:{processItem.UserName}, Message:{(string.IsNullOrWhiteSpace(apiResult.RawMessage) ? apiResult.Message : apiResult.RawMessage)}",
                                RawMessage = "This is TpService/BonusTransfer Error Result."
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
                            Message = $"Admin Error：BonusId:{item}, Message:Not Found.",
                            RawMessage = "This is Admin Error Result."
                        }
                    });
                }
            }

            return result;
        }
    }
}