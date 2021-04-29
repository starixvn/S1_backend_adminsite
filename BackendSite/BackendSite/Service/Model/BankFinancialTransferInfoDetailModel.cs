using BackendSite.Service.Model.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class BankFinancialTransferInfoDetailModel
    {

        /// <summary>
        /// 讀寫 交易ID
        /// </summary>
        public string TransferId
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 交易型態
        /// </summary>
        public TransactionTypeEnum TransactionType
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 交易類型
        /// </summary>
        public TransferTypeEnum TransferType
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 交易日期
        /// </summary>
        public string TransDate
        {
            get;
            set;
        }

        #region From Data

        /// <summary>
        /// 讀寫 付款方式
        /// </summary>
        public TransferChannelEnum FromChannel
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 代理人
        /// </summary>
        public string FromAgent
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 第三方支付ID
        /// </summary>
        public int? FromPayId
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 第三方支付名稱
        /// </summary>
        public string FromPayName
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 網站ID
        /// </summary>
        public int FromSiteId
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 網站名稱
        /// </summary>
        public string FromSiteName
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 客戶ID
        /// </summary>
        public int? FromCustId
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 客戶名稱
        /// </summary>
        public string FromUserName
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 幣別
        /// </summary>
        public int FromCurrencyId
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 幣別代號
        /// </summary>
        public string FromIsoCode
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 銀行ID
        /// </summary>
        public int FromSysId
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 銀行代號
        /// </summary>
        public string FromBankCode
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 帳戶帳號
        /// </summary>
        public string FromAccountNo
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 帳戶顯示名稱
        /// </summary>
        public string FromAccountDisplayName
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 交易金額
        /// </summary>
        public decimal FromAmount
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 手續費(皆為減項)
        /// </summary>
        public decimal FromFee
        {
            get;
            set;
        }

        #endregion

        #region To Data

        /// <summary>
        /// 讀寫 付款方式
        /// </summary>
        public TransferChannelEnum ToChannel
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 代理人
        /// </summary>
        public string ToAgent
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 第三方支付ID
        /// </summary>
        public int? ToPayId
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 第三方支付名稱
        /// </summary>
        public string ToPayName
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 網站ID
        /// </summary>
        public int ToSiteId
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 網站名稱
        /// </summary>
        public string ToSiteName
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 客戶ID
        /// </summary>
        public int? ToCustId
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 客戶名稱
        /// </summary>
        public string ToUserName
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 幣別
        /// </summary>
        public int ToCurrencyId
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 幣別代號
        /// </summary>
        public string ToIsoCode
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 銀行ID
        /// </summary>
        public int ToSysId
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 銀行代號
        /// </summary>
        public string ToBankCode
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 帳戶帳號
        /// </summary>
        public string ToAccountNo
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 帳戶顯示名稱
        /// </summary>
        public string ToAccountDisplayName
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 交易金額
        /// </summary>
        public decimal ToAmount
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 手續費(皆為減項)
        /// </summary>
        public decimal ToFee
        {
            get;
            set;
        }

        #endregion

        /// <summary>
        /// 讀寫 原因
        /// </summary>
        public int? Reason
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 備註
        /// </summary>
        public string Remark
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 資料建立時間
        /// </summary>
        public string CreateTime
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 資料建立者
        /// </summary>
        public int Creator
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 資料建立者名稱
        /// </summary>
        public string CreatorName
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 資料最近更新時間
        /// </summary>
        public string UpdateTime
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 資料更新者
        /// </summary>
        public int Updator
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 資料更新者名稱
        /// </summary>
        public string UpdatorName
        {
            get;
            set;
        }
    }
}
