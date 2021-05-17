using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System.Collections.Generic;

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
        public IEnumerable<BannerInfoList> GetBannerList(int SiteId,int BannerTypeId)
        {
            return bannerManageService.GetBannerList(SiteId, BannerTypeId);

        }
        public BannerInfo GetBanner(int SiteId, string BannerName)
        {
            return bannerManageService.GetBanner(SiteId, BannerName);
        }
        public int UpdateBannerStatus(int SiteId, string BannerName, int Status, string User)
        {
            return bannerManageService.UpdateBannerStatus(SiteId, BannerName, Status, User);
        }
    }
}
