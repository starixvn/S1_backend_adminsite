using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.PromotionDisplayCondition;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class BonusManageBLL
    {
        private readonly BonusManageService _bonusManageService;
        private readonly ServerInfoBLL _serverInfoBLL;
        private readonly AuthorizeBLL _authorizeBLL;

        public BonusManageBLL(BonusManageService bonusManageService,
            ServerInfoBLL serverInfoBLL,
            AuthorizeBLL authorizeBLL)
        {
            _bonusManageService = bonusManageService;
            _serverInfoBLL = serverInfoBLL;
            _authorizeBLL = authorizeBLL;
        }

        public IEnumerable<PromotionInfo> GetPromotionList(IEnumerable<int> siteIds,
            IEnumerable<int> promotionTypes,
            int status,
            long startDate,
            long endDate,
            int device,
            string bonusCode)
        {

            var siteList = _serverInfoBLL.GetSiteListByLoginUser().Select(x => x.SiteId);

            return _bonusManageService.GetPromotionList(JsonConvert.SerializeObject(siteIds),
                        JsonConvert.SerializeObject(promotionTypes),
                        status,
                        startDate,
                        endDate,
                        device,
                        bonusCode,
                        JsonConvert.SerializeObject(siteList));
        }

        public PromotionInfo GetPromotion(string bonusCode, int siteId)
        {
            var promotion = _bonusManageService.GetPromotionFlow(bonusCode, siteId);
            
            //Convert Condition Description
            if (promotion.PromotionDisplayCondition.Any())
            {
                foreach (var condition in promotion.PromotionDisplayCondition)
                {
                    switch (condition.ConditionType)
                    {
                        case PromotionDisplayConditionType.VipLevel:
                            condition.Description =
                                JsonConvert.DeserializeObject<VipLevelCondictionModel>(condition.Description);
                            break;
                    }
                }
            }

            return promotion;
        }

        public bool SetPromotion(bool isAdd, PromotionInfo promotion, DisplayConditionAbountVipLevel vipLevelCondition,string Operator)
        {
            promotion.LastUpdateUser = isAdd ? "" : _authorizeBLL.GetAuthData().UserName;
            var promotionDisplayConditionList = new List<PromotionDisplayConditionModel>();

            //TODO:(Pending) If update,how to check expected delete row
            //PromotionSpecific/PrmotionRebate/PromotionDisplayCondiction
            // int expectedDelSpecificRow = 0;
            // int expectedDelRebateRow = 0;
            //
            // if (!isAdd)
            // {
            //     var oldPromotionSpecificList =
            //         bonusManageService.GetPromotionSpecific(promotion.BonusCode, promotion.SiteId);
            //
            //     if (promotion.PromotionType == 6 || promotion.PromotionType == 7)
            //     {
            //         var oldPromotionRebateList =
            //             bonusManageService.GetPromotionRebate(promotion.BonusCode, promotion.SiteId);
            //     }
            // }

            //Display Condition Logic Process
            if (vipLevelCondition.IsForVipLevel)
            {
                promotionDisplayConditionList.Add(new PromotionDisplayConditionModel()
                {
                    ConditionType = PromotionDisplayConditionType.VipLevel,
                    Description = new VipLevelCondictionModel() { VipLevel = vipLevelCondition.SelectedVipLevel }
                });
            }

            return _bonusManageService.SetPromotionFlow(isAdd, promotion, promotionDisplayConditionList,
                0, 0, 0, Operator, vipLevelCondition);
        }

        public bool DuplicatePromotion(string bonusCode, int siteId, string newBonusCode)
        {
            //return _bonusManageService.DuplicatePromotion(bonusCode, siteId, newBonusCode);

            var expectedSpecificRow = _bonusManageService.GetPromotionSpecific(bonusCode, siteId).Count();
            var expectedRebateRow = _bonusManageService.GetPromotionRebate(bonusCode, siteId).Count();
            var expectedConditionRow = _bonusManageService.GetPromotionDisplayCondition(bonusCode, siteId).Count();


            return _bonusManageService.DuplicatePromotionFlow(bonusCode,
                 siteId,
                 newBonusCode,
                 expectedSpecificRow,
                 expectedRebateRow,
                 expectedConditionRow);
        }

        public int SetPromotionStatus(string bonusCode, int siteId, int status)
        {
            return _bonusManageService.SetPromotionStatus(bonusCode, siteId, status,
                _authorizeBLL.GetAuthData().UserName);
        }

        public IEnumerable<PromoCategory> GetPromoCategories(int walletType)
        {
            return _bonusManageService.GetPromoCategories(walletType);
        }

        public IEnumerable<TpGameType> GetTpGameList()
        {
            return _bonusManageService.GetTpGameList();
        }

        public IEnumerable<TpGames> GetCashBackGameList(int tpId,int categoryId)
        {
            return _bonusManageService.GetCashBackGameList(tpId, categoryId);
        }

        public async Task<BonusImportResult> BonusImportAsync(BonusImportPara bonusImportPara)
        {
            return await _bonusManageService.BonusImportAsync(bonusImportPara);
        }

        private static Random random = new Random();

        public static string RandomString(int length, int type)
        {
            const string chars1 = "0123456789";
            const string chars2 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string chars3 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string chars = chars3;
            if (type == 1)
            {
                chars = chars1;
            }
            else if (type == 2)
            {
                chars = chars2;
            }

            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public int GenScratchCardCode(string specialCode, int num, string lastUpdateUser)
        {
            int count = num;
            int errorcount = 0;
            while (count > 0 && errorcount < 30)
            {
                string scratchCardCode = $"{RandomString(5, 2)}-{RandomString(6, 3)}";
                if (_bonusManageService.SetScratchCardCode(specialCode, scratchCardCode, lastUpdateUser) == 0)
                {
                    count--;
                }
                else
                {
                    errorcount++; //Prevent infinite loops
                }
            }

            if (errorcount >= 30)
            {
                return 90;
            }

            return 0;
        }
        public IEnumerable<ScratchCard> GetScratchCardList(string specialCode, int filterType)
        {
            var ScratchCardList = _bonusManageService.GetScratchCardList(specialCode);
            if(filterType == 1) //unused
                 ScratchCardList = ScratchCardList.Where(x => x.CustId == 0);
            else if (filterType == 2) //used
                ScratchCardList = ScratchCardList.Where(x => x.CustId != 0);
            return ScratchCardList;
        }
        public IEnumerable<ScratchCardExport> GetScratchCardListExport(string specialCode, int filterType)
        {
            var ScratchCardList = _bonusManageService.GetScratchCardListExport(specialCode);
            if (filterType == 1) //unused
                ScratchCardList = ScratchCardList.Where(x => x.Joiner == "-");
            else if (filterType == 2) //used
                ScratchCardList = ScratchCardList.Where(x => x.Joiner != "-");
            return ScratchCardList;
        }
    }
}