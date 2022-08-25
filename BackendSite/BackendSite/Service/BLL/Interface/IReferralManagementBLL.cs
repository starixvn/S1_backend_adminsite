using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;

namespace BackendSite.Service.BLL.Interface
{
    public interface IReferralManagementBLL
    {
        string Operator { get; set; }
        IEnumerable<ReferralList> GetReferralBySiteId(int siteId);
        Task<ReferralDetail> GetReferralDetail(int siteId, int currencyId);
        void SetReferral(List<ReferralManagement> referralList);
        Task<int> SetReferralAsync(SetReferralRequest setReferral);
        int SetReferralStatus(int siteId, int currencyId,int status);
        Task<int> SetReferralStatusAsync(int siteId, int currencyId,int status);
        IEnumerable<ReferralCustList> GetReferralCustList(IEnumerable<int> selectSite, string userName, int currencyId, DateTime? startDate, DateTime? endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount);
        Task<ReferralCustListPageData> GetReferralCustList(int siteId, string userName, int currencyId, DateTime startDate, DateTime endDate, int pageSize, int pageNumber, bool isOrder, string orderByProperty, OrderByDirection orderByDirection);
        IEnumerable<ReferralExcelData> GetReferralCustListExport(string selectSite, string userName, int currencyId , DateTime? startDate, DateTime? endDate);
        IEnumerable<ReferralFriend> GetRefFriendList(int custId, DateTime? StartDate, DateTime? EndDate, int PageSize, int PageNumber, out int TotalPages, out int RowCount, bool byRewardTime);
        IEnumerable<ReferralFriendRresponse> GetRefFriendList(int custId, DateTime? StartDate, DateTime? EndDate, int PageSize, int PageNumber, out int TotalPages, out int RowCount);
    }
}