using System.ComponentModel.DataAnnotations.Schema;

namespace BackendSite.Service.Model
{
    /// <summary>
    /// Currency Schema
    /// </summary>
    [Table("Currency")]
    public class CurrencyModel
    {
        public int CurrencyId
        {
            get;
            set;
        }

        public string IsoCode
        {
            get;
            set;
        }

        public string Note
        {
            get;
            set;
        }

        public decimal ActualRate
        {
            get;
            set;
        }

    }
}
