using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Newtonsoft.Json;

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

        public IEnumerable<PromotionInfo> GetPromotionList(int[] siteArray, PromotionSelect Select = null)
        {
            FilterPara filterPara = new FilterPara();
            if (Select != null)
            {
                filterPara = new FilterPara()
                {
                    SiteidFilter = Select.SiteidFilter != null ? JsonConvert.DeserializeObject<Filter[]>(Select.SiteidFilter) : null,
                    PromotionTypeFilter = Select.PromotionTypeFilter != null ? JsonConvert.DeserializeObject<Filter[]>(Select.PromotionTypeFilter) : null,
                    Status = Select.Status,
                    SiteArray = siteArray,
                    StartDate = Select.StartDate,
                    EndDate = Select.EndDate,
                    Device = Select.Device
                };
            }

            var result = bonusManageService.GetPromotionList(null, filterPara);
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

        public async Task<BonusImportResult> BonusImportAsync(BonusImportPara bonusImportPara)
        {
            return await bonusManageService.BonusImportAsync(bonusImportPara);
        }
        private static Random random = new Random();
        public static string RandomString(int length,int type)
        {
            const string chars1 = "0123456789";
            const string chars2 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string chars3 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"; 
            string chars = chars3;
            if (type == 1)
            {
                chars = chars1;
            } else if(type == 2)
            {
                chars = chars2;
            }

            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
       
        public int GenScratchCardCode(string specialCode,int num, string lastUpdateUser)
        {
            int count = num;
            int errorcount = 0;
            while (count > 0 && errorcount < 30)
            {
                string ScratchCardCode = $"{RandomString(5, 2)}-{RandomString(6, 3)}";
                if (bonusManageService.SetScratchCardCode(specialCode, ScratchCardCode, lastUpdateUser) == 0)
                {
                    count--;
                }
                else
                {
                    errorcount++; //Prevent infinite loops
                }
            }
            if(errorcount>=30)
            {
                return 90;
            }
            return 0;
        }
        public IEnumerable<ScratchCard> GetScratchCardList(string specialCode, int filterType)
        {
            var ScratchCardList = bonusManageService.GetScratchCardList(specialCode);
            if(filterType == 1) //unused
                 ScratchCardList = ScratchCardList.Where(x => x.CustId == 0);
            else if (filterType == 2) //used
                ScratchCardList = ScratchCardList.Where(x => x.CustId != 0);
            return ScratchCardList;
        }
        public IEnumerable<ScratchCardExport> GetScratchCardListExport(string specialCode, int filterType)
        {
            var ScratchCardList = bonusManageService.GetScratchCardListExport(specialCode);
            if (filterType == 1) //unused
                ScratchCardList = ScratchCardList.Where(x => x.Joiner == "-");
            else if (filterType == 2) //used
                ScratchCardList = ScratchCardList.Where(x => x.Joiner != "-");
            return ScratchCardList;
        }
    }
}
