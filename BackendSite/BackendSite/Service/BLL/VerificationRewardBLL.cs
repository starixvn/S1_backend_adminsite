using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Service.BLL
{
    public class VerificationRewardBLL
    {
        private readonly VerificationRewardService _verificationRewardService;
        private readonly AdjustmentInfoLogService _adjustmentInfoLogService;

        public VerificationRewardBLL(IServiceProvider provider)
        {
            _verificationRewardService = provider.GetService<VerificationRewardService>();
            _adjustmentInfoLogService = provider.GetService<AdjustmentInfoLogService>();
        }
        public async Task<VerificationRewardModel> GetVerificationReward(int siteId, int currencyId)
        {
            return await _verificationRewardService.GetVerificationReward(siteId, currencyId);
        }
        
        public async Task<int> SetVerificationReward(int siteId, int currencyId, int verificationType, decimal phoneReward, decimal emailReward, string updateOperator)
        {
            var result = await _verificationRewardService.InsertOrUpdate(siteId, currencyId, verificationType, phoneReward, emailReward, updateOperator);
            if (result == 1 || result == 2)
            {
                var para = new
                {
                    siteId,
                    currencyId,
                    verificationType,
                    phoneReward,
                    emailReward
                };
                _adjustmentInfoLogService.InsertAdjustmentInfoLog("VerificationReward", 1, para, updateOperator);
            }

            return result;
        }
        
        public async Task<int> UpdateStatus(int siteId, int currencyId, int status, string updateOperator)
        {
            var result = await _verificationRewardService.UpdateStatus(siteId, currencyId, status, updateOperator);
            if (result == 1)
            {
                var para = new
                {
                    siteId,
                    currencyId,
                    status,
                };
                _adjustmentInfoLogService.InsertAdjustmentInfoLog("VerificationReward", 1, para, updateOperator);
            }
            return result;
        }
    }
}