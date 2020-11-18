using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

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

    }
}
