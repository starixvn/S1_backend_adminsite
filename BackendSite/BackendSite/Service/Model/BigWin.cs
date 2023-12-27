using MessagePack;
using Newtonsoft.Json;

namespace BackendSite.Service.Model
{
    public class BigWin
    {
    }
    public class GetBigWinSettingRequest
    {
        public int SiteId { get; set; }
    }
    public class BigWinSetting
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string TpCategory { get; set; }
        public int MultipleOfWinning { get; set; }
        public int Status { get; set; }
        public string CurrencyName { get; set; }
        public string SiteName { get; set; }
        public decimal MinOfWinning { get; set; }
        public IEnumerable<int> ITpCategoryList
        {
            get
            {
                return string.IsNullOrWhiteSpace(TpCategory) ?
                    null :
                    JsonConvert.DeserializeObject<IEnumerable<int>>(TpCategory);
            }
        }
    }
    public class BigWinReportRequest
    {
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public IEnumerable<int> CurrencyId { get; set; }
        public int SiteId { get; set; }
    }
    public class BigWinReport 
    { 
        public string Site { get; set; }
        public string Currency { get; set; }
        public string EventDate { get; set; }
        public string MemberID { get; set; }
        public string Category { get; set; }
        public string VenderName { get; set; }
        public decimal Amount { get; set; }
        public int TpId { get; set; }
        public int TpGameType { get; set; }
        public int TpCategory { get; set; }
        public string GameID { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string TypeName { get; set; }
    }
    public class BigWinExportData
    {
        public string Site { get; set; }
        public string Currency { get; set; }
        public string EventDate { get; set; }
        public string MemberID { get; set; }
        public string Category { get; set; }
        public string VenderName { get; set; }
        public string GameName { get; set; }
        public decimal Amount { get; set; }
    }
}
