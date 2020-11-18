using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RebateManageController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly RebateManageBLL rebateManageBLL;
        private readonly AuthorizeBLL authorizeBLL;

        public RebateManageController(IConfiguration configuration, RebateManageBLL rebateManageBLL, AuthorizeBLL authorizeBLL)
        {
            this.configuration = configuration;
            this.rebateManageBLL = rebateManageBLL;
            this.authorizeBLL = authorizeBLL;
        }

        public class GetRebateActionTimeReq
        {
            public int siteId { get; set; }
        }
        [HttpPost]
        public ApiResult<IEnumerable<RebateActionTime>> GetRebateActionTime(GetRebateActionTimeReq getRebateActionTimeReq)
        {
            var apiResult = new ApiResult<IEnumerable<RebateActionTime>>();
            try
            {
                apiResult.ErrorCode = 0;
                apiResult.Data = rebateManageBLL.GetRebateActionTime(getRebateActionTimeReq.siteId);
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }
        public class SetRebateActionTimeReq
        {
            public int SiteId { get; set; }
            public int PeriodType { get; set; }
            public int? PeriodDay { get; set; }
            public int RedeemHours { get; set; }
        }
        public ApiResult SetRebateActionTime(SetRebateActionTimeReq setRebateActionTimeReq)
        {
            var apiResult = new ApiResult();
            try
            {
                var authData = authorizeBLL.GetAuthData();
                rebateManageBLL.SetRebateActionTime(setRebateActionTimeReq.SiteId, setRebateActionTimeReq.PeriodType, setRebateActionTimeReq.PeriodDay, setRebateActionTimeReq.RedeemHours, authData.UserName);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }
        public class DelRebateActionTimeReq
        {
            public int siteId { get; set; }
        }
        public ApiResult DelRebateActionTime(DelRebateActionTimeReq delRebateActionTimeReq)
        {
            var apiResult = new ApiResult();
            try
            {
                rebateManageBLL.DelRebateActionTime(delRebateActionTimeReq.siteId);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }
        public class SetRebatePayoutLimitReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int GroupID { get; set; }
            public int RollOver { get; set; }
            public decimal MaxPayout { get; set; }
            public decimal MinPayout { get; set; }
        }
        public ApiResult SetRebatePayoutLimit(SetRebatePayoutLimitReq setRebatePayoutLimitReq)
        {
            var apiResult = new ApiResult();
            try
            {
                var authData = authorizeBLL.GetAuthData();
                rebateManageBLL.SetRebatePayoutLimit(setRebatePayoutLimitReq.SiteId, setRebatePayoutLimitReq.CurrencyId, setRebatePayoutLimitReq.GroupID, setRebatePayoutLimitReq.RollOver, setRebatePayoutLimitReq.MaxPayout, setRebatePayoutLimitReq.MinPayout, authData.UserName);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }
        public class SetRebatePayoutLimitBatchReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public string GroupPayout { get; set; }
        }
        public ApiResult SetRebatePayoutLimitBatch(SetRebatePayoutLimitBatchReq setRebatePayoutLimitBatchReq)
        {
            var apiResult = new ApiResult();
            try
            {
                var authData = authorizeBLL.GetAuthData();
                rebateManageBLL.SetRebatePayoutLimitbatch(setRebatePayoutLimitBatchReq.SiteId, setRebatePayoutLimitBatchReq.CurrencyId, setRebatePayoutLimitBatchReq.GroupPayout, authData.UserName);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }
        public class DelRebatePayoutLimitReq
        {
            public int siteId { get; set; }
            public int groupID { get; set; }
            public int currencyId { get; set; }
        }
        public ApiResult DelRebatePayoutLimit(DelRebatePayoutLimitReq delRebatePayoutLimitReq)
        {
            var apiResult = new ApiResult();
            try
            {
                rebateManageBLL.DelRebatePayoutLimit(delRebatePayoutLimitReq.siteId, delRebatePayoutLimitReq.groupID, delRebatePayoutLimitReq.currencyId);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }
        public class GetRebatePayoutLimitReq
        {
            public int siteId { get; set; }
            public int currencyId { get; set; }
        }
        public ApiResult<IEnumerable<RebatePayoutLimit>> GetRebatePayoutLimit(GetRebatePayoutLimitReq getRebatePayoutLimitReq)
        {
            var apiResult = new ApiResult<IEnumerable<RebatePayoutLimit>>();
            try
            {
                apiResult.ErrorCode = 0;
                apiResult.Data = rebateManageBLL.GetRebatePayoutLimit(getRebatePayoutLimitReq.siteId, getRebatePayoutLimitReq.currencyId);
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }
        public class InsLevelGroupReq
        {
            public int SiteId { get; set; }
            public string GroupName { get; set; }
            public int LevelBase { get; set; }
        }
        public ApiResult InsLevelGroup(InsLevelGroupReq insLevelGroupReq)
        {
            var apiResult = new ApiResult();
            try
            {
                rebateManageBLL.InsLevelGroup(insLevelGroupReq.SiteId, insLevelGroupReq.GroupName, insLevelGroupReq.LevelBase);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }
        public class InsLevelGroupbatchReq
        {
            public int SiteId { get; set; }
            public string GroupBaseList { get; set; }
        }
        public ApiResult InsLevelGroupbatch(InsLevelGroupbatchReq insLevelGroupbatchReq)
        {
            var apiResult = new ApiResult();
            try
            {
                rebateManageBLL.InsLevelGroupbatch(insLevelGroupbatchReq.SiteId, insLevelGroupbatchReq.GroupBaseList);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }
        public class UpdateLevelGroupReq
        {
            public int GroupID { get; set; }
            public int SiteId { get; set; }
            public string GroupName { get; set; }
            public int LevelBase { get; set; }
        }
        public ApiResult UpdateLevelGroup(UpdateLevelGroupReq updateLevelGroupReq)
        {
            var apiResult = new ApiResult();
            try
            {
                rebateManageBLL.UpdateLevelGroup(updateLevelGroupReq.GroupID, updateLevelGroupReq.SiteId, updateLevelGroupReq.GroupName, updateLevelGroupReq.LevelBase);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }
        public class DelLevelGroupReq
        {
            public int GroupID { get; set; }
        }
        public ApiResult DelLevelGroup(DelLevelGroupReq delLevelGroupReq)
        {
            var apiResult = new ApiResult();
            try
            {
                rebateManageBLL.DelLevelGroup(delLevelGroupReq.GroupID);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }
        public class GetLevelGroupReq
        {
            public int SiteId { get; set; }
        }
        public ApiResult<IEnumerable<LevelGroup>> GetLevelGroup(GetLevelGroupReq getLevelGroupReq)
        {
            var apiResult = new ApiResult<IEnumerable<LevelGroup>>();
            try
            {
                apiResult.ErrorCode = 0;
                apiResult.Data = rebateManageBLL.GetLevelGroup(getLevelGroupReq.SiteId);
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }
        public class SetRebateCategoryReq
        {
            public int SiteId { get; set; }
            public int CategoryId { get; set; }
            public int GroupID { get; set; }
            public decimal Percentage { get; set; }
        }
        public ApiResult SetRebateCategory(SetRebateCategoryReq setRebateCategoryReq)
        {
            var apiResult = new ApiResult();
            try
            {
                var authData = authorizeBLL.GetAuthData();
                rebateManageBLL.SetRebateCategory(setRebateCategoryReq.SiteId, setRebateCategoryReq.CategoryId, setRebateCategoryReq.GroupID, setRebateCategoryReq.Percentage, authData.UserName);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }
        public class SetRebateCategoryBatchReq
        {
            public int SiteId { get; set; }
            public int CategoryId { get; set; }
            public string groupPercentage { get; set; }
        }
        public ApiResult SetRebateCategoryBatch(SetRebateCategoryBatchReq setRebateCategoryBatchReq)
        {
            var apiResult = new ApiResult();
            try
            {
                var authData = authorizeBLL.GetAuthData();
                rebateManageBLL.SetRebateCategoryBatch(setRebateCategoryBatchReq.SiteId, setRebateCategoryBatchReq.CategoryId, setRebateCategoryBatchReq.groupPercentage, authData.UserName);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }
        public class DelRebateCategoryReq
        {
            public int SiteId { get; set; }
            public int CategoryId { get; set; }
            public int GroupID { get; set; }
        }
        public ApiResult DelRebateCategory(DelRebateCategoryReq delRebateCategoryReq)
        {
            var apiResult = new ApiResult();
            try
            {
                rebateManageBLL.DelRebateCategory(delRebateCategoryReq.SiteId, delRebateCategoryReq.CategoryId, delRebateCategoryReq.GroupID);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }
        public class GetRebateCategoryReq
        {
            public int SiteId { get; set; }
            public int CategoryId { get; set; }
        }
        [HttpPost]
        public ApiResult<IEnumerable<RebateGroup>> GetRebateCategory(GetRebateCategoryReq getRebateCategoryReq)
        {
            var apiResult = new ApiResult<IEnumerable<RebateGroup>>();
            try
            {
                apiResult.ErrorCode = 0;
                apiResult.Data = rebateManageBLL.GetRebateCategory(getRebateCategoryReq.SiteId, getRebateCategoryReq.CategoryId);
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }
        public class SetRebateGameTypeReq
        {
            public int SiteId { get; set; }
            public int GameType { get; set; }
            public int GroupID { get; set; }
            public decimal Percentage { get; set; }
        }
        public ApiResult SetRebateGameType(SetRebateGameTypeReq setRebateGameTypeReq)
        {
            var apiResult = new ApiResult();
            try
            {
                var authData = authorizeBLL.GetAuthData();
                rebateManageBLL.SetRebateCategory(setRebateGameTypeReq.SiteId, setRebateGameTypeReq.GameType, setRebateGameTypeReq.GroupID, setRebateGameTypeReq.Percentage, authData.UserName);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }
        public class SetRebateGameTypeBatchReq
        {
            public int SiteId { get; set; }
            public int GameType { get; set; }
            public string groupPercentage { get; set; }
        }
        public ApiResult SetRebateGameTypeBatch(SetRebateGameTypeBatchReq setRebateGameTypeBatchReq)
        {
            var apiResult = new ApiResult();
            try
            {
                var authData = authorizeBLL.GetAuthData();
                rebateManageBLL.SetRebateGameTypeBatch(setRebateGameTypeBatchReq.SiteId, setRebateGameTypeBatchReq.GameType, setRebateGameTypeBatchReq.groupPercentage, authData.UserName);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }
        public class DelRebateGameTypeReq
        {
            public int SiteId { get; set; }
            public int CategoryId { get; set; }
            public int GameType { get; set; }
        }
        public ApiResult DelRebateGameType(DelRebateGameTypeReq delRebateGameTypeReq)
        {
            var apiResult = new ApiResult();
            try
            {
                rebateManageBLL.DelRebateCategory(delRebateGameTypeReq.SiteId, delRebateGameTypeReq.GameType, delRebateGameTypeReq.GroupID);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }
        public class GetRebateGameTypeReq
        {
            public int SiteId { get; set; }
            public int GameType { get; set; }
        }
        [HttpPost]
        public ApiResult<IEnumerable<RebateGroup>> GetRebateGameType(GetRebateGameTypeReq getRebateGameTypeReq)
        {
            var apiResult = new ApiResult<IEnumerable<RebateGroup>>();
            try
            {
                apiResult.ErrorCode = 0;
                apiResult.Data = rebateManageBLL.GetRebateGameType(getRebateGameTypeReq.SiteId, getRebateGameTypeReq.GameType);
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }
    }
}
