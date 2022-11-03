using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.Options;

namespace BackendSite.Service.DAL
{
    public class AffiliateApiService : IAffiliateApiService
    {
        private readonly AffConfig _affConfig;
        private readonly CourierAdapter _courierAdapter;
        public AffiliateApiService(IOptions<AffConfig> affConfig, CourierAdapter courierAdapter)
        {
            _affConfig = affConfig.Value;
            _courierAdapter = courierAdapter;
        }

        public async Task<AffInfo> QueryAffInfo(string userName, string currency, int site)
        {
            var token = await GetAuthToken();
            if (token == null)
                return null;
            var affInfoUrl = $"{_affConfig.ApiUrl}/api/AffInfo?access_token={token.access_token}";
            var now = DateTime.UtcNow;
            var siteName = (AffSiteEnum)site;
            var data = new
            {
                PlayerName = userName,
                MonthDate = new DateTime(now.Year, now.Month, now.Day),
                BaseCurrencyName = currency,
                LicenseeCode = siteName
            };
            var apiResult = await _courierAdapter.PostAsync<AffInfo, object>(affInfoUrl, data);
            return apiResult;
        }

        private async Task<AffToken> GetAuthToken()
        {
            try
            {
                var authUrl = $"{_affConfig.ApiUrl}/api/auth";
                var data = new
                {
                    username = _affConfig.UserName,
                    password = _affConfig.Password
                };
                var affToken = await _courierAdapter.PostAsync<AffToken, object>(authUrl, data);
                return affToken;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}