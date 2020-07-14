using BackendSite.Service.DAL;
using BackendSite.Service.Model;
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
        public BonusManageBLL(BonusManageService bonusManageService, ServerInfoBLL serverInfoBLL)
        {
            this.bonusManageService = bonusManageService;
            this.serverInfoBLL = serverInfoBLL;
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
    }
}
