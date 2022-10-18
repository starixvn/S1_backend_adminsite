using BackendSite.Service.DAL;
using BackendSite.Service.Model.enums;
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
            var tierRedepositBonus = _adjustmentService.CheckPayoutBonusCode(request.BonusCode, request.SiteId, request.CurrencyId);
            if (tierRedepositBonus == null || !tierRedepositBonus.TierAmountList.Any())
            {
                return result;
            }
            
            //check deposit
            var checkResult = _bonusPayoutService.CheckTierRedepositBonus(request, tierRedepositBonus.PromotionType);
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

            decimal bonusAmount;
            decimal lockAmount;
            decimal rolloverAmount;
            if (tierRedepositBonus.BonusType == BonusTypeEnum.Amount)
            {
                bonusAmount = tierAmount.Bonus;
                lockAmount = tierAmount.RequiredAmount;
            }
            else
            {
                bonusAmount = Math.Ceiling(tierAmount.Bonus * checkResult.ActualDeposit / 100);
                lockAmount = bonusAmount * 100 / tierAmount.Bonus;
                if (tierAmount.MaxBonus != 0 && bonusAmount > tierAmount.MaxBonus)
                {
                    bonusAmount = tierAmount.MaxBonus;
                    lockAmount = tierAmount.MaxBonus * 100 / tierAmount.Bonus;
                }
            }
            rolloverAmount = (lockAmount + bonusAmount) * (tierRedepositBonus.FullFillType == FullFillTypeEnum.RollOver ? tierAmount.RollOver : tierAmount.WinOver);

            result.BonusAmount = bonusAmount;
            result.LockAmount = lockAmount;
            result.RolloverAmount = rolloverAmount;
            result.RollOver = tierAmount.RollOver;
            result.WinOver = tierAmount.WinOver;
            result.IsSuccess = true;
            return result;
        }
    }
}
