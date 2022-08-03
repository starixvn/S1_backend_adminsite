using System;
using System.Collections.Generic;

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
        public string SiteName{ get; set; }
        public string TransId{ get; set; }
        public string UserName{ get; set; }
        public int CustId{ get; set; }
        public int CurrencyId{ get; set; }
        public string Currency{ get; set; }
        public decimal Amount{ get; set; }
        public string BankName{ get; set; }
        public string AccountNo{ get; set; }
        public string Memo{ get; set; }
        public string Operator{ get; set; }
        private DateTimeOffset _approvedTime{ get; set; }
        public DateTimeOffset ApprovedTime
        {
            get => _approvedTime;
            set => _approvedTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public string LatestUpdateOperator{ get; set; }
        private DateTimeOffset _latestUpdateTime{ get; set; }
        public DateTimeOffset LatestUpdateTime
        {
            get => _latestUpdateTime;
            set => _latestUpdateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        
    }
}