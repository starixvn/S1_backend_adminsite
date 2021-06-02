using BackendSite.Service.Model.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class TpMaintain
    {
        public int SN { get; set; }
        public int SiteId { get; set; }
        public int TpId { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public MaintainType MaintainType { get; set; }
        public int TypeValue { get; set; }
        public string BeginTime { get; set; }
        public string EndTime { get; set; }
    }

    public class CommonTpInfo
    {
        public int Id { set; get; }
        public string TpName { set; get; }
    }
}
