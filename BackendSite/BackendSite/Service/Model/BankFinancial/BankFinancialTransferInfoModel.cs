using System;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model.BankFinancial
{
    public class BankFinancialTransferInfoModel
    {
        /// <summary>
        /// 讀寫 識別值ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 讀寫 交易ID
        /// </summary>
        public string TransferId { get; set; }

        /// <summary>
        /// 讀寫 交易型態
        /// </summary>
        public TransactionTypeEnum TransactionType { get; set; }

        /// <summary>
        /// 讀寫 交易類型
        /// </summary>
        public TransferTypeEnum TransferType { get; set; }

        /// <summary>
        /// 讀寫 加項或減項
        /// </summary>
        public PlusOrMinusEnum PlusOrMinus { get; set; }

        /// <summary>
        /// 讀寫 交易日期
        /// </summary>
        public DateTimeOffset TransDate { get; set; }

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
        /// 讀寫 網站ID
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// 讀寫 網站名稱
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// 讀寫 客戶ID
        /// </summary>
        public int? CustId { get; set; }

        /// <summary>
        /// 讀寫 幣別
        /// </summary>
        public int CurrencyId { get; set; }

        /// <summary>
        /// 讀寫 銀行ID
        /// </summary>
        public int SysId { get; set; }

        /// <summary>
        /// 讀寫 帳戶帳號
        /// </summary>
        public string AccountNo { get; set; }

        /// <summary>
        /// 讀寫 帳戶顯示名稱
        /// </summary>
        public string AccountDisplayName { get; set; }

        /// <summary>
        /// 讀寫 交易金額
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 讀寫 手續費(皆為減項)
        /// </summary>
        public decimal Fee { get; set; }

        /// <summary>
        /// 讀寫 原因
        /// </summary>
        public int? Reason { get; set; }

        /// <summary>
        /// 讀寫 備註
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 讀寫 資料建立時間
        /// </summary>
        public DateTimeOffset CreateTime { get; set; }

        /// <summary>
        /// 讀寫 資料建立者
        /// </summary>
        public int Creator { get; set; }

        /// <summary>
        /// 讀寫 資料最近更新時間
        /// </summary>
        public DateTimeOffset UpdateTime { get; set; }

        /// <summary>
        /// 讀寫 資料更新者
        /// </summary>
        public int Updator { get; set; }
    }
}
