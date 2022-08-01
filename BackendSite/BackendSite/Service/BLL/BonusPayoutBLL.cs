using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
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
            //get bonus detail
            var tierRedepositBonus = _adjustmentService.CheckBalanceAdjustBonusCode(request.BonusCode, request.PromotionType, request.SiteId, request.CurrencyId);
            if (tierRedepositBonus == null || !tierRedepositBonus.TierAmountList.Any())
            {
                return new CheckTierRedepositBonusRep
                {
                    IsSuccess = false
                };
            }
            
            //check deposit
            var checkResult = _bonusPayoutService.CheckTierRedepositBonus(request);
            if (!checkResult.IsSuccess)
            {
                return new CheckTierRedepositBonusRep
                {
                    IsSuccess = false
                };
            }

            //calculate bonus
            var actualDeposit = checkResult.ActualDeposit;
            var tierAmount = tierRedepositBonus.CalculateTierAmount(actualDeposit);

            return new CheckTierRedepositBonusRep
            {
                IsSuccess = tierAmount != null,
                ActualDeposit = actualDeposit,
                BonusAmount = tierAmount != null ? tierAmount.Bonus : 0,
                RequiredAmount = tierAmount != null ? tierAmount.RequiredAmount : 0,
            };


        }
    }
}
