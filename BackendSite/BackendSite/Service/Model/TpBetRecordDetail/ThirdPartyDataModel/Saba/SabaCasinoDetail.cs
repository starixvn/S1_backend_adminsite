using BackendSite.Service.Model.TpBetRecordDetail.Interfaces;
using BackendSite.Service.Model.TpBetRecordDetail.ThirdPartyDataModel.Saba;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace BackendSite.Service.Model.TpBetRecordDetail.ThirdPartyDataModel
{

    public class SabaCasinoDetail : BaseDetail, ICasinoDetail
    {
        private readonly BetDetail _betDetail;
        private readonly TpCurrency _tpCurrency;
        private const int TimeZone = -4;

        public SabaCasinoDetail(GetDetailObject detailObject) : base(detailObject)
        {
            _betDetail = JsonConvert.DeserializeObject<GetBetDetailResult>(detailObject.TpBetRecordDetail.Detail)?.BetDetails?.First();
            _tpCurrency = detailObject.TpCurrency;          
        }
        public string RefTransId => _betDetail?.trans_id;

        public string TpUserName => _betDetail?.vendor_member_id;

        public DateTime UTCBetTime => _betDetail == null ? DateTime.MinValue : new DateTimeOffset(_betDetail.transaction_time, new TimeSpan(TimeZone, 0, 0)).DateTime;

        public DateTime UTCSettledTime => _betDetail == null ? DateTime.MinValue : new DateTimeOffset(_betDetail.settlement_time, new TimeSpan(TimeZone, 0, 0)).DateTime;

        public decimal Stake => (_betDetail?.stake ?? 0) * TpCurrency.ExchangeRate;

        public decimal WinlostAmount => (_betDetail?.winlost_amount ?? 0) * TpCurrency.ExchangeRate;
    }
}