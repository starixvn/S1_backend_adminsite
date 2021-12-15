using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;

namespace BackendSite.Service.BLL
{
    public class ReferralManagementBLL
    {
        private readonly ReferralManagementService referralManagementService;
     
        public ReferralManagementBLL(ReferralManagementService referralManagementService)
        {
            this.referralManagementService = referralManagementService;
        }
        public IEnumerable<ReferralList> GetReferralBySiteId(int siteId)
        {
            return referralManagementService.GetReferralBySiteId(siteId);
        }
        public void SetReferral(List<ReferralManagement> referralList)
        {
            referralManagementService.SetReferral(referralList);
        }
        public void SetReferralStatus(int siteId, int currencyId)
        {
            referralManagementService.SetReferralStatus(siteId, currencyId);
        }
        public IEnumerable<ReferralCustList> GetReferralCustList(IEnumerable<int> selectSite, string userName, int currencyId, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return referralManagementService.GetReferralCustList(selectSite, userName, currencyId, pageSize, ref pageNumber, out totalPages, out rowCount);
        }
        public IEnumerable<ReferralFriend> GetRefFriendList(int custId,
            DateTime? StartDate,
            DateTime? EndDate,
            int PageSize,
            int PageNumber,
            out int TotalPages,
            out int RowCount)
        {
            return referralManagementService.GetRefFriendList(custId,
                StartDate,
                EndDate,
                Math.Abs(PageSize),
                (Math.Abs(PageNumber) - 1) * Math.Abs(PageSize),
                out TotalPages,
                out RowCount);
        }
    }
}
