namespace BackendSite.Service.Model
{
    public class ThirdPartyAccountRequest
    {
        public int CustId { get; set; }
    }

    public class ThirdPartyAccountResponse 
    {
        /// <summary>
        /// 第三方名稱
        /// </summary>
        public string TpName { get; set; }

        /// <summary>
        /// 玩家在第三方名稱
        /// </summary>
        public string TpUserName { get; set; }
    }
}
