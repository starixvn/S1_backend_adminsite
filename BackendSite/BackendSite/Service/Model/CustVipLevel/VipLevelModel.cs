using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model.CustVipLevel
{
    public class VipLevelModel
    {
        public int SiteId
        {
            get; set;
        }

        public int Level
        {
            get; set;
        }

        public string LevelName
        {
            get; set;
        }

        public int Fixed
        {
            get; set;
        }

        public DateTime CreateTime
        {
            get; set;
        }

        public DateTime UpdateTime
        {
            get; set;
        }
    }
}
