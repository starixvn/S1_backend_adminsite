namespace BackendSite.Service.Model.BankFinancial
{
    public class BankFinancialTransferInfoReceiptModel
    {
        public int Id { get; set; }
        public string TransferId { get; set; }
        public string PicFileName { get; set; }
        public byte[] TransferPic { get; set; }
    }
}
