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
        public void SetReferralStatus(int SiteId,int CurrencyId)
        {
            referralManagementService.SetReferralStatus(SiteId, CurrencyId);
        }
        public IEnumerable<ReferralCustList> GetReferralCustList(IEnumerable<int> selectSite, string userName, int currencyId, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return referralManagementService.GetReferralCustList(selectSite, userName, currencyId, pageSize, ref pageNumber, out totalPages, out rowCount);
        }
        public IEnumerable<ReferralFriend> GetRefFriendList(int custId, long? StartDate, long? EndDate, int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {
            return referralManagementService.GetRefFriendList(custId, StartDate, EndDate, PageSize, ref PageNumber, ref TotalPages, ref RowCount);
        }
    }
}
