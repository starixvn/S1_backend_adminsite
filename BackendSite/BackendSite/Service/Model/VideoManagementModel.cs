namespace BackendSite.Service.Model
{

    public class GetVideoDataPara
    {
        public int SiteId { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public IEnumerable<string> LangList { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public GetVideoDataPara()
        {
            //PageSize預設10
            if (PageSize == 0)
                PageSize = 10;
        }
    }
    public class VideoDataResult
    {
        public List<VideoData> VideoDatas { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }
    public class VideoData
    {
        public string Date { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string LangId { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        private DateTimeOffset _lastUpdateTime { get; set; }
        public DateTimeOffset LastUpdateTime
        {
            get => _lastUpdateTime;
            set => _lastUpdateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public string Operator { get; set; }
    }

    public class SetVideoPara
    {
        public string LangId { get; set; }
        public string Date { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
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
    public class InsertVideoPara
    {
        public int SiteId { get; set; }
        public string Date { get; set; }
        public string Detail { get; set; }
    }
}
