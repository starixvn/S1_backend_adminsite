using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class BankFinancialTransferInfoReceiptModel
    {
        public int Id { get; set; }
        public string TransferId { get; set; }
        public string PicFileName { get; set; }
        public byte[] TransferPic { get; set; }
    }
}
