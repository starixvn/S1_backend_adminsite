using BackendSite.Extensions;

namespace BackendSite.Service.Model.ApiModel
{
    public class PageInfo : IPageInfo
    {
        public PageInfo(){}

        public PageInfo(int pageNumber, int pageSize, int totalPages, int totalCount)
        {
            TotalPages = totalPages;
            PageNumber = pageNumber;
            RowCount = totalCount;
            PageRange = pageNumber.GetPageRange(pageSize, totalCount);
        }
        
        public int TotalPages { get; set; } = 0;
        public int PageNumber { get; set; } = 0;
        public int RowCount { get; set; } = 0;
        public string PageRange { get; set; } = "-";
    }
}