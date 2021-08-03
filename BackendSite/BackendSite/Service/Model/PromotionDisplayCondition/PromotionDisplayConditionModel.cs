using System;
using System.Collections.Generic;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model.PromotionDisplayCondition
{
    public class PromotionDisplayConditionModel
    {
        public int Id
        {
            get;
            set;
        }

        public string BonusCode
        {
            get;
            set;
        }

        public int SiteId
        {
            get;
            set;
        }

        public PromotionDisplayConditionType ConditionType
        {
            get;
            set;
        }

        public dynamic Description
        {
            get;
            set;
        }

        public DateTime CreateTime
        {
            get;
            set;
        }

        public DateTime UpdateTime
        {
            get;
            set;
        }
    }
}