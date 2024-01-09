using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model.Report
{
    public class GetListReq
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int LobbyCategoryId { get; set; }
    }
    
    public class UpdateHeaderSettingByTpGameIdReq
    {
        public int Id { get; set; }
        private DateTimeOffset _iconStartTime;
        private DateTimeOffset _iconEndTime;
        public int SiteId { get; set; }
        public int TpGameIdNo { get; set; }
        public int Status { get; set; }
        public HeaderIconTypeEnum IconType { get; set; }
        public DateTimeOffset IconStartTime
        {
            get => _iconStartTime;
            set => _iconStartTime = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset IconEndTime
        {
            get => _iconEndTime;
            set => _iconEndTime = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
    }
    
    public class UpdateHeaderSettingReq : UpdateHeaderSettingByTpGameIdReq
    {
        public string Memo { get; set; }
        public string CurrencyGameName { get; set; }
    }
    
    public class UpdateHeaderSortReq
    {
        public int Id { get; set; }
        public int Sort { get; set; }
    }
    
    public class UpdateHeaderPictureReq
    {
        public int Id { get; set; }
        public HeaderPictureEnum HeaderPictureEnum { get; set; }
        public IFormFile Receipt { get; set; }
    }
    
    public class GetHeaderPictureReq
    {
        public int Id { get; set; }
    }
    
    public class HeaderPictureResponse
    {
        public HeaderPictureEnum HeaderPictureEnum { get; set; }
        public string Url { get; set; }
    }
    
    public class HeaderPicture
    {
        public string PcHeaderPic { get; set; }
        public string PcLobbyPic { get; set; }
        public string MobileHeaderPic { get; set; }
        public string MobileLobbyPic { get; set; }
    }
    
    public class HeaderManagementReport
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public int LobbyCategoryId { get; set; }
        public string LobbyCategory { get; set; }
        public int TpId { get; set; }
        public string TpName { get; set; }
        public int TpGameIdNo { get; set; }
        public string ProductName { get; set; }
        public string CurrencyGameName { get; set; }
        public string Memo { get; set; }
        public HeaderIconTypeEnum IconType { get; set; }
        public DateTime IconStartTime { get; set; }
        public DateTime IconEndTime { get; set; }
        public int Status { get; set; }
        public int Sort { get; set; }
        public DateTime UpdateTime { get; set; }
        public string Updater { get; set; }
    }
    
    public class HeaderManagementGroup
    {
        public int SiteId { get; set; }
        public string LobbyCategory { get; set; }
        public int TpGameIdNo { get; set; }
        public string ProductName { get; set; }
        public int Status { get; set; }
        public HeaderIconTypeEnum IconType { get; set; }
        public DateTimeOffset IconStartTime { get; set; }
        public DateTimeOffset IconEndTime { get; set; }
    }
    
    public class HeaderLobbyCategory
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}

