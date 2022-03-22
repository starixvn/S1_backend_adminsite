using System;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System.Collections.Generic;
using BackendSite.Controllers.ApiV2.CMS.Setting;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class BannerManageBLL
    {
        private readonly BannerManageService bannerManageService;
        private readonly ServerInfoBLL serverInfoBLL;

        public BannerManageBLL(BannerManageService bannerManageService, ServerInfoBLL serverInfoBLL)
        {
            this.bannerManageService = bannerManageService;
            this.serverInfoBLL = serverInfoBLL;
        }
        public int SetBanner(BannerInfo bannerInfo, string user)
        {
            return bannerManageService.SetBanner(bannerInfo, user);
        }
        public IEnumerable<BannerInfoList> GetBannerList(IEnumerable<int> siteId, IEnumerable<int> bannerTypeId,
            IEnumerable<int> status, IEnumerable<int> currency, int pageNumber, int pageSize, out int totalPages, out int rowCount)
        {
            return bannerManageService.GetBannerList(JsonConvert.SerializeObject(siteId),
                JsonConvert.SerializeObject(bannerTypeId),
                JsonConvert.SerializeObject(status),
                JsonConvert.SerializeObject(currency),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize), out totalPages, out rowCount);

        }
        public BannerInfo GetBanner(int siteId, string bannerName ,int currency)
        {
            return bannerManageService.GetBanner(siteId, bannerName, currency);
        }
        public int UpdateBannerStatus(int siteId, string bannerName, int status, string user)
        {
            return bannerManageService.UpdateBannerStatus(siteId, bannerName, status, user);
        }
        public IEnumerable<HotEventList> GetHotEventList(IEnumerable<int> siteId,
            IEnumerable<int> status, int pageNumber, int pageSize, out int totalPages, out int rowCount)
        {
            return bannerManageService.GetHotEventList(JsonConvert.SerializeObject(siteId),
                JsonConvert.SerializeObject(status),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize), out totalPages, out rowCount);

        }
    }
}
