using BackendSite.Service.BLL.Interface;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class AutoWithdrawalPaymentGateWayBLL : IAutoWithdrawalPaymentGateWayBLL
    {
        public record AutoWithdrawalPaymentGateWayDto(int? PayId, int? ChannelId, string PayName, WithdrawalTypeEnum WithdrawalType);

        private readonly IWithdrawalAccountBLL _withdrawalAccountBll;
        private readonly BankDetailBLL _bankDetailBll;
        private readonly ILogger<IAutoWithdrawalPaymentGateWayBLL> logger;

        public AutoWithdrawalPaymentGateWayBLL(IWithdrawalAccountBLL withdrawalAccountBll, BankDetailBLL bankDetailBll, ILogger<IAutoWithdrawalPaymentGateWayBLL> logger)
        {
            _withdrawalAccountBll = withdrawalAccountBll;
            _bankDetailBll = bankDetailBll;
            this.logger = logger;
        }

        public AutoWithdrawalPaymentGateWayDto GetPaymentGateWay(WithdrawalRequest withdrawalData, int siteId)
        {
            logger.LogInformation($"{withdrawalData.TransId}：GetPaymentGateWay withdrawalData：{JsonConvert.SerializeObject(withdrawalData)}, SiteId：{siteId}");

            var accountType = _withdrawalAccountBll.MapWithdrawalAccountType(withdrawalData.AccountType);

            logger.LogInformation($"{withdrawalData.TransId} accountType：{accountType}");

            switch (accountType)
            {
                case WithdrawalAccountTypeEnum.BankCard:

                    logger.LogInformation($"{withdrawalData.TransId} IS BankCard");

                    var bankInfoOnline = _bankDetailBll
                        .GetBankInfoPayList(siteId, withdrawalData.CurrencyId, withdrawalData.SysId, withdrawalData.Amount, withdrawalData.CustId)
                        .ToList();

                    logger.LogInformation($"{withdrawalData.TransId} bankInfoOnline：{JsonConvert.SerializeObject(bankInfoOnline)}");

                    var bankInfo = bankInfoOnline.FirstOrDefault();

                    if (bankInfo != null)
                    {
                        return new AutoWithdrawalPaymentGateWayDto(bankInfo.PayId, bankInfo.ChannelId, bankInfo.PayName, WithdrawalTypeEnum.OnlinePayment);
                    }

                    break;

                case WithdrawalAccountTypeEnum.Crypto:

                    logger.LogInformation($"{withdrawalData.TransId} IS Crypto");

                    var cryptoOnline = _bankDetailBll
                        .GetCryptoPayList(siteId, withdrawalData.CurrencyId, withdrawalData.ChannelName, withdrawalData.Amount, withdrawalData.CustId)
                        .ToList();

                    logger.LogInformation($"{withdrawalData.TransId} cryptoOnline：{JsonConvert.SerializeObject(cryptoOnline)}");

                    var cryptoInfo = cryptoOnline.FirstOrDefault();
                    if (cryptoInfo != null)
                    {
                        return new AutoWithdrawalPaymentGateWayDto(cryptoInfo.PayId, cryptoInfo.ChannelId, cryptoInfo.PayName, WithdrawalTypeEnum.CryptoPay);
                    }

                    break;

                case WithdrawalAccountTypeEnum.EWallet:

                    logger.LogInformation($"{withdrawalData.TransId} IS EWallet");

                    var eWalletOnline = _bankDetailBll
                        .GetEWalletPayList(siteId, withdrawalData.CurrencyId, withdrawalData.EWalletSysId, withdrawalData.Amount, withdrawalData.CustId)
                        .ToList();

                    logger.LogInformation($"{withdrawalData.TransId} eWalletOnline：{JsonConvert.SerializeObject(eWalletOnline)}");

                    var eWalletInfo = eWalletOnline.FirstOrDefault();
                    if (eWalletInfo != null)
                    {
                        return new AutoWithdrawalPaymentGateWayDto(eWalletInfo.PayId, eWalletInfo.ChannelId, eWalletInfo.PayName, WithdrawalTypeEnum.OnlinePayment);
                    }

                    break;
            }

            return null;
        }
    }
}
