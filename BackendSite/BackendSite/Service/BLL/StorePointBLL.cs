using System;
using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Store.StorePoint;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Service.BLL
{
    public class StorePointBLL
    {
        private readonly StoreService _storeService;
        private readonly CourierAdapter _courierAdapter;
        private readonly string _storeSiteUrl;

        public StorePointBLL(IServiceProvider provider)
        {
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            _storeService = provider.GetRequiredService<StoreService>();
            var configuration = provider.GetRequiredService<IConfiguration>();
            _storeSiteUrl = configuration.GetValue<string>("StoreService");
        }
        
        public Task<ApiResult<object>> GetMemberListAsync(GetMemberListRequest req)
        {
            var url = _storeSiteUrl + "/api/Member/GetList";
            var result = _courierAdapter.MyPostAsync<object, GetMemberListRequest>(url, req);

            return result;
        }
        
        public Task<ApiResult<object>> GetMemberRewardPointLogAsync(GetMemberRewardPointLogRequest req)
        {
            var url = _storeSiteUrl + "/api/Member/GetMemberRewardPointLog";
            var result = _courierAdapter.MyPostAsync<object, GetMemberRewardPointLogRequest>(url, req);

            return result;
        }
    }
}