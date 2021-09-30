using System;
using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class News
    {
        public int? NewsId { get; set; }
        public int? SiteId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageURL { get; set; }
        public int? Status { get; set; }
        public DateTime? PostTime { get; set; }

        public DateTime? CreateTime { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public long? UnixPostTime { get; set; }
        public string LastUpdateUser { get; set; }
        public int? Sort { get; set; }
    }
    public class NewsData 
    {
        public List<News> NewsList { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }
}
