using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model.BankFinancial
{
    public class BankFinancialTransferSummaryModel
    {
        public string Date
        {
            get;
            set;
        }

        public int SiteId
        {
            get;
            set;
        }

        public int CurrencyId
        {
            get;
            set;
        }

        public string AccountNo
        {
            get;
            set;
        }

        public TransferTypeEnum TransferType
        {
            get;
            set;
        }

        public decimal TransferSummary
        {
            get;
            set;
        }

        public decimal FeeSummary
        {
            get;
            set;
        }
    }
}
