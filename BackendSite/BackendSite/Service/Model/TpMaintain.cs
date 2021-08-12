using BackendSite.Service.Model.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model.TpMaintain
{
    public class TpMaintain
    {
        public int? SN { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int TpId { get; set; }
        public string TpName { get; set; }
        public int TpGameType { get; set; } = 0;
        public string TpGameTypeList{ get; set; }
        public string TypeName { get; set; }
        public int TpCategoryId { get; set; } = 0;
        public string CategoryName { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public string MaintainType { get; set; }
        public int TypeValue { get; set; }
        public string BeginTime { get; set; }
        public string EndTime { get; set; }
        public string UpdateUser { get; set; }
    }

    public class CommonTpInfo
    {
        public int Id { set; get; }
        public string TpName { set; get; }
    }

    public class TpGameType
    {
        public int Type { set; get; }
        public string TypeName { set; get; }
        public int TpId { set; get; }
        public int CategoryId { set; get; }
    }
}
