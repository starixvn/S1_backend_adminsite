using System;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model.PromotionDisplayCondition
{
    public class PromotionDisplayConditionModel
    {
        private DateTimeOffset _createTime { get; set; }
        private DateTimeOffset _updateTime { get; set; }
        public int Id { get; set; }
        public string BonusCode { get; set; }
        public int SiteId { get; set; }
        public PromotionDisplayConditionType ConditionType { get; set; }
        public dynamic Description { get; set; }
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