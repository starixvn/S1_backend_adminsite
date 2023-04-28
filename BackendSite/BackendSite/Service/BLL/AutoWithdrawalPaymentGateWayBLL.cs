using BackendSite.Service.BLL.Interface;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL
{
    public class AutoWithdrawalPaymentGateWayBLL : IAutoWithdrawalPaymentGateWayBLL
    {
        public record AutoWithdrawalPaymentGateWayDto(int? PayId, int? ChannelId, string PayName, WithdrawalTypeEnum WithdrawalType);

        private readonly IWithdrawalAccountBLL _withdrawalAccountBll;
        private readonly BankDetailBLL _bankDetailBll;

        public AutoWithdrawalPaymentGateWayBLL(IWithdrawalAccountBLL withdrawalAccountBll, BankDetailBLL bankDetailBll)
        {
            _withdrawalAccountBll = withdrawalAccountBll;
            _bankDetailBll = bankDetailBll;
        }

        public AutoWithdrawalPaymentGateWayDto GetPaymentGateWay(WithdrawalRequest withdrawalData, int siteId)
        {
            var accountType = _withdrawalAccountBll.MapWithdrawalAccountType(withdrawalData.AccountType);
            switch (accountType)
            {
                case WithdrawalAccountTypeEnum.BankCard:
                    var bankInfoOnline = _bankDetailBll
                        .GetBankInfoPayList(siteId, withdrawalData.CurrencyId, withdrawalData.SysId, withdrawalData.Amount, withdrawalData.CustId)
                        .ToList();

                    var bankInfo = bankInfoOnline.FirstOrDefault();

                    if (bankInfo != null)
                    {
                        return new AutoWithdrawalPaymentGateWayDto(bankInfo.PayId, bankInfo.ChannelId, bankInfo.PayName, WithdrawalTypeEnum.OnlinePayment);
                    }

                    break;

                case WithdrawalAccountTypeEnum.Crypto:
                    var cryptoOnline = _bankDetailBll
                        .GetCryptoPayList(siteId, withdrawalData.CurrencyId, withdrawalData.ChannelName, withdrawalData.Amount, withdrawalData.CustId)
                        .ToList();

                    var cryptoInfo = cryptoOnline.FirstOrDefault();
                    if (cryptoInfo != null)
                    {
                        return new AutoWithdrawalPaymentGateWayDto(cryptoInfo.PayId, cryptoInfo.ChannelId, cryptoInfo.PayName, WithdrawalTypeEnum.CryptoPay);
                    }

                    break;

                case WithdrawalAccountTypeEnum.EWallet:
                    var eWalletOnline = _bankDetailBll
                        .GetEWalletPayList(siteId, withdrawalData.CurrencyId, withdrawalData.EWalletSysId, withdrawalData.Amount, withdrawalData.CustId)
                        .ToList();

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
