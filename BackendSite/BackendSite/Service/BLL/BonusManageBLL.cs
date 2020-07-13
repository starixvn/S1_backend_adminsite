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
        public BonusManageBLL(BonusManageService bonusManageService)
        {
            this.bonusManageService = bonusManageService;
        }

        public IEnumerable<PromotionInfo> GetPromotionList()
        {
            var result = bonusManageService.GetPromotion(null);
            return result;
        }

        //public IEnumerable<PromotionDetails> GetPromotionDetails(string BonusCode)
        //{
            
        //}

        //public IEnumerable<PromotionSpecific> GetPromotionSpecific(string BonusCode)
        //{
            
        //}
    }
}
