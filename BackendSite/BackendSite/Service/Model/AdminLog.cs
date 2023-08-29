using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class AdminLog
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public AdminAdjustmentLogMenuEnum MenuEnum { get; set; }
        public ActionType ActionType { get; set; }
        public string RefTransId { get; set; }
        public string BeforeDetailInfo { get; set; }
        public string AfterDetailInfo { get; set; }
        public string Operator { get; set; }
        public int CustId { get; set; }
        public string Menu { get; set; }
        public string Note { get; set; }
        public string DetailInfo { get; set; }
    }
}

