using System;
using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class News
    {
        private DateTimeOffset? _createTime { get; set; }
        private DateTimeOffset? _postTime { get; set; }
        private DateTimeOffset? _lastUpdateTime { get; set; }
        public int? NewsId { get; set; }
        public int? SiteId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageURL { get; set; }
        public int? Status { get; set; }
        public DateTimeOffset? PostTime 
        {
            get => _createTime;
            set
            {
                if (value != null) _createTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
        public DateTimeOffset? CreateTime
        {
            get => _postTime;
            set
            {
                if (value != null) _postTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
        public DateTimeOffset? LastUpdateTime 
        {
            get => _lastUpdateTime;
            set
            {
                if (value != null) _lastUpdateTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
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
