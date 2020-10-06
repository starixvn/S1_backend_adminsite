using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class BonusManageBLL
    {
        private readonly BonusManageService bonusManageService;
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly AuthorizeBLL authorizeBLL;

        public BonusManageBLL(BonusManageService bonusManageService, ServerInfoBLL serverInfoBLL, AuthorizeBLL authorizeBLL)
        {
            this.bonusManageService = bonusManageService;
            this.serverInfoBLL = serverInfoBLL;
            this.authorizeBLL = authorizeBLL;
        }

        public IEnumerable<PromotionInfo> GetPromotionList()
        {
            var result = bonusManageService.GetPromotionList(null);
            return result;
        }

        public PromotionInfo GetPromotion(string BonusCode, int SiteId)
        {
            return bonusManageService.GetPromotion(BonusCode, SiteId);
        }

        public int SetPromotion(bool IsAdd, PromotionInfo promotion)
        {
            promotion.LastUpdateUser = IsAdd ? "" : authorizeBLL.GetAuthData().UserName;
            if (IsAdd)
            {
                //promotion.SiteId = serverInfoBLL.GetServerInfo().SiteId;
                //promotion.CategoryId = 1;
                //promotion.Sort = 1;
                promotion.Status = 2;
            }
            
            return bonusManageService.SetPromotion(IsAdd, promotion);
        }

        public int DuplicatePromotion(string bonusCode, int siteId, string newBonusCode)
        {
            return bonusManageService.DuplicatePromotion(bonusCode, siteId, newBonusCode);
        }

        public int SetPromotionStatus(string BonusCode, int SiteId, int Status)
        {
            return bonusManageService.SetPromotionStatus(BonusCode, SiteId, Status, authorizeBLL.GetAuthData().UserName);
        }
        public IEnumerable<PromoCategory> GetPromoCategories(int WalletType)
        {
            return bonusManageService.GetPromoCategories(WalletType);
        }
        public IEnumerable<TpGameType> GetTpGameList()
        {
            return bonusManageService.GetTpGameList();
        }
    }
}
