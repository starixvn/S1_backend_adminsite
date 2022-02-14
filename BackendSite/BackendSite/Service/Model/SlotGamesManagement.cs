using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class SlotGamesManagement
    {
    }
    public class GetSlotGameListReq
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int Site { get; set; }
        public int TPid { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public GetSlotGameListReq()
        {
            //PageSize預設100
            if (PageSize == 0)
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
        //public List<CurrencyStatus> CurrencyStatus { get; set; }
        public int MYR { get; set; }
        public int THB { get; set; }
        public int RMB { get; set; }
        public int IDR { get; set; }
        public int INR { get; set; }
        public int VND { get; set; }
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
        public int Site { get; set; }
    }
    public class GetSlotGameCategoryListReq
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int Site { get; set; }
        public int TPid { get; set; }
        public string Name { get; set; }
        public int Category { get; set; }
        //public int Status { get; set; }
        public GetSlotGameCategoryListReq()
        {
            //PageSize預設100
            if (PageSize == 0)
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
        public int Site { get; set; }
        public int CategoryId { get; set; }
    }
}
