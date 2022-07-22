using System;

namespace BackendSite.Service.Model
{
    public class GetTpBetRecordDetailRequest
    {
        public int GameType { get; set; }

        public string TransId { get; set; }

        public int CustId { get; set; }

        public DateTime SettledTime { get; set; }
    }
}
