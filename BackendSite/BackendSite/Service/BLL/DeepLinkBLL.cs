using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public IEnumerable<GetDeepLinkRep> GetDeepLinkTable(int siteId,int deepLinkId)
        {
            var deepLinkTable = _deepLinkService.GetDeepLinkTable(siteId, deepLinkId).ToList();
            var siteList = _serverInfoService.GetSiteList().ToList();
            foreach (int DeepLinkTableType in Enum.GetValues(typeof(DeepLinkTableTypeEnum)))
            {
                if(deepLinkTable.Find(x=> (int)x.Type == DeepLinkTableType) == null)
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
            return deepLinkTable.OrderBy(x=> x.Type);
        }
        public int SetDeepLink(int siteId, DeepLinkTableTypeEnum type, string domain, DeepLinkTableProtocolTypeEnum protocolType, int deepLinkId, string lastUpdateUser)
        {
            return _deepLinkService.SetDeepLink(siteId, type, domain, protocolType, deepLinkId, lastUpdateUser);
        }
    }
}
