using BackendSite.Attributes;
using BackendSite.Service.BLL;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers
{
    public class MarketingController : Controller
    {
        private readonly SelLangBLL selLangBLL;
        private readonly ServerInfoBLL serverInfoBLL;

        public MarketingController(SelLangBLL selLangBLL, ServerInfoBLL serverInfoBLL)
        {
            this.selLangBLL = selLangBLL;
            this.serverInfoBLL = serverInfoBLL;
        }

        public IActionResult PointLevelManagement()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/BonusInfoReport" })]
        public IActionResult BonusInfoReport()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/BonusCashBack" })]
        public IActionResult BonusCashBack()
        {
            return View();
        }
        public IActionResult BonusManage()
        {
            return View();
        }
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/RebateInfoReport" })]
        public IActionResult RebateInfoReport()
        {
            return View();
        }
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/RebateLogReport" })]
        public IActionResult RebateLogReport()
        {
            return View();
        }
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/PendingBonus" })]
        public IActionResult PendingBonus()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/NewsManage" })]
        public IActionResult NewsManage()
        {
            return View();
        }
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/BannerManage" })]
        public IActionResult BannerManage()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/BonusImport" })]
        public IActionResult BonusImport()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/ReferralManagement" })]
        public IActionResult ReferralManagement()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/ReferralReport" })]
        public IActionResult ReferralReport()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/DrawPrizeReport" })]
        public IActionResult DrawPrizeReport()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/GameCasinoManagement" })]
        public IActionResult GameCasinoManagement()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VipUpgrade" })]
        public IActionResult VipUpgrade()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/LatestHighlightVideoManagement" })]
        public IActionResult LatestHighlightVideoManagement()
        {
            return View();
        }
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VipRebateMangetment" })]
        public IActionResult VipRebateMangetment()
        {
            return View();
        }
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VipRebateInfoReport" })]
        public IActionResult VipRebateInfoReport()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/DomainManage" })]
        public IActionResult DomainManage()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/TpMaintain" })]
        public IActionResult TpMaintain()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/LeaderBoardExtraPoint" })]
        public IActionResult LeaderBoardExtraPoint()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VipLevelReview" })]
        public IActionResult VipLevelReview()
        {
            return View();
        }
        
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VerificationReward" })]
        public IActionResult VerificationReward()
        {
            return View();
        }
        
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VerificationRewardReport" })]
        public IActionResult VerificationRewardReport()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/AppLinkManagement" })]
        public IActionResult AppLinkManagement()
        {
            return View();
        }
        
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/RegistrationForMarketing" })]
        public IActionResult RegistrationForMarketing()
        {
            return View();
        }
        
         [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/SlotGamesManagement" })]
        public IActionResult SlotGamesManagement()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/CategoryManagement" })]
        public IActionResult CategoryManagement()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/HotEventManagement" })]
        public IActionResult HotEventManagement()
        {
            return View();
        }


        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/HotGamesManagement" })]
        public IActionResult HotGamesManagement()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/DeepLinkManagement" })]
        public IActionResult DeepLinkManagement()
        {
            return View();
        }
    }
}
