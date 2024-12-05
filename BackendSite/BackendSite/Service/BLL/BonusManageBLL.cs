using Amazon.CloudFront.Model;

using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.PromotionDisplayCondition;

using Newtonsoft.Json;
using static BackendSite.Controllers.ApiV2.Marketing.Bonus.BonusController;

namespace BackendSite.Service.BLL
{
    public class BonusManageBLL
    {
        private readonly BonusManageService _bonusManageService;
        private readonly ServerInfoBLL _serverInfoBLL;
        private readonly AuthorizeBLL _authorizeBLL;

        public BonusManageBLL(IServiceProvider provider)
        {
            _bonusManageService = provider.GetRequiredService<BonusManageService>();
            _serverInfoBLL = provider.GetRequiredService<ServerInfoBLL>();
            _authorizeBLL = provider.GetRequiredService<AuthorizeBLL>();
        }

        public IEnumerable<PromotionInfo> GetPromotionList(IEnumerable<int> siteIds,
            IEnumerable<int> currencyIds,
            IEnumerable<int> promotionTypes,
            int status,
            long startDate,
            long endDate,
            int device,
            string bonusCode,
            string scratchCardCode,
            bool isSpecial,
            int pageNumber,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {
            var siteList = JsonConvert.SerializeObject(siteIds);

            return _bonusManageService.GetPromotionList(
                        siteList,
                        currencyIds: currencyIds != null ? JsonConvert.SerializeObject(currencyIds) : "[]",
                        JsonConvert.SerializeObject(promotionTypes),
                        status,
                        startDate,
                        endDate,
                        device,
                        bonusCode,
                        siteList,
                        scratchCardCode,
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
            return _bonusManageService.SetPromotionFlow(isAdd, promotion, vipLevelCondition, Operator);
        }

        public bool DuplicatePromotion(string bonusCode, int siteId, string newBonusCode, string Operator, string newSpecialCode, bool isSpecial)
        {
            var expectedSpecificRow = _bonusManageService.GetPromotionSpecific(bonusCode, siteId).Count();
            var expectedConditionRow = _bonusManageService.GetPromotionDisplayCondition(bonusCode, siteId).Count();
            var expectedCashBackRow = _bonusManageService.GetPromotionCashBack(bonusCode, siteId).Count();

            return _bonusManageService.DuplicatePromotionFlow(bonusCode,
                 siteId,
                 newBonusCode,
                 expectedSpecificRow,
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

        public IEnumerable<PromoCategory> GetPromoCategories()
        {
            return _bonusManageService.GetPromoCategories();
        }

        public IEnumerable<GetCashBackGamerep> GetCashBackGameList(string siteIds, int currencyId, int categoryId)
        {
            var result = new List<GetCashBackGamerep>();
            var tempResult = _bonusManageService.GetCashBackGameList(siteIds, currencyId, categoryId);


            if (categoryId == (int)PromotionCategoryEnum.SBSW)
            {
                //SBSW key by CategoryId
                tempResult.ToList().ForEach(x =>
                {
                    result.Add(new GetCashBackGamerep { GameName = x.GameName, Key = x.CategoryId });
                });
            }
            else
            {
                tempResult.ToList().ForEach(x =>
                {
                    result.Add(new GetCashBackGamerep { GameName = x.GameName, Key = x.TpId });
                });

                //因 SBO Sports、 SBO Virtual Sports 的 PromoCategoryId 相同造成前端元件Bind資料會有問題
                //故HardCode 移除重複選項，只保留一組 SBO 選單內容
                if (categoryId == (int)PromotionCategoryEnum.Sports)
                {
                    result = result.Where(r => r.Key != 42).ToList();
                    result.Add(new GetCashBackGamerep { GameName = "SBO Sports", Key = 42 });
                }
            }

            return result;
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

        public bool CheckPromotionExists(string bonusCode, string specialCode)
        {
            bonusCode = (bonusCode ?? string.Empty).Trim();
            specialCode = (specialCode ?? string.Empty).Trim();
            return _bonusManageService.CheckPromotionExists(bonusCode, specialCode).ToList().Count > 0;
        }

        public IEnumerable<PromotionType> GetPromotionType(bool isPayout, bool isNew)
        {
            return _bonusManageService.GetPromotionType(isPayout, isNew);
        }

        public int SetBonusCustUsingInfo(int siteId, string bonusCode, string creator)
        {
            return _bonusManageService.SetBonusCustUsingInfo(siteId, bonusCode, creator);
        }

        public bool SetBonusCustUsingDetail(List<BonusCustUsing> list)
        {
            return _bonusManageService.SetBonusCustUsingDetail(list);
        }

        public IEnumerable<BonusCustUsingList> GetBonusCustUsingDetail(int siteId, string bonusCode)
        {
            return _bonusManageService.GetBonusCustUsingDetail(siteId, bonusCode);
        }
        public bool UploadPromotionUploadCustList(List<PromotionCustUploadList> list)
        {
            return _bonusManageService.UploadPromotionUploadCustList(list);
        }

        public IEnumerable<PromotionCustList> GetPromotionUploadCustList(int siteId, string bonusCode)
        {
            return _bonusManageService.GetPromotionUploadCustList(siteId, bonusCode);
        }
        public int DeletePromotionUploadCustList(DeletePromotionCustListRequest req)
        {
            return _bonusManageService.DeletePromotionUploadCustList(req);
        }
    }

}
