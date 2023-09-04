namespace BackendSite.Service.Model
{
    public class GetDepositAccountingListModel
    {
        public List<DepositAccountingModel> Datas { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }

    public class DepositAccountingModel
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public string TransId { get; set; }
        public string UserName { get; set; }
        public int CustId { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public string BankName { get; set; }
        public string AccountNo { get; set; }
        public string Memo { get; set; }
        public string Operator { get; set; }
        private DateTime _approvedTime { get; set; }
        public DateTimeOffset ApprovedTime
        {
            get
            {
                return new DateTimeOffset(_approvedTime, _offset);
            }
            set
            {
                _approvedTime = value.DateTime;
            }
        }
        public string LatestUpdateOperator { get; set; }
        private DateTime _latestUpdateTime { get; set; }
        public DateTimeOffset LatestUpdateTime
        {
            get
            {
                return new DateTimeOffset(_latestUpdateTime, _offset);
            }
            set
            {
                _latestUpdateTime = value.DateTime;
            }
        }
        private TimeSpan _offset { get; set; } = TimeSpan.Zero;
        public void SetDepositAccountingTimeOffset(TimeSpan timeSpan)
        {
            _offset = timeSpan;
        }
    }
}