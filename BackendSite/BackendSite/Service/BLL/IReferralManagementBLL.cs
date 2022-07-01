using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public interface IReferralManagementBLL
    {
        string Operator { get; set; }
        IEnumerable<ReferralList> GetReferralBySiteId(int siteId);
        Task<ReferralDetail> GetReferralDetail(int siteId, int currencyId);
        void SetReferral(List<ReferralManagement> referralList);
        Task<int> SetReferralAsync(SetReferralRequest setReferral);
        int SetReferralStatus(int siteId, int currencyId,int status);
        IEnumerable<ReferralCustList> GetReferralCustList(IEnumerable<int> selectSite, string userName, int currencyId, DateTime? startDate, DateTime? endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount);
        IEnumerable<ReferralExcelData> GetReferralCustListExport(string selectSite, string userName, int currencyId , DateTime? startDate, DateTime? endDate);
        IEnumerable<ReferralFriend> GetRefFriendList(int custId, DateTime? StartDate, DateTime? EndDate, int PageSize, int PageNumber, out int TotalPages, out int RowCount, bool byRewardTime);
    }
}