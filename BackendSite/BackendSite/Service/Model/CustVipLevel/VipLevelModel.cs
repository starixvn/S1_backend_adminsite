using System;

namespace BackendSite.Service.Model.CustVipLevel
{
    public class VipLevelModel
    {
        private DateTimeOffset _createTime { get; set; }
        private DateTimeOffset _updateTime { get; set; }
        public int SiteId { get; set; }
        public int Level { get; set; }
        public string LevelName { get; set; }
        public int Fixed { get; set; }
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
    }
}
