using System;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public class LiveRegistrationBLL
    {
        private readonly LiveRegistrationService _registrationService;

        public LiveRegistrationBLL(LiveRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        public async Task<LiveRegistrationResponse> GetRegLiveStatusData(int selectSite, int currency, int depositStatus, int status, string username, int pageNumber, int pageSize, int sort, DateTimeOffset startDate, DateTimeOffset endDate, string affiliatesCode, int agentCodeType)
        {
            var searchResult = await _registrationService.GetRegLiveStatusData(selectSite, currency, depositStatus, status, username, pageNumber, pageSize, sort, startDate, endDate, affiliatesCode, agentCodeType);
            var regLiveStatus = searchResult.RegLiveStatusData.Select(a => new LiveRegistration()
            {
                SiteId = a.SiteId,
                CustId = a.CustId,
                Currency = a.Currency,
                UserName = a.UserName,
                AccountName = a.AccountName,
                Agent = a.Agent,
                AffiliatesCode = a.AffCode,
                PhoneNo = a.PhoneNo,
                PhoneVerified = a.PhoneVerified,
                DepositStatus = a.ActStatus,
                Status = a.Status,
                RegTime = new DateTimeOffset(a.RegTime, TimeSpan.Zero)
            });
            return new LiveRegistrationResponse()
            {
                LiveRegistration = regLiveStatus.ToList(),
                PageNumber = searchResult.PageNumber,
                TotalPages = searchResult.TotalPages,
                RowCount = searchResult.RowCount
            };
        }
    }
}