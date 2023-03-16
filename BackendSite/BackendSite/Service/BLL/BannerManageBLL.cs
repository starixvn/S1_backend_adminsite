using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class BannerManageBLL
    {
        private readonly BannerManageService bannerManageService;

        public BannerManageBLL(BannerManageService bannerManageService)
        {
            this.bannerManageService = bannerManageService;
        }
        public int SetBanner(ParamsWithLog<SetRequest> input )
        {
            return bannerManageService.SetBanner(input);
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
        public int UpdateBannerStatus(ParamsWithLog<UpdateStatusRequest> input)
        {
            return bannerManageService.UpdateBannerStatus(input);
        }
        
        public int DeleteBanner(ParamsWithLog<GetBannerRequest> input)
        {
            return bannerManageService.DeleteBanner(input);
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
