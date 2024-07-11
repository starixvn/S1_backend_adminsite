namespace BackendSite.Service.Model
{
    public class TpBonusCodeList
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public int TpId { get; set; }
        public string GameId { get; set; }
        public string Bonuscode { get; set; }
        public DateTimeOffset ExpirationDate { get; set; }
        public DateTimeOffset CreateAt { get; set; }
    }
}
