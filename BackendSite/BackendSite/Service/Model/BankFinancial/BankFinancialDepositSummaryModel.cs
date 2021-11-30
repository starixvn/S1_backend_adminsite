using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model.BankFinancial
{
    public class BankFinancialDepositSummaryModel
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

        public DepositTypeEnum DepositType
        {
            get;
            set;
        }

        public decimal DepositSummary
        {
            get;
            set;
        }
    }
}
