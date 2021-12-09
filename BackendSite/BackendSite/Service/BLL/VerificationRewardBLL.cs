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
        public async Task<IEnumerable<VerificationRewardModel>> GetVerificationRewardSetting(int siteId)
        {
            var verificationRewards = await _verificationRewardService.GetVerificationRewardSetting(siteId);
            return verificationRewards;
        }
    }
}