using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class UserOnlineReportModel
    {

        public string LocalTime
        {
            get;
            set;
        }

        public int MaxCount
        {
            get;
            set;
        }
    }

    public class UserOnlinePartcialSummaryModel
    {
        public DateTime Time
        {
            get;
            set;
        }

        public int SiteId
        {
            get;
            set;
        }

        public int MaxCount
        {
            get;
            set;
        }
    }
}
