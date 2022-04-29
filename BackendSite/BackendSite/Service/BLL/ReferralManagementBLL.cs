using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public int SetReferral(ReferralManagement referral)
        {
            return referralManagementService.SetReferral(referral);
        }
        public void SetReferralStatus(int siteId, int currencyId,int status)
        {
            referralManagementService.SetReferralStatus(siteId, currencyId, status);
        }
        public IEnumerable<ReferralCustList> GetReferralCustList(IEnumerable<int> selectSite, string userName, int currencyId, DateTime? startDate, DateTime? endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return referralManagementService.GetReferralCustList(selectSite, userName, currencyId, startDate, endDate , pageSize, ref pageNumber, out totalPages, out rowCount);
        }
        
        public IEnumerable<ReferralExcelData> GetReferralCustListExport(string selectSite, string userName, int currencyId , DateTime? startDate, DateTime? endDate)
        {
            List<ReferralExcelData> result = new List<ReferralExcelData>();
            var referralCustList =  referralManagementService.GetReferralCustListExport(selectSite, userName, currencyId, startDate, endDate).ToList();
            referralCustList.ForEach(x =>
            {
                result.Add(new ReferralExcelData()
                {
                    SiteName = x.SiteName,
                    ReferralUser = x.ReferralUser,
                    ReferredUser = x.ReferredUser,
                    CurrencyId = x.CurrencyId,
                    Currency = x.Currency,
                    RegTime = x.RegTime,
                    RewardTime = x.RewardTime,
                    ReferralAmount = x.ReferralAmount,
                    Status = Enum.GetName(typeof(ReferralCustStatusEnum), x.Status)
                });
            });
           
            return result;
        }
        public IEnumerable<ReferralFriend> GetRefFriendList(int custId,
            DateTime? StartDate,
            DateTime? EndDate,
            int PageSize,
            int PageNumber,
            out int TotalPages,
            out int RowCount,
            bool byRewardTime)
        {
            return referralManagementService.GetRefFriendList(custId,
                StartDate,
                EndDate,
                Math.Abs(PageSize),
                (Math.Abs(PageNumber) - 1) * Math.Abs(PageSize),
                out TotalPages,
                out RowCount,
                byRewardTime);
        }
    }
}
