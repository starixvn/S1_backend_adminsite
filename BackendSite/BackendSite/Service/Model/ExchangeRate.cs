using System.ComponentModel.DataAnnotations;

namespace BackendSite.Service.Model
{
    public class CurrencyUSDRate
    {
        public int CurrencyId { get; set; }
        public string IsoCode { get; set; }
        public string Note { get; set; }
        public decimal USDRate { get; set; }
        public bool IsCrypto { get; set; }
        public string TimeZone { get; set; }
        public string LastUpdateUser { get; set; }
        public DateTimeOffset UpdateTime
        {
            get
            {
                return new DateTimeOffset(_updateTime, TimeSpan.Zero);
            }
            set
            {
                _updateTime = value.DateTime;
            }
        }
        private DateTime _updateTime { get; set; }
    }

    public class SetExchangeUSDRateRequest
    {
        [Required]
        public int CurrencyId { get; set; }

        [Required]
        public decimal USDRate { get; set; } 
    }

}
