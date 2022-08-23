using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace BackendSite.Service.Model
{    
    public class GetTpBetRecordRequest
    {
        public int CustId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }

        public string GetRedisKey()
        {
            var jObject = JObject.Parse(JsonConvert.SerializeObject(this));
            jObject["StartTime"] = StartTime.ToString("yyyyMMddHHmmssfff");
            jObject["EndTime"] = EndTime.ToString("yyyyMMddHHmmssfff");
            var key = new StringBuilder();
            foreach (var item in jObject.Properties())
            {
                key.Append($"{item.Name}:{item.Value}:");
            }
            return key.ToString();
        }
    }

    public class TpBetRecord
    {
        public int TpId { get; set; }
        public string TransId { get; set; }
        public int GameType { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public int SiteId { get; set; }
        public string TpUserName { get; set; }
        public DateTimeOffset TransTime
        {
            get
            {
                return new DateTimeOffset(_transTime, TimeSpan.Zero);
            }
            set
            {
                _transTime = value.DateTime;
            }
        }
        private DateTime _transTime { get; set; }
        public DateTimeOffset SettledTime
        {
            get
            {
                return new DateTimeOffset(_settledTime, TimeSpan.Zero);
            }
            set
            {
                _settledTime = value.DateTime;
            }

        }
        private DateTime _settledTime { get; set; }
        public decimal Stake { get; set; }
        public decimal ValidBet { get; set; }
        public decimal Revenue { get; set; }
        public decimal WinlostAmount { get; set; }
        public decimal ProgressiveBet { get; set; }
        public decimal ProgressiveWin { get; set; }
        public int SettlementType { get; set; }
        public int TicketStatus { get; set; }
        public string OriginalState { get; set; }
        public long TicketKey { get; set; }
        public int BonusId { get; set; }
    }

}
