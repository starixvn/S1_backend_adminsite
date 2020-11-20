using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class RebateManageBLL
    {
        private readonly RebateManageService rebateManageService;

        public RebateManageBLL(RebateManageService rebateManageService)
        {
            this.rebateManageService = rebateManageService;
        }
        public IEnumerable<RebateActionTime> GetRebateActionTime(int siteId)
        {
            return rebateManageService.GetRebateActionTime(siteId);
        }
        public void SetRebateActionTime(int siteId, int periodType, int? periodDay, int redeemHours, string lastUpdateUser)
        {
             rebateManageService.SetRebateActionTime(siteId, periodType, periodDay, redeemHours, lastUpdateUser);
        }
        public void DelRebateActionTime(int siteId)
        {
             rebateManageService.DelRebateActionTime(siteId);
        }
        public void SetRebatePayoutLimit(int siteId, int currencyId, int groupID, int rollOver, decimal maxPayout, decimal minPayout, string lastUpdateUser)
        {
            rebateManageService.SetRebatePayoutLimit(siteId, currencyId, groupID, rollOver, maxPayout, minPayout, lastUpdateUser);
        }
        public void SetRebatePayoutLimitbatch(int siteId, int currencyId, string groupPayout, string lastUpdateUser)
        {
            rebateManageService.SetRebatePayoutLimitbatch( siteId, currencyId, groupPayout, lastUpdateUser);
        }
        public void DelRebatePayoutLimit(int siteId, int groupID, int currencyId)
        {
            rebateManageService.DelRebatePayoutLimit(siteId, groupID, currencyId);
        }
        public IEnumerable<RebatePayoutLimit> GetRebatePayoutLimit(int siteId, int currencyId)
        {
            return rebateManageService.GetRebatePayoutLimit(siteId, currencyId);
        }
        public void InsLevelGroup(int siteId, string groupName, int levelBase)
        {
            rebateManageService.InsLevelGroup( siteId, groupName, levelBase);
        }
        public void InsLevelGroupbatch(int siteId, string groupBaseList)
        {
            rebateManageService.InsLevelGroupbatch(siteId, groupBaseList);
        }
        public void UpdateLevelGroup(int groupID, int siteId, string groupName, int levelBase)
        {
            rebateManageService.UpdateLevelGroup( groupID, siteId, groupName, levelBase);
        }
        public void DelLevelGroup(int groupID)
        {
            rebateManageService.DelLevelGroup(groupID);
        }
        public IEnumerable<LevelGroup> GetLevelGroup(int siteId)
        {
            return rebateManageService.GetLevelGroup(siteId);
        }
        public void SetRebateCategory(int siteId, int categoryId, int groupID, decimal percentage, string lastUpdateUser)
        {
            rebateManageService.SetRebateCategory( siteId, categoryId, groupID,percentage, lastUpdateUser);
        }
        public void SetRebateCategoryBatch(int siteId, int categoryId, string groupPercentage, string lastUpdateUser)
        {
            rebateManageService.SetRebateCategoryBatch(siteId, categoryId, groupPercentage, lastUpdateUser);
           
        }
        public void DelRebateCategory(int siteId, int categoryId, int groupID)
        {
            rebateManageService.DelRebateCategory(siteId, categoryId, groupID);
        }
        public IEnumerable<RebateGroup> GetRebateCategory(int siteId, int categoryId)
        {
            return rebateManageService.GetRebateCategory(siteId, categoryId);
        }

        public void SetRebateGameType(int siteId, int gameType, int groupID, decimal percentage, string lastUpdateUser)
        {
            rebateManageService.SetRebateGameType( siteId, gameType, groupID, percentage, lastUpdateUser);
        }
        public void SetRebateGameTypeBatch(int siteId, int gameType, string groupPercentage, string lastUpdateUser)
        {
            rebateManageService.SetRebateGameTypeBatch( siteId, gameType, groupPercentage, lastUpdateUser);
        }
        public void DelRebateGameType(int siteId, int gameType, int groupID)
        {
            rebateManageService.DelRebateGameType( siteId, gameType, groupID);
        }
        public IEnumerable<RebateGroup> GetRebateGameType(int siteId, int gameType)
        {
            return rebateManageService.GetRebateGameType( siteId, gameType);
        }
    }
}
