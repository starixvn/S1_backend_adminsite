using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.DAL
{
    public class AffiliateApiService : IAffiliateApiService
    {
        private readonly AffConfig _affConfig;
        private readonly CourierAdapter _courierAdapter;
        private readonly ICacheHelper _cacheHelper;
        private readonly ILogger<AffiliateApiService> _logger;
        private const string TokenKey = "AffAuthToken";
        public AffiliateApiService(AffConfig affConfig, CourierAdapter courierAdapter, ICacheHelper cacheHelper, ILogger<AffiliateApiService> logger)
        {
            _affConfig = affConfig;
            _courierAdapter = courierAdapter;
            _cacheHelper = cacheHelper;
            _logger = logger;
        }

        public async Task<AffInfo> QueryAffInfo(string userName, string currency, int site)
        {
            var token = await _cacheHelper.StringGetAsync(TokenKey);
            if (string.IsNullOrEmpty(token))
            {
                var affToken = await GetAuthToken();
                if (affToken == null)
                {
                    _logger.LogError("aff token is null");
                    return null;
                }
                await _cacheHelper.StringSetAsync(TokenKey, affToken.access_token, TimeSpan.FromMinutes(25));
                token = affToken.access_token;
            }

            var affInfoUrl = $"{_affConfig.ApiUrl}/api/AffInfo?access_token={token}";
            var now = DateTime.UtcNow;
            var siteName = (AffSiteEnum)site;
            var data = new
            {
                PlayerName = userName,
                MonthDate = new DateTime(now.Year, now.Month, now.Day),
                BaseCurrencyName = currency,
                LicenseeCode = siteName.ToString()
            };
            try
            {
                var apiResult = await _courierAdapter.PostAsync<AffInfo, object>(affInfoUrl, data);
                return apiResult;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "aff AffInfo is failed");
                return null;
            }
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
                _logger.LogError(e, "get auth token failed");
                return null;
            }
        }
    }
}