using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class LatestHighlightVideoManagementModel
    {
    }
    public class GetLatestHighlightVideoDataPara
    {
        public IEnumerable<int> SelectSite { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        //public int CurrencyId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int Lang { get; set; }
        public GetLatestHighlightVideoDataPara()
        {
            //PageSize預設10
            if (PageSize == 0)
                PageSize = 10;
        }
    }
    public class LatestHighlightVideoDataResult
    {
        public List<LatestHighlightVideoData> LatestHighlightVideoDatas { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }
    public class LatestHighlightVideoData
    {
        public int SiteId { get; set; }
        public string LangId { get; set; }
        public string Date { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public string Time { get; set; }
        public string Operator { get; set; }
        public string SiteName { get; set; }
        public int VideoId { get; set; }
    }

    public class SetLatestHighlightVideoPara
    {
        public string Date { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public int VideoId { get; set; }
    }
    public class GetSiteLangPara
    {
        public int SiteId { get; set; }
    }
    public class GetSiteLangData
    {
        public string LangId { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }

    }
    public class InsrtLatestHighlightVideoPara
    {
        public int SiteId { get; set; }
        public string Date { get; set; }
        public string Detail { get; set; }
    }
}
