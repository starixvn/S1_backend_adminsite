using System;
using System.Threading.Tasks;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Store.Redemption;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BackendSite.Service.BLL
{
    public class RedemptionBLL
    {
        private readonly ILogger<RedemptionBLL> _logger;
        private readonly CourierAdapter _courierAdapter;
        private readonly string _storeSiteUrl;
        

        public RedemptionBLL(IServiceProvider provider)
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            _logger = provider.GetRequiredService<ILogger<RedemptionBLL>>();
            _storeSiteUrl = configuration.GetValue<string>("StoreService");
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
        }

        public Task<ApiListResult<object>> GetRedemptionListAsync(GetRedemptionListRequest req)
        {
            var url = _storeSiteUrl + "/api/Redemption/GetRedemptionList";
            var result = _courierAdapter.PostAsync<ApiListResult<object>, GetRedemptionListRequest>(url, req);

            return result;
        }
        
        public Task<ApiResult<object>> GetRedemptionDetailAsync(GetRedemptionDetailRequest req)
        {
            var url = _storeSiteUrl + "/api/Redemption/GetRedemptionDetail";
            var result = _courierAdapter.PostAsync<ApiResult<object>, GetRedemptionDetailRequest>(url, req);

            return result;
        }
    }
}