using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL
{
    public class DeepLinkBLL
    {
        private readonly DeepLinkService _deepLinkService;
        private readonly ServerInfoService _serverInfoService;
        public DeepLinkBLL(IServiceProvider provider)
        {
            _deepLinkService = provider.GetRequiredService<DeepLinkService>();
            _serverInfoService = provider.GetRequiredService<ServerInfoService>();
        }
        public IEnumerable<DeepLink> GetDeepLinkList()
        {
            return _deepLinkService.GetDeepLinkList();
        }
        public IEnumerable<GetDeepLinkRep> GetDeepLinkTable(int siteId, int deepLinkId,
            int pageNumber, int pageSize, out int totalPages, out int rowCount)
        {
            var deepLinkTable = GetDeepLink(siteId, deepLinkId,
                pageNumber, pageSize, out totalPages, out rowCount).ToList();
            var siteList = _serverInfoService.GetSiteList().ToList();
            foreach (int DeepLinkTableType in Enum.GetValues(typeof(DeepLinkTableTypeEnum)))
            {
                if (deepLinkTable.Find(x => (int)x.Type == DeepLinkTableType) == null)
                {
                    deepLinkTable.Add(new GetDeepLinkRep()
                    {
                        SiteId = siteId,
                        SiteName = siteList.Find(x => x.SiteId == siteId).SiteName,
                        Domain = string.Empty,
                        Type = (DeepLinkTableTypeEnum)DeepLinkTableType,
                        ProtocolType = DeepLinkTableProtocolTypeEnum.HTTP,
                        DeepLinkId = deepLinkId
                    });
                }
            }
            return deepLinkTable.OrderBy(x => x.Type);
        }

        public IEnumerable<GetDeepLinkRep> GetDeepLink(int siteId, int deepLinkId,
            int pageNumber, int pageSize, out int totalPages, out int rowCount)
        {
            return _deepLinkService.GetDeepLinkTable(siteId, deepLinkId, (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages, out rowCount);
        }

        public  async Task<bool> SetDeepLink(int siteId, DeepLinkTableTypeEnum type, string domain, DeepLinkTableProtocolTypeEnum protocolType, int deepLinkId, string lastUpdateUser)
        {
            return await _deepLinkService.SetDeepLink(siteId, type, domain, protocolType, deepLinkId, lastUpdateUser);
        }
    }
}
