namespace BackendSite.Service.Model.TpBetRecordDetail
{
    public class GetDetailObject
    {
        public int OriginalTpId { get; set; }

        public int OriginalGameType { get; set; }

        public string OriginalGameTypeName { get; set; }

        public string OriginalTransId { get; set; }

        public int TpCategoryId { get; set; }

        public TpCurrency TpCurrency { get; set; }

        public TpBetRecordDetailModel TpBetRecordDetail { get; set; }
    }
}
