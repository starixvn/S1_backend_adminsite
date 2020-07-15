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

        public PromotionInfo GetPromotion(string BonusCode)
        {
            return bonusManageService.GetPromotion(BonusCode);
        }

        public int SetPromotion(bool IsAdd, PromotionInfo promotion)
        {
            promotion.LastUpdateUser = IsAdd ? "" : authorizeBLL.GetAuthData().UserName;
            promotion.SiteId = IsAdd ?  serverInfoBLL.GetServerInfo().SiteId : promotion.SiteId;
            return bonusManageService.SetPromotion(IsAdd, promotion);
        }

        public int SetPromotionStatus(string BonusCode, int SiteId, int Status)
        {
            return bonusManageService.SetPromotionStatus(BonusCode, SiteId, Status, authorizeBLL.GetAuthData().UserName);
        }
    }
}
