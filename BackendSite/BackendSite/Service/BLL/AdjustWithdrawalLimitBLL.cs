using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL
{
    public class AdjustWithdrawalLimitBLL
    {
        private readonly AdjustmentService _adjustmentService;
        private readonly ILogger<AdjustWithdrawalLimitBLL> _logger;


        public AdjustWithdrawalLimitBLL(IServiceProvider provider)
        {
            _adjustmentService = provider.GetRequiredService<AdjustmentService>();
            _logger = provider.GetRequiredService<ILogger<AdjustWithdrawalLimitBLL>>();
        }

        public WalletInfo GetCustWalletInfo(int CustId)
        {
            var walletInfo = _adjustmentService.GetWithdrawalLimitInfo(CustId);
            decimal Balance = 0;
            decimal DifferenceAmount = 0;
            decimal AvailableBalance = 0;

            if (walletInfo != null)
            {
                Balance = walletInfo.Balance;
                var tmpDeduction = walletInfo.WithdrawalLimit - walletInfo.TurnOver;
                DifferenceAmount = tmpDeduction < 0 ? 0 : tmpDeduction;
                var tmpAvailableBalance = walletInfo.Balance - (DifferenceAmount);
                AvailableBalance = tmpAvailableBalance < 0 ? 0 : walletInfo.Balance - (DifferenceAmount);
            }

            return new WalletInfo
            {
                DifferenceAmount = DifferenceAmount,
                Balance = Balance,
                AvailableBalance = AvailableBalance
            };
        }

        public IEnumerable<AdjustWithdrawalLimit> GetAdjustWithdrawalLimitsLog(int CustId,
            int pageNumber,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {
            return _adjustmentService.GetAdjustWithdrawalLimitsLog(CustId,
                Math.Abs(pageSize),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                out totalPages,
                out rowCount);
        }

        public int SetAdjustWithdrawalLimit(AdjustWithdrawalLimit withdrawalLimitInfo,
            string UserName,
            int CurrencyId,
            int SiteId,
            string Operator)
        {
            return _adjustmentService.SetAdjustWithdrawalLimit(withdrawalLimitInfo,
                UserName,
                CurrencyId,
                SiteId,
                Operator);
        }


        public (bool Success, string Message) SetAdjustWithdrawalLimitV2(SetAdjustWithdrawalLimitParams input)
        {
            var (resultCode, transId) = _adjustmentService.SetAdjustWithdrawalLimitV2(input);

            if (resultCode == (int)ErrorCodeEnum.Success)
            {
                return (true, $"TransId: {transId}");
            }

            var msg = resultCode switch
            {
                -21 => "Debit amount can not more then difference amount",
                -22 => "Insert AdjustWithdrawalLimitInfo failed.",
                -2 => "SetWithdrawalLimit failed.",
                -3 => "CreateAdminAdjustmentInfoLog failed.",
                _ => $"Unknown error, resultCode: {resultCode}"
            };

            _logger.LogError(msg);
            return (false, msg);
        }
    }
}
