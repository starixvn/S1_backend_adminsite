using BackendSite.Service.DAL;
using static BackendSite.Controllers.ApiV2.Marketing.Bonus.BonusPayoutController;

namespace BackendSite.Service.BLL
{
    public class BonusPayoutBLL
    {
        private readonly BonusPayoutService _bonusPayoutService;
        private readonly AdjustmentService _adjustmentService;
        public BonusPayoutBLL(IServiceProvider provider)
        {
            _bonusPayoutService = provider.GetRequiredService<BonusPayoutService>();
            _adjustmentService = provider.GetRequiredService<AdjustmentService>();
        }

        public CheckTierRedepositBonusRep CheckTierRedepositBonus(CheckTierRedepositBonusReq request)
        {
            var result = new CheckTierRedepositBonusRep
            {
                IsSuccess = false,
                DepositId = request.DepositId,
            };

            //get bonus detail
            var tierRedepositBonus = _adjustmentService.CheckBalanceAdjustBonusCode(request.BonusCode, request.PromotionType, request.SiteId, request.CurrencyId);
            if (tierRedepositBonus == null || !tierRedepositBonus.TierAmountList.Any())
            {
                return result;
            }
            
            //check deposit
            var checkResult = _bonusPayoutService.CheckTierRedepositBonus(request);
            if (!checkResult.IsSuccess)
            {
                return result;
            }

            //calculate bonus
            result.ActualDeposit = checkResult.ActualDeposit;
            var tierAmount = tierRedepositBonus.CalculateTierAmount(result.ActualDeposit);
            if (tierAmount == null)
            {
                return result;
            }

            result.BonusAmount = tierAmount.Bonus;
            result.LockAmount = tierAmount.RequiredAmount;
            result.RolloverAmount = tierAmount.RequiredAmount;


            result.IsSuccess = true;
            return result;
        }
    }
}
