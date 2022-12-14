using Newtonsoft.Json;

namespace BackendSite.Service.Model.Mpa.Marketing
{
    public interface IMpaMarketingDepositAmountBaseDto
    {
        public DateTime Date { get; set; }
        public decimal DepositAmount { get; set; }
        public int CurrencyId { get; set; }
    }

    public class MpaMarketingDepositAmountSummaryDto : IMpaMarketingDepositAmountBaseDto
    {
        public DateTime Date { get; set; }
        public decimal DepositAmount { get; set; }
        public int CurrencyId { get; set; }
    }


    public interface IMpaMarketingDepositSignUpBaseDto
    {
        public DateTime Date { get; set; }
        public decimal DepositAmount { get; set; }

        public decimal FirstDepositAmount { get; set; }
        public int CurrencyId { get; set; }
    }


    public class MpaMarketingDepositSignUpSummaryDto: IMpaMarketingDepositSignUpBaseDto
    {
        public DateTime Date { get; set; }

        public int CustId { get; set; }

        public int CurrencyId { get; set; }

        public decimal DepositAmount { get; set; }

        public decimal FirstDepositAmount { get; set; }

        public string AffCode { get; set; }

        public string SignUpUrl { get; set; }

        public DateTime RegTime { get; set; }

        public int AgentId { get; set; }

        /// <summary>
        /// 此欄位非 DB 查詢欄位，由程式碼邏輯判斷 Set Value
        /// </summary>
        public string SignUpGroupName { get; set; }
    }

    public class MpaMarketingDepositSignUpDetailDto : IMpaMarketingDepositSignUpBaseDto
    {
        public DateTime Date { get; set; }

        public string UserName { get; set; }

        public int CurrencyId { get; set; }

        public decimal DepositAmount { get; set; }

        public decimal FirstDepositAmount { get; set; }

        public int DepositCount { get; set; }

        public string AffCode { get; set; }

        public string SignUpUrl { get; set; }

        public DateTime RegisteredDate { get; set; }
    }


    public class SignUpConfig
    {
        public string AppDomain { get; set; }

        public List<int> CreditSiteAgentIds { get; set; }

        public string CreditSiteName { get; set; }

        public Dictionary<string, IEnumerable<string>> MainSiteInfo { get; set; }
    }

}