using BackendSite.Service.DAL;

namespace BackendSite.Service.BLL
{
    public class HomeBLL
    {
        private readonly HomeService homeService;
        public HomeBLL(HomeService homeService)
        {
            this.homeService = homeService;
        }
        public int GetRegisteredCustomers(int SiteId, long StartDate, long EndDate)
        {
            return homeService.GetRegisteredCustomers(SiteId, StartDate, EndDate);
        }
        public int GetepositCustomers(int SiteId, long StartDate, long EndDate)
        {
            return homeService.GetDepositCustomers(SiteId, StartDate, EndDate);
        }
    }
}
