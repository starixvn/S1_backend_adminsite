using System;

namespace BackendSite.Service.Model
{
    public class LineChannelModel
    {
        public LineChannelModel(LineChannelDto dto)
        {
            Id = dto.Id;
            SiteId = dto.SiteId;
            CurrencyId = dto.CurrencyId;
            Link = dto.Link;
            Status = dto.Status;
            CreateTime = dto.CreateTime;
            UpdateTime = dto.UpdateTime;
            LastOperator = dto.LastOperator;
        }

        private DateTimeOffset _createTime { get; set; }
        private DateTimeOffset _updateTime { get; set; }
        public int Id { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string Link { get; set; }
        public int Status { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset UpdateTime
        {
            get => _updateTime;
            set => _updateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public string LastOperator { get; set; }
    }
}
