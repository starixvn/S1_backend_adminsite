namespace BackendSite.Service.Model.ApiModel;

public interface IApiRequest
{
    public int SiteId { get; set; }
    public IEnumerable<int> SiteIds { get; set; }
    public int CurrencyId { get; set; }
    public IEnumerable<int> CurrencyIds { get; set; }

    public IEnumerable<MultiSiteRequestDto> ConvertToMultiSiteParam(IEnumerable<ServerInfo> serverInfos)
    {
        List<MultiSiteRequestDto> result = new List<MultiSiteRequestDto>();
        ConvertSiteId(serverInfos);
        ConvertCurrencyId(serverInfos);
        SiteIds.ToList().ForEach(z =>
        {
            var siteData = serverInfos.ToList().Find(x => x.SiteId == z);
            if (siteData != null)
            {
                var currencyList = siteData.ICurrencyList.Intersect(CurrencyIds);
                result.AddRange(currencyList.Select(c => new MultiSiteRequestDto() { SiteId = z, CurrencyId = c }));
            }
        });

        return result;
    }

    private void ConvertSiteId(IEnumerable<ServerInfo> serverInfos)
    {
        if (SiteIds == null)
            if (SiteId == 0) //Select All
                SiteIds = serverInfos.Select(x => x.SiteId).ToList();
            else
                SiteIds = new List<int> { SiteId };
    }

    private void ConvertCurrencyId(IEnumerable<ServerInfo> serverInfos)
    {
        if (CurrencyIds == null)
            if (CurrencyId == 0) //Select All
                CurrencyIds = serverInfos.SelectMany(x => x.ICurrencyList).Distinct().ToList();
            else
                CurrencyIds = new List<int> { CurrencyId };
    }

}
