using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System.Collections.Generic;
using BackendSite.Controllers.ApiV2.CMS.Setting;

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
        public int SetBanner(BannerController.SetBannerReq bannerInfo, string user)
        {
            return bannerManageService.SetBanner(bannerInfo, user);
        }
        public IEnumerable<BannerInfoList> GetBannerList(int siteId,int bannerTypeId,int status)
        {
            return bannerManageService.GetBannerList(siteId, bannerTypeId, status);

        }
        public BannerInfo GetBanner(int siteId, string bannerName)
        {
            return bannerManageService.GetBanner(siteId, bannerName);
        }
        public int UpdateBannerStatus(int siteId, string bannerName, int status, string user)
        {
            return bannerManageService.UpdateBannerStatus(siteId, bannerName, status, user);
        }
    }
}
