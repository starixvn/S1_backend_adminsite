using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class BankFinancialReasonModel
    {
        /// <summary>
        /// 讀寫 原因ID
        /// </summary>
        public int ReasonId
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 原因說明
        /// </summary>
        public string Description
        {
            get;
            set;
        }
    }
}
