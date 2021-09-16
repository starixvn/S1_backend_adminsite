using BackendSite.Service.Model.TpBetRecordDetail.Interfaces;
using BackendSite.Service.Model.TpBetRecordDetail.ThirdPartyDataModel.Saba;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace BackendSite.Service.Model.TpBetRecordDetail.ThirdPartyDataModel
{

    public class SabaVirtualSportsDetail : BaseDetail, IVirtualSportsDetail
    {
        private readonly BetVirtualSportDetail _betDetail;
        private readonly TpCurrency _tpCurrency;
        private const int TimeZone = -4;

        public SabaVirtualSportsDetail(GetDetailObject detailObject) : base(detailObject)
        {
            _betDetail = JsonConvert.DeserializeObject<GetBetDetailResult>(detailObject.TpBetRecordDetail.Detail)?.BetVirtualSportDetails?.First();
            _tpCurrency = detailObject.TpCurrency;          
        }
        public string RefTransId => _betDetail?.trans_id;

        public string TpUserName => _betDetail?.vendor_member_id;

        public DateTime UTCBetTime => _betDetail == null ? DateTime.Now : new DateTimeOffset(_betDetail.transaction_time, new TimeSpan(TimeZone, 0, 0)).DateTime;

        public DateTime UTCSettledTime => _betDetail == null ? DateTime.Now : new DateTimeOffset(_betDetail.match_datetime, new TimeSpan(TimeZone, 0, 0)).DateTime;

        public decimal Stake => (_betDetail?.stake ?? 0) * _tpCurrency.ExchangeRate;

        public decimal WinlostAmount => (_betDetail?.winlost_amount ?? 0) * _tpCurrency.ExchangeRate;
    }
}