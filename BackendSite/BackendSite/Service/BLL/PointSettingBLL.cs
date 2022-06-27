using System;
using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using Microsoft.Extensions.Configuration;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Store.PointSetting;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Service.BLL
{
    public class PointSettingBLL
    {
        private readonly CourierAdapter _courierAdapter;
        private readonly string _storeSiteUrl;

        public PointSettingBLL(IServiceProvider provider)
        {
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            var configuration = provider.GetRequiredService<IConfiguration>();
            _storeSiteUrl = configuration.GetValue<string>("StoreService");
        }

        public Task<ApiResult<object>> GetInactivePointExpirySettingAsync(GetInactivePointExpirySettingRequest req)
        {
            var url = _storeSiteUrl + "/api/PointSetting/GetInactivePointExpirySetting";
            var result = _courierAdapter.MyPostAsync<object, GetInactivePointExpirySettingRequest>(url, req);

            return result;
        }

        public async Task<ApiResult> SetInactivePointExpirySettingAsync(SetInactivePointExpirySettingRequest req,
            string user)
        {
            var url = _storeSiteUrl + "/api/PointSetting/SetInactivePointExpirySetting";
            var data = new SetInactivePointExpirySettingParams(req, user);
            var result = await _courierAdapter.MyPostAsync<object, SetInactivePointExpirySettingParams>(url, data);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        public Task<ApiResult<object>> GetTurnoverPointSettingListAsync(GetTurnoverPointSettingListRequest req)
        {
            var url = _storeSiteUrl + "/api/PointSetting/GetTurnoverPointSettingList";
            var result =
                _courierAdapter.MyPostAsync<object, GetTurnoverPointSettingListRequest>(url, req);

            return result;
        }

        public Task<ApiResult<object>> GetTurnoverPointSettingAsync(GetTurnoverPointSettingRequest req)
        {
            var url = _storeSiteUrl + "/api/PointSetting/GetTurnoverPointSetting";
            var result =
                _courierAdapter.MyPostAsync<object, GetTurnoverPointSettingRequest>(url, req);

            return result;
        }

        public async Task<ApiResult> SetTurnoverPointSettingAsync(SetTurnoverPointSettingRequest req, string user)
        {
            var url = _storeSiteUrl + "/api/PointSetting/SetTurnoverPointSetting";
            var data = new SetTurnoverPointSettingParams(req, user);
            var result = await _courierAdapter.MyPostAsync<object, SetTurnoverPointSettingParams>(url, data);

            return new ApiResult(result.ErrorCode, result.Message);
        }


        public Task<ApiResult<object>> GetDepositPointSettingAsync(GetDepositPointSettingRequest req)
        {
            var url = _storeSiteUrl + "/api/PointSetting/GetDepositPointSetting";
            var result =
                _courierAdapter.MyPostAsync<object, GetDepositPointSettingRequest>(url, req);

            return result;
        }

        public async Task<ApiResult> SetDepositPointSettingAsync(SetDepositPointSettingRequest req, string user)
        {
            var url = _storeSiteUrl + "/api/PointSetting/SetDepositPointSetting";
            var data = new SetDepositPointSettingParams(req, user);
            var result = await _courierAdapter.MyPostAsync<object, SetDepositPointSettingParams>(url, data);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        public Task<ApiResult<object>> GetTurnoverPointStatusSettingAsync(GetTurnoverPointStatusSettingRequest req)
        {
            var url = _storeSiteUrl + "/api/PointSetting/GetTurnoverPointStatusSetting";
            var result =
                _courierAdapter.MyPostAsync<object, GetTurnoverPointStatusSettingRequest>(url, req);

            return result;
        }

        public async Task<ApiResult> SetTurnoverPointStatusSettingAsync(SetTurnoverPointStatusSettingRequest req,
            string user)
        {
            var url = _storeSiteUrl + "/api/PointSetting/SetTurnoverPointStatusSetting";
            var data = new SetTurnoverPointStatusSettingParams(req, user);
            var result = await _courierAdapter.MyPostAsync<object, SetTurnoverPointStatusSettingParams>(url, data);

            return new ApiResult(result.ErrorCode, result.Message);
        }
    }
}