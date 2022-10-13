using System.ComponentModel.DataAnnotations;

namespace BackendSite.Service.Model
{
    public class GetExchangeUSDRateRequest
    {
        [Required]
        public int CurrencyId { get; set; }
    }

    public class GetExchangeUSDRateResponse
    {
        public decimal USDRate { get; set; }
    }

    public class SetExchangeUSDRateRequest
    {
        [Required]
        public int CurrencyId { get; set; }

        [Required]
        public decimal USDRate { get; set; }
    }

}
