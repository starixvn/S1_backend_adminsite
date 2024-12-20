using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.Model
{
    public class NewAdminUser
    {
        public int UserId { set; get; }
        public string UserName { set; get; }
        public string Userpwd { set; get; }
        public bool IsSuper { set; get; }
        public int Status { set; get; }
        public DateTime CreateTime { get; set; }
        public string CurrencyList { get; set; }
        public int SiteId { get; set; }
        public AdminUserTypeEnum UserType { get; set; }
        public int RefId { get; set; }
    }

    public class SpecialAdminUser
    {
        public int UserId { set; get; }
        public string UserName { set; get; }
        public int Status { set; get; }
        [JsonIgnore]
        public string JsonCurrencyIds { get; set; }
        public IEnumerable<int> CurrencyIds
            => JsonConvert.DeserializeObject<IEnumerable<int>>(JsonCurrencyIds);
        [JsonIgnore]
        public string JsonGroupIds { get; set; }
        public IEnumerable<int> GroupIds
            => JsonConvert.DeserializeObject<IEnumerable<int>>(JsonGroupIds);
        [JsonIgnore]
        public string JsonSiteIds { get; set; }
        public IEnumerable<int> SiteIds
            => JsonConvert.DeserializeObject<IEnumerable<int>>(JsonSiteIds);
    }
    
    public class SetSpecialAdminUserParams
    {
        public int UserId { set; get; }
        public string UserName { set; get; }
        public string Userpwd { set; get; }
        public AdminUserTypeEnum UserType { get; set; }
        public int RefId { get; set; }
        public IEnumerable<int> SiteIds { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public IEnumerable<int> GroupIds { get; set; }
        public string Operator { get; set; }
    }
}
