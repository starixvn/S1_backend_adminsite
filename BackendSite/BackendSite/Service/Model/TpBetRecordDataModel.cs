using System;

namespace BackendSite.Service.Model
{
    /// <summary>
    /// 代表 第三方下注紀錄資料
    /// </summary>
    public class TpBetRecordDataModel
    {
        /// <summary>
        /// 讀寫 第三方ID
        /// </summary>
        public int TpId { get; set; }

        /// <summary>
        /// 讀寫 第三方名稱
        /// </summary>
        public string TpName { get; set; }
        
        /// <summary>
        /// 讀寫 交易ID
        /// </summary>
        public string TransId { get; set; }

        /// <summary>
        /// 讀寫 類別ID
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// 讀寫 類別名稱
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// 讀寫 遊戲ID
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// 讀寫 遊戲名稱
        /// </summary>

        public string GameName { get; set; }

        /// <summary>
        /// 讀寫 客戶ID
        /// </summary>
        public int CustId { get; set; }

        /// <summary>
        /// 讀寫 客戶名稱
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 讀寫 幣別
        /// </summary>
        public int CurrencyId { get; set; }

        /// <summary>
        /// 讀寫 幣別代號
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 讀寫 網站ID
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// 讀寫 網站名稱
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// 讀寫 第三方使用者名稱
        /// </summary>
        public string TpUserName { get; set; }

        /// <summary>
        /// 讀寫 交易時間(UTC+8)
        /// </summary>
        public DateTime TransTime { get; set; }

        /// <summary>
        /// 讀寫 開獎時間(UTC+8)
        /// </summary>
        public DateTime SettledTime { get; set; }

        /// <summary>
        /// 讀寫 投注金額
        /// </summary>
        public decimal Stake { get; set; }

        /// <summary>
        /// 讀寫 有效投注金額
        /// </summary>
        public decimal ValidBet { get; set; }

        /// <summary>
        /// 讀寫 傭金金額
        /// </summary>
        public decimal Revenue { get; set; }

        /// <summary>
        /// 讀寫 輸贏金額
        /// </summary>
        public decimal WinLostAmount { get; set; }

        /// <summary>
        /// 讀寫 開獎類型
        /// </summary>
        public int SettlementType { get; set; }

        /// <summary>
        /// 讀寫 票券狀態
        /// </summary>
        public int TicketStatus { get; set; }

        /// <summary>
        /// 讀寫 票券狀態(文字)
        /// </summary>
        public string OriginalState { get; set; }

        /// <summary>
        /// 讀寫 票券驗證碼
        /// </summary>
        public int TicketKey { get; set; }

        /// <summary>
        /// 讀寫 獎金ID
        /// </summary>
        public int BonusId { get; set; }

        /// <summary>
        /// 讀寫 資料建立時間(UTC+8)
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
