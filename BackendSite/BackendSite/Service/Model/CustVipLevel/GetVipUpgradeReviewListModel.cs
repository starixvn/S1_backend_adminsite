using System.Collections.Generic;

namespace BackendSite.Service.Model.CustVipLevel
{
    public class GetVipUpgradeReviewListModel
    {
        public List<VipUpgradeReviewDetailModel> Data { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }
}