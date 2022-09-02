using BackendSite.Extensions;

namespace BackendSite.Service.Model.ApiModel
{
    public class PageInfo : IPageInfo
    {      
        public int TotalPages { get; set; } = 0;
        public int PageNumber { get; set; } = 0;
        public int RowCount { get; set; } = 0;
        public string PageRange { get; set; } = "-";
    }
}