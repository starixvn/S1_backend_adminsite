using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Option;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class AutoWithdrawalPaymentGateWayBLL : IAutoWithdrawalPaymentGateWayBLL
    {
        public record AutoWithdrawalPaymentGateWayDto(int? PayId, int? ChannelId, string PayName, WithdrawalTypeEnum WithdrawalType);
        private readonly IWithdrawalAccountBLL _withdrawalAccountBll;
        private readonly BankDetailBLL _bankDetailBll;
        private readonly WithdrawalBLL _withdrawalBll;
        private readonly IWithdrawalStateTransferBLL _withdrawalStateTransferBll;
        private readonly CourierAdapter _courierAdapter;
        private readonly SelLangBLL _selLangBll;
        private readonly string _payApiUrl;
        private readonly ILogger<IAutoWithdrawalPaymentGateWayBLL> _logger;

        public AutoWithdrawalPaymentGateWayBLL(IServiceProvider provider)
        {
            _withdrawalAccountBll = provider.GetRequiredService<IWithdrawalAccountBLL>();
            _bankDetailBll = provider.GetRequiredService<BankDetailBLL>();
            _withdrawalBll = provider.GetRequiredService<WithdrawalBLL>();
            _withdrawalStateTransferBll = provider.GetRequiredService<IWithdrawalStateTransferBLL>();
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
            _logger = provider.GetRequiredService<ILogger<IAutoWithdrawalPaymentGateWayBLL>>();

            var configBll = provider.GetRequiredService<ConfigBLL>();
            _payApiUrl = configBll.GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").PayService;
        }

        public AutoWithdrawalPaymentGateWayDto GetPaymentGateWay(WithdrawalRequest withdrawalData, int siteId)
        {
            _logger.LogInformation($"{withdrawalData.TransId}：GetPaymentGateWay withdrawalData：{JsonConvert.SerializeObject(withdrawalData)}, SiteId：{siteId}");

            var accountType = _withdrawalAccountBll.MapWithdrawalAccountType(withdrawalData.AccountType);

            _logger.LogInformation($"{withdrawalData.TransId} accountType：{accountType}");

            switch (accountType)
            {
                case WithdrawalAccountTypeEnum.BankCard:

                    _logger.LogInformation($"{withdrawalData.TransId} IS BankCard");

                    var bankInfoOnline = _bankDetailBll
                        .GetBankInfoPayList(siteId, withdrawalData.CurrencyId, withdrawalData.SysId, withdrawalData.Amount, withdrawalData.CustId)
                        .ToList();

                    _logger.LogInformation($"{withdrawalData.TransId} bankInfoOnline：{JsonConvert.SerializeObject(bankInfoOnline)}");

                    var bankInfo = bankInfoOnline.FirstOrDefault();

                    if (bankInfo != null)
                    {
                        return new AutoWithdrawalPaymentGateWayDto(bankInfo.PayId, bankInfo.ChannelId, bankInfo.PayName, WithdrawalTypeEnum.OnlineBanking);
                    }

                    break;

                case WithdrawalAccountTypeEnum.Crypto:

                    _logger.LogInformation($"{withdrawalData.TransId} IS Crypto");

                    var cryptoOnline = _bankDetailBll
                        .GetCryptoPayList(siteId, withdrawalData.CurrencyId, withdrawalData.ChannelName, withdrawalData.Amount, withdrawalData.CustId)
                        .ToList();

                    _logger.LogInformation($"{withdrawalData.TransId} cryptoOnline：{JsonConvert.SerializeObject(cryptoOnline)}");

                    var cryptoInfo = cryptoOnline.FirstOrDefault();
                    if (cryptoInfo != null)
                    {
                        return new AutoWithdrawalPaymentGateWayDto(cryptoInfo.PayId, cryptoInfo.ChannelId, cryptoInfo.PayName, WithdrawalTypeEnum.CryptoPay);
                    }

                    break;

                case WithdrawalAccountTypeEnum.EWallet:

                    _logger.LogInformation($"{withdrawalData.TransId} IS EWallet");

                    var eWalletOnline = _bankDetailBll
                        .GetEWalletPayList(siteId, withdrawalData.CurrencyId, withdrawalData.EWalletSysId, withdrawalData.Amount, withdrawalData.CustId)
                        .ToList();

                    _logger.LogInformation($"{withdrawalData.TransId} eWalletOnline：{JsonConvert.SerializeObject(eWalletOnline)}");

                    var eWalletInfo = eWalletOnline.FirstOrDefault();
                    if (eWalletInfo != null)
                    {
                        return new AutoWithdrawalPaymentGateWayDto(eWalletInfo.PayId, eWalletInfo.ChannelId, eWalletInfo.PayName, WithdrawalTypeEnum.EWalletPayment);
                    }

                    break;
            }

            return null;
        }

        public ApiResult AutoWithdrawalProcessWay(SendToNextRequest request, WithdrawalRequest withdrawalData, AuthData authData, ApiResult apiResult, out int withdrawConfirmResult)
        {
            var paymentGateWay = GetPaymentGateWay(withdrawalData, withdrawalData.SiteId);
            if (paymentGateWay == null)
            {
                // 找不到 paymentGateWay 就單純只做 Verify，並且 comment 失敗原因
                withdrawConfirmResult = _withdrawalStateTransferBll.WithdrawalVerify(request.TransId, "Not Found PaymentGateWay ProcessWay Set to AutoFailBackManual", authData.UserName);
                var _ = _withdrawalBll.SetWithdrawalProcessWay(request.TransId, WithdrawalInfoProcessWayEnum.AutoFailBackManual);
                return apiResult;
            }

            var withdrawalInfo = JsonConvert.DeserializeObject<WithdrawalInfo>(withdrawalData.WithdrawalInfo);
            withdrawalInfo.PayId = paymentGateWay.PayId;
            withdrawalInfo.ChannelId = paymentGateWay.ChannelId;
            request.WithdrawalType = paymentGateWay.WithdrawalType;

            // 先 Verify 後 Process
            withdrawConfirmResult = _withdrawalStateTransferBll.WithdrawalVerify(request.TransId, request.Comment, authData.UserName);
            withdrawConfirmResult = _withdrawalStateTransferBll.WithdrawalProcess(
                request.TransId,
                request.Comment,
                updater: paymentGateWay.PayName,
                authData.UserId,
                JsonConvert.SerializeObject(withdrawalInfo),
                (int)request.WithdrawalType,
                request.WithdrawalAccountingModels,
                out var custId, out var amount);

            if (withdrawConfirmResult > 0)
            {
                var createFailResult = PaymentOnlineCreate(request.TransId, apiResult, authData.UserName, "system").Result;
                if (createFailResult != null)
                {
                    // 自動出款失敗，狀態改回 AutoFailBackManual
                    var _ = _withdrawalBll.SetWithdrawalProcessWay(request.TransId, WithdrawalInfoProcessWayEnum.AutoFailBackManual);
                    return createFailResult;
                }
            }

            return apiResult;
        }

        public async Task<ApiResult> PaymentOnlineCreate(string transId, ApiResult apiResult, string boUserName, string systemOperation = "")
        {
            var apiOnlineResult = await _courierAdapter
                .MyPostAsync<object, object>($"{_payApiUrl}Withdrawal/Create", new { TransId = transId });
            
            var updater = string.IsNullOrEmpty(systemOperation)
                ? boUserName
                : systemOperation;
            
            if (apiOnlineResult == null)
            {
                _withdrawalStateTransferBll.WithdrawalVerify(transId, apiResult.Message, updater);
                apiResult.ErrorCode = 1101;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
                
                return apiResult;
            }

            if (apiOnlineResult.ErrorCode == 0)
                return null;

            _withdrawalStateTransferBll.WithdrawalVerify(transId, apiOnlineResult.Message, updater); 
            
            return apiOnlineResult;
        }
    }
}
