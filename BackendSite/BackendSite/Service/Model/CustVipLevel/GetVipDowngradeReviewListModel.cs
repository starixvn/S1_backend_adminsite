using System.Collections.Generic;

namespace BackendSite.Service.Model.CustVipLevel
{
    public class GetVipDowngradeReviewListModel
    {
        public List<VipDowngradeReviewDetailModel> Data { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }
}