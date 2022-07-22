using System;
using BackendSite.Attributes;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class LineChannelDto
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string Link { get; set; }
        public int Status { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string LastOperator { get; set; }
    }
    
    public class SetLineChannel
    {
        [LogIgnore]
        public int Id { get; set; }
        public string Link { get; set; }
        [LogIgnore]
        public LineChannelStatusEnum Status { get; set; }
        public string StatusName => Status.ToString();
    }
    
    public class DeleteLineChannel
    {
        [LogIgnore]
        public int Id { get; set; }
        // Log記錄用
        public string Link { get; set; }
    }
}
