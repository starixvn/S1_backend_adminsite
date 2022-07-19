using System;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model.BankFinancial
{
    public class BankFinancialTransferAccountModel
    {
        private DateTimeOffset _createTime { get; set; }
        private DateTimeOffset _updateTime { get; set; }
        /// <summary>
        /// 讀寫 識別值ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 讀寫 網站ID
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// 讀寫 網站名稱
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// 讀寫 幣別Id
        /// </summary>
        public int CurrencyId { get; set; }

        /// <summary>
        /// 讀寫 銀行ID
        /// </summary>
        public int SysId { get; set; }

        /// <summary>
        /// 讀寫 目的
        /// 1=Deposit,2=Withdrawal,3=Transfer
        /// </summary>
        public string Purpose { get; set; }

        /// <summary>
        /// 讀寫 付款方式
        /// </summary>
        public TransferChannelEnum Channel { get; set; }

        /// <summary>
        /// 讀寫 代理人
        /// </summary>
        public string Agent { get; set; }

        /// <summary>
        /// 讀寫 第三方支付ID
        /// </summary>
        public int? PayId { get; set; }

        /// <summary>
        /// 讀寫 帳戶帳號
        /// </summary>
        public string AccountNo { get; set; }

        /// <summary>
        /// 讀寫 帳戶名稱
        /// </summary>
        public string AccountName { get; set; }
        
        /// <summary>
        /// 目前只 for INR IFSC. 使用
        /// </summary>
        public string AccountCode { get; set; }

        /// <summary>
        /// 讀寫 帳戶顯示名稱
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 讀寫 初始金額
        /// </summary>
        public decimal InitialAmount { get; set; }

        /// <summary>
        /// 讀寫 超過餘額警示
        /// </summary>
        public decimal MaxBalanceAlertAmount { get; set; }

        /// <summary>
        /// 讀寫 原因
        /// </summary>
        public int? Reason { get; set; }

        /// <summary>
        /// 讀寫 備註
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 讀寫 資料狀態
        /// </summary>
        public TransferAccountStatusEnum Status { get; set; }

        /// <summary>
        /// 讀寫 資料建立時間
        /// </summary>
        public DateTimeOffset CreateTime 
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }

        /// <summary>
        /// 讀寫 資料建立者
        /// </summary>
        public int Creator { get; set; }

        /// <summary>
        /// 讀寫 資料最近更新時間
        /// </summary>
        public DateTimeOffset UpdateTime 
        {
            get => _updateTime;
            set => _updateTime = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }

        /// <summary>
        /// 讀寫 資料更新者
        /// </summary>
        public int Updator { get; set; }
    }
}
