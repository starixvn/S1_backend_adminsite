namespace BackendSite.Service.Model
{
    public class GetHotGameListRequest
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
    }

    public class GetHotGameListResponse
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string IsoCode { get; set; }
        public string TpName { get; set; }
        public string TpGameType { get; set; }
        public string TpCategory { get; set; }
        public string GameName { get; set; }
    }

    public class SetHotGameRequest
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string LangId { get; set; }
        public int TpId { get; set; }
        public string TpName { get; set; }
        public string GameId { get; set; }
        public int TpGameType { get; set; }
        public int TpCategory { get; set; }
        public string GameName { get; set; }
        public GameLogo GameLogo { get; set; }
        public string TpLogo { get; set; }
        public int Sort { get; set; }
    }

    public class GameLogo
    {
        public string HotGamePcUrl { get; set; }
        public string HotGameMobileUrl { get; set; }
    }

    public class GetHotGameDetailRequest
    {
        public int Id { get; set; }
    }
    
    public class GetHotGameDetailResponse
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string IsoCode { get; set; }
        public int TpId { get; set; }
        public string TpName { get; set; }
        public int TpGameType { get; set; }
        public string GameTypeName { get; set; }
        public string TpCategory { get; set; }
        public string GameName { get; set; }
        public GameLogo GameLogo { get; set; }
        public string TpLogo { get; set; }
        public int Sort { get; set; }
    }
}