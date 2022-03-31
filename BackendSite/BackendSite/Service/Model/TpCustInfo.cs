using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class TpCustInfo
    {
        public int SiteId { get; set; }
        public int TpId { set; get; }
        public string TpName { get; set; }
        public int CustId { set; get; }
        public string UserName { set; get; }
        public string TpUserName { get; set; }
        public TpCustInfoStatusEnum Status { get; set; }
    }
}
