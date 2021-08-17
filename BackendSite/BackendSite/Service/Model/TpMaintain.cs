using BackendSite.Extensions;
using System;
using System.Collections.Generic;

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
        public string TpGameTypeList { get; set; }
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

        public bool SiteFilter(FilterInfo filter)
        {
            return filter.SiteId.Contains(SiteId);
        }

        public bool TpFilter(FilterInfo filter)
        {
            if (filter.TpId == 0)
            {
                return true;
            }

            return filter.TpId == TpId;
        }

        public bool TpGameTypeFilter(FilterInfo filter)
        {
            if (filter.TpGameType == 0)
            {
                return true;
            }

            return filter.TpGameType == TpGameType;
        }

        public bool TpCategoryIdFilter(FilterInfo filter)
        {
            if (filter.TpCategoryId == 0)
            {
                return true;
            }

            return filter.TpCategoryId == TpCategoryId;
        }

        public bool DateFilter(FilterInfo filter)
        {
            return BeginDate.IsBetween(filter.StartDate, filter.EndDate, false) ||
                   EndDate.IsBetween(filter.StartDate, filter.EndDate, false) ||
                   (filter.StartDate > BeginDate && filter.EndDate < EndDate);
        }

        public bool ExpiredFilter(FilterInfo filter)
        {
            if (!filter.IsExpired)
            {
                return true;
            }

            return EndDate < DateTime.UtcNow;
        }
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

    public class TpCategory
    {
        public int CategoryId { set; get; }
        public string CategoryName { set; get; }
    }

    public class FilterInfo
    {
        public List<int> SiteId { get; set; }
        public int TpId { get; set; }
        public int TpGameType { get; set; }
        public int TpCategoryId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsExpired { get; set; }
    }
}
