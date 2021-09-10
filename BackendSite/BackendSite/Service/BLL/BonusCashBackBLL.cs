using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System.Collections.Generic;

namespace BackendSite.Service.BLL
{
    public class BonusCashBackBLL
    {
        private readonly BonusCashBackService bonusCashBackService;
        public BonusCashBackBLL(BonusCashBackService bonusCashBackService)
        {
            this.bonusCashBackService = bonusCashBackService;
        }

        public IEnumerable<SiteTpList> GetTpList()
        {
            return bonusCashBackService.GetTpList();
        }

        public int? AddCashBackBonus(CashBackBonusBase _CashBackBonus, IEnumerable<CashBackGames> _CashBackGames, IEnumerable<CashBackMinMaxAmount> _CashBackMinMaxAmounts, int SiteId)
        {
            return bonusCashBackService.AddCashBackBonus(_CashBackBonus, _CashBackGames, _CashBackMinMaxAmounts, SiteId);
        }

        public CashBackBonus GetCashBackBonus(int SiteId)
        {
            return bonusCashBackService.GetCashBackBonus(SiteId);
        }

        public void SetCashBackBonus(CashBackBonusBase _CashBackBonus, IEnumerable<CashBackGames> _CashBackGames, IEnumerable<CashBackMinMaxAmount> _CashBackMinMaxAmounts, int SiteId)
        {
            bonusCashBackService.SetCashBackBonus(_CashBackBonus, _CashBackGames, _CashBackMinMaxAmounts, SiteId);
        }
    }
}
