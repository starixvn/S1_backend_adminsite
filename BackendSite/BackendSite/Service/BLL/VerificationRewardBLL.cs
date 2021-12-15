using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public class VerificationRewardBLL
    {
        private readonly VerificationRewardService _verificationRewardService;

        public VerificationRewardBLL(VerificationRewardService verificationRewardService)
        {
            _verificationRewardService = verificationRewardService;
        }
        public async Task<VerificationRewardModel> GetVerificationReward(int siteId, int currencyId)
        {
            return await _verificationRewardService.GetVerificationReward(siteId, currencyId);
        }
        
        public async Task<int> SetVerificationReward(int siteId, int currencyId, int verificationType, decimal phoneReward, decimal emailReward, string updateOperator)
        {
            return await _verificationRewardService.InsertOrUpdate(siteId, currencyId, verificationType, phoneReward, emailReward, updateOperator);
        }
        
        public async Task<int> UpdateStatus(int siteId, int currencyId, int status, string updateOperator)
        {
            return await _verificationRewardService.UpdateStatus(siteId, currencyId, status, updateOperator);
        }
    }
}