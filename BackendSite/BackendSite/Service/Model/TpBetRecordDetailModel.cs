using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    /// <summary>
    /// 代表 第三方下注紀錄資料
    /// </summary>
    [Table("TpBetRecord")]
    public class TpBetRecordDetailModel
    {
        /// <summary>
        /// 讀寫 第三方ID
        /// </summary>
        [Column("TpId")]
        public int TpId
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 第三方名稱
        /// </summary>
        [Column("TpName")]
        public string TpName
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 交易ID
        /// </summary>
        [Column("TransId")]
        public string TransId
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 類別ID
        /// </summary>
        [Column("CategoryId")]
        public int CategoryId
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 類別名稱
        /// </summary>
        [Column("CategoryName")]
        public string CategoryName
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 遊戲ID
        /// </summary>
        [Column("GameType")]
        public int GameId
        {
            get;
            set;
        }
        
        /// <summary>
        /// 讀寫 遊戲名稱
        /// </summary>
        [Column("GameName")]
        public string GameName
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 客戶ID
        /// </summary>
        [Column("CustId")]
        public int CustId
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 客戶名稱
        /// </summary>
        [Column("UserName")]
        public string UserName
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 幣別
        /// </summary>
        [Column("CurrencyId")]
        public int CurrencyId
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 幣別代號
        /// </summary>
        [Column("Code")]
        public string Code
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 網站ID
        /// </summary>
        [Column("SiteId")]
        public int SiteId
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 網站名稱
        /// </summary>
        [Column("SiteName")]
        public string SiteName
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 第三方使用者名稱
        /// </summary>
        [Column("TpUserName")]
        public string TpUserName
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 交易時間(UTC+8)
        /// </summary>
        [Column("TransTime")]
        public DateTime TransTime
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 開獎時間(UTC+8)
        /// </summary>
        [Column("SettledTime")]
        public DateTime SettledTime
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 投注金額
        /// </summary>
        [Column("Stake")]
        public decimal Stake
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 有效投注金額
        /// </summary>
        [Column("VaildBet")]
        public decimal ValidBet
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 傭金金額
        /// </summary>
        [Column("Revenue")]
        public decimal Revenue
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 輸贏金額
        /// </summary>
        [Column("WinlostAmount")]
        public decimal WinLostAmount
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 開獎類型
        /// </summary>
        [Column("SettlementType")]
        public int SettlementType
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 票券狀態
        /// </summary>
        [Column("TicketStatus")]
        public int TicketStatus
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 票券狀態(文字)
        /// </summary>
        [Column("OriginalState")]
        public string OriginalState
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 票券驗證碼
        /// </summary>
        [Column("TicketKey")]
        public int TicketKey
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 獎金ID
        /// </summary>
        [Column("BonusId")]
        public int BonusId
        {
            get;
            set;
        }

        /// <summary>
        /// 讀寫 資料建立時間(UTC+8)
        /// </summary>
        [Column("CreateTime")]
        public DateTime CreateTime
        {
            get;
            set;
        }
    }
}
