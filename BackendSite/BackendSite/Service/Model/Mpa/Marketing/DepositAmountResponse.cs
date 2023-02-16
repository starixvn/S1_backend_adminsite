using Newtonsoft.Json;

namespace BackendSite.Service.Model.Mpa.Marketing
{
    public class DepositAmountSummaryResponse
    {
        public decimal DepositAmount { get; set; }

        public decimal CompareDepositAmount { get; set; }

        public string ChangeRate { get; set; }
    }

    public class DepositSinUpSummaryResponse
    {
        [JsonProperty("signUpData")]
        public List<SignUpDatSumDto> SignUpData { get; set; }

        [JsonProperty("totalSummary")]
        public SignUpTotalSummary TotalSummary { get; set; }
    }

    public class SignUpChildDto
    {
        [JsonProperty("signUpSource")] public string SignUpSource => "";

        [JsonProperty("signUpUrl")]
        public string SignUpUrl { get; set; }

        [JsonProperty("firstDepositAmount")]
        public decimal FirstDepositAmount { get; set; }

        [JsonProperty("totalDepositAmount")]
        public decimal TotalDepositAmount { get; set; }

        [JsonProperty("noOfPlayers")]
        public int NoOfPlayers { get; set; }
    }

    public class SignUpDatSumDto
    {
        [JsonProperty("signUpSource")]
        public string SignUpSource { get; set; }

        [JsonProperty("signUpUrl")]
        public string SignUpUrl { get; set; }

        [JsonProperty("firstDepositAmount")]
        public decimal FirstDepositAmount { get; set; }

        [JsonProperty("totalDepositAmount")]
        public decimal TotalDepositAmount { get; set; }

        [JsonProperty("noOfPlayers")]
        public int NoOfPlayers { get; set; }

        [JsonProperty("children")]
        public List<SignUpChildDto> Children { get; set; }
    }

    public class SignUpTotalSummary
    {
        public decimal TotalDepositAmount { get; set; }
        public decimal FirstDepositAmount { get; set; }
        public int NoOfPlayers { get; set; }
    }


    public class DepositSignUpDetailData
    {
        [JsonIgnore]
        public int Num { get; set; }

        public string UserName { get; set; }

        public string CurrencyName { get; set; }

        public string RegisteredDate { get; set; }

        public string SignUpUrl { get; set; }

        public string AffCode { get; set; }

        public decimal FirstDepositAmount { get; set; }

        public int DepositCount { get; set; }

        public decimal TotalDepositAmount { get; set; }
    }
}