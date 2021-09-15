namespace BackendSite.Service.Model.TpBetRecordDetail
{
    public class BaseDetail
    {
        public int TpId { get; }

        public int TpCategoryId { get; }

        public int GameType { get; }

        public string GameTypeName { get; }

        public string TransId { get; }

        public string Detail { get; }

        public TpCurrency TpCurrency { get; }

        public BaseDetail(GetDetailObject detailObject)
        {
            TpId = detailObject.TpBetRecordDetail.TpId == 0 ? detailObject.OriginalTpId : detailObject.TpBetRecordDetail.TpId;
            TpCategoryId = detailObject.TpCategoryId;
            GameType = detailObject.TpBetRecordDetail.GameType == 0 ? detailObject.OriginalGameType : detailObject.TpBetRecordDetail.GameType;
            GameTypeName = detailObject.OriginalGameTypeName;
            TransId = detailObject.TpBetRecordDetail.TransId == null ? detailObject.OriginalTransId : detailObject.TpBetRecordDetail.TransId;
            Detail = detailObject.TpBetRecordDetail.Detail == null ? null : detailObject.TpBetRecordDetail.Detail;
            TpCurrency = detailObject.TpCurrency;
        }
    }
}
