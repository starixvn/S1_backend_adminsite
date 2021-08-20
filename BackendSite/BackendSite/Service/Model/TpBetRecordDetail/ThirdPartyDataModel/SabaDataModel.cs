namespace BackendSite.Service.Model.TpBetRecordDetail.ThirdPartyDataModel
{
    /// <summary>
    /// TpId=1,沙巴資料模型
    /// Description:根據沙巴資料明細模型對應
    /// </summary>
    public class SabaDataModel : object
    {
        public object BetDetail { get; set; }

        public object BetNumberDetail { get; set; }

        public long last_version_key { get; set; }

        public object BetVirtualSportDetails { get; set; }
    }
}