using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Option;

namespace BackendSite.Service.BLL
{
    public class GaBLL : IGaBLL
    {
        private readonly CourierAdapter _courierAdapter;
        private readonly string _commonServiceUrl;

        public GaBLL(IServiceProvider provider)
        {
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            var configBll = provider.GetRequiredService<ConfigBLL>();
            _commonServiceUrl = configBll.GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").CommonService;
        }

        public async Task<ApiResult> PurchaseEventSendAsync(PurchaseRequest request)
        {
            var url = $"{_commonServiceUrl}GA/SendGaPurchase";
            var purchaseResult = await _courierAdapter.MyPostAsync<ApiResult, PurchaseRequest>(url, request);
            return purchaseResult;
        }
    }
}
