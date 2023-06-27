using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    #region Reuqest Model

    public class AutoVerifyRuleDataListRequest
    {
        public int SiteId { get; set; }
    }

    public class AutoVerifyRuleCreateRequest
    {
        public int SiteId { get; set; }

        public CurrencyEnum CurrencyId { get; set; }

        public decimal WithdrawalAmount { get; set; }

        public int AccountWithdrawalCount { get; set; }

        public List<int> VipLevels { get; set; }

        public List<int> Tags { get; set; }
        
        public List<string> DeviceLevel { get; set; }
        
        public int DeviceScores { get; set; }
    }

    public class AutoVerifyRuleSetRequest
    {
        /// <summary>
        /// DB SQ
        /// </summary>
        public int Id { get; set; }

        public decimal WithdrawalAmount { get; set; }

        public int AccountWithdrawalCount { get; set; }

        /// <summary>
        /// '0: Close 1:Open'
        /// </summary>
        public int Status {get; set; }

        public List<int> VipLevels { get; set; }

        public List<int> Tags { get; set; }
        
        public List<string> DeviceLevel { get; set; }
        
        public int DeviceScores { get; set; }
        
    }

    public class AutoVerifyRuleDeleteRequest
    {
        public int Id {get; set; }
    }

    #endregion

    #region Base 共用欄位

    public class AutoVerifyRuleDataListBase
    {
        /// <summary>
        /// DB SQ
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 0：Close 1：Open
        /// </summary>
        public int Status { get; set; }

        public DateTime CreateTime { get; set; }

        public string CreateUser { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public string LastUpdateUser { get; set; }
    }

    #endregion

    #region DTO

    public class AutoVerifyRuleDataListDto : AutoVerifyRuleDataListBase
    {
        public CurrencyEnum CurrencyId { get; set; }

        public decimal WithdrawalAmount { get; set; }

        public int AccountWithdrawalCount { get; set; }

        public string VipLevelStr { get; set; }

        public string TagStr { get; set; }
        
        public string DeviceLevelStr { get; set; }

        public int DeviceScores { get; set; }

        public int SiteId { get; set; }
    }
    
    #endregion

    #region Response Model

    public class AutoVerifyRuleDataListResponse : AutoVerifyRuleDataListBase
    {
        public CurrencyEnum CurrencyId { get; set; }

        public string IsoCode => CurrencyId.ToString();

        public decimal WithdrawalAmount { get; set; }

        public int AccountWithdrawalCount { get; set; }

        public List<int> VipLevels { get; set; }

        public List<int> Tags { get; set; }
        
        public List<string> DeviceLevel { get; set; }
        
        public int? DeviceScores { get; set; }

    }
    #endregion

    

}
