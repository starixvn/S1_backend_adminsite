using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Controllers.ApiV2.Marketing.Bonus;
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
            string bonusCode,
            bool isSpecial,
            int pageNumber,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {
            var siteList = _serverInfoBLL.GetSiteListByLoginUser().Select(x => x.SiteId);

            return _bonusManageService.GetPromotionList(JsonConvert.SerializeObject(siteIds),
                        JsonConvert.SerializeObject(promotionTypes),
                        status,
                        startDate,
                        endDate,
                        device,
                        bonusCode,
                        JsonConvert.SerializeObject(siteList),
                        isSpecial,
                        (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                        Math.Abs(pageSize),
                        out totalPages,
                        out rowCount);
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

        public bool SetPromotion(bool isAdd, PromotionInfoReq promotion, DisplayConditionAboutVipLevel vipLevelCondition, string Operator)
        {
            promotion.LastUpdateUser = isAdd ? "" : _authorizeBLL.GetAuthData().UserName;
            var promotionDisplayConditionList = new List<PromotionDisplayConditionModel>();

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

        public bool DuplicatePromotion(string bonusCode, int siteId, string newBonusCode, string Operator, string newSpecialCode, bool isSpecial)
        {
            var expectedSpecificRow = _bonusManageService.GetPromotionSpecific(bonusCode, siteId).Count();
            var expectedRebateRow = _bonusManageService.GetPromotionRebate(bonusCode, siteId).Count();
            var expectedConditionRow = _bonusManageService.GetPromotionDisplayCondition(bonusCode, siteId).Count();
            var expectedCashBackRow = _bonusManageService.GetPromotionCashBack(bonusCode, siteId).Count();

            return _bonusManageService.DuplicatePromotionFlow(bonusCode,
                 siteId,
                 newBonusCode,
                 expectedSpecificRow,
                 expectedRebateRow,
                 expectedConditionRow,
                 expectedCashBackRow,
                 Operator,
                 newSpecialCode,
                 isSpecial);
        }

        public int SetPromotionStatus(string bonusCode, int siteId, int status)
        {
            return _bonusManageService.SetPromotionStatus(bonusCode, siteId, status, _authorizeBLL.GetAuthData().UserName);
        }

        public IEnumerable<PromoCategory> GetPromoCategories(int walletType)
        {
            return _bonusManageService.GetPromoCategories(walletType);
        }

        public IEnumerable<TpGameType> GetTpGameList()
        {
            return _bonusManageService.GetTpGameList();
        }

        public IEnumerable<GetCashBackGamerep> GetCashBackGameList(int tpId, int categoryId)
        {
            var result = new List<GetCashBackGamerep>();
            var tempResult = _bonusManageService.GetCashBackGameList(tpId, categoryId);

            //tpId == 0,WalletType is Seamless
            //tpId != 0,WalletType is Transfer
            if (tpId == 0)
            {
                //WalletType is Seamless key by tpId
                tempResult.ToList().ForEach(x =>
                {
                    result.Add(new GetCashBackGamerep { GameName = x.GameName, Key = x.TpId });
                });
            }
            else
            {
                //WalletType is Seamless key by CategoryId
                tempResult.ToList().ForEach(x =>
                {
                    result.Add(new GetCashBackGamerep { GameName = x.GameName, Key = x.CategoryId });
                });
            }

            return result;
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
            var chars = chars3;
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
            var count = num;
            var errorCount = 0;
            while (count > 0 && errorCount < 30)
            {
                var scratchCardCode = $"{RandomString(5, 2)}-{RandomString(6, 3)}";
                if (_bonusManageService.SetScratchCardCode(specialCode, scratchCardCode, lastUpdateUser) == 0)
                {
                    count--;
                }
                else
                {
                    errorCount++; //Prevent infinite loops
                }
            }

            return errorCount >= 30 ? 90 : 0;
        }
        public IEnumerable<ScratchCard> GetScratchCardList(string specialCode,
            int filterType,
            int pageNumber,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {
            var scratchCardList = _bonusManageService.GetScratchCardList(specialCode,
                filterType,
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages,
                out rowCount);

            return scratchCardList;
        }
        public IEnumerable<ScratchCardExport> GetScratchCardListExport(string specialCode, int filterType)
        {
            var scratchCardList = _bonusManageService.GetScratchCardListExport(specialCode);
            if (filterType == 1) //unused
                scratchCardList = scratchCardList.Where(x => x.Joiner == "-");
            else if (filterType == 2) //used
                scratchCardList = scratchCardList.Where(x => x.Joiner != "-");

            return scratchCardList;
        }
        public IEnumerable<LeaderboardExtraPointList> GetLeaderboardExtraPointList(IEnumerable<int> selectSite, int categoryType, int currencyId, DateTime startDate, DateTime endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return _bonusManageService.GetLeaderboardExtraPointList(selectSite, categoryType, currencyId, startDate, endDate, pageSize, ref pageNumber, out totalPages, out rowCount);
        }
        public int SetLeaderboardExtraPoint(long activeDate, int siteId, int categoryId, string userName, decimal extraPoint, string lastUpdateUser)
        {
            return _bonusManageService.SetLeaderboardExtraPoint(activeDate, siteId, categoryId, userName, extraPoint, lastUpdateUser);
        }

        public bool CheckSpecialCodeDuplicate(string specialCode)
        {
            return _bonusManageService.CheckSpecialCodeDuplicate(specialCode).ToList().Count > 0;
        }
    }
}