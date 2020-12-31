using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class ReferralManagementBLL
    {
        private readonly ReferralManagementService referralManagementService;
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly AuthorizeBLL authorizeBLL;
        public ReferralManagementBLL(ReferralManagementService referralManagementService, ServerInfoBLL serverInfoBLL, AuthorizeBLL authorizeBLL)
        {
            this.referralManagementService = referralManagementService;
            this.serverInfoBLL = serverInfoBLL;
            this.authorizeBLL = authorizeBLL;
        }
        public IEnumerable<ReferralList> GetReferralBySiteId(int SiteId)
        {
            return referralManagementService.GetReferralBySiteId(SiteId);
        }
        public void SetReferral(List<ReferralManagement> referralList)
        {
            referralManagementService.SetReferral(referralList);
        }
    }
}
