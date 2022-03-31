using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class GetSlotGameListReq : ApiRequestPageModel
    {
        public int SiteId { get; set; }
        public int TpId { get; set; }
        public string Name { get; set; }
        public SlotGameFilterStatusEnum Status { get; set; }
        public GetSlotGameListReq()
        {
            //PageSize預設100
            PageSize = 100;
        }
    }
    public class SlotGameList
    {
        private DateTimeOffset _updateTime { get; set; }
        public DateTimeOffset UpdateTime
        {
            get => _updateTime;
            set => _updateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public int TpId { get; set; }
        public int Id { get; set; }
        public string GameId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Operator { get; set; }
    }
    public class SlotGameName
    {
        public string en { get; set; }
        public string ch { get; set; }
        public string cs { get; set; }
    }
    public class GetSlotGameListRep 
    {
        public List<SlotGameData> SlotGameDataList { get; set; }
    }
    public class SlotGameData
    {
        public DateTimeOffset UpdateTime { get; set; }
        public int TpId { get; set; }
        public int Id { get; set; }
        public string GameId { get; set; }
        public string EName { get; set; }
        public string CName { get; set; }
        public string Operator { get; set; }
        //public SlotGameStatusEnum MYR { get; set; }
        //public SlotGameStatusEnum THB { get; set; }
        //public SlotGameStatusEnum RMB { get; set; }
        //public SlotGameStatusEnum IDR { get; set; }
        //public SlotGameStatusEnum INR { get; set; }
        //public SlotGameStatusEnum VND { get; set; }
        public List<CurrencyStatus> CurrencyStatusList { get; set; }
    }
    public class CurrencyStatus
    {
        public int CurrencyId { get; set; }
        public int Status { get; set; }
    }
    public class SetSlotGameReq
    { 
        public int CurrencyId { get; set; }
        public int SlotGameId { get; set; }
        public int SiteId { get; set; }
        public SlotGameStatusEnum Status { get; set; }
    }
    public class GetSlotGameCategoryListReq : ApiRequestPageModel
    {
        public int SiteId { get; set; }
        public int TpId { get; set; }
        public string Name { get; set; }
        public SlotGameCategoryEnum Category { get; set; }
        public GetSlotGameCategoryListReq()
        {
            PageSize = 100;
        }
    }
    public class GetSlotGameCategoryListRep
    {
        public List<SlotGameData> SlotGameCategoryList { get; set; }
    }
    public class SlotGameCategoryList
    {
        private DateTimeOffset _updateTime { get; set; }
        public DateTimeOffset UpdateTime
        {
            get => _updateTime;
            set => _updateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public int TpId { get; set; }
        public int Id { get; set; }
        public string GameId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Operator { get; set; }
    }
    public class SlotGameStatus
    {
        public int CurrencyId { get; set; }
        public int Status { get; set; }
    }
    public class SetSlotGameCategoryReq
    {
        public int CurrencyId { get; set; }
        public int SlotGameId { get; set; }
        public int SiteId { get; set; }
        public int CategoryId { get; set; }
        public SlotGameDetailStatusEnum Status { get; set; }
    }
}
