using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Option;

namespace BackendSite.Service.BLL;

public class FaceBookPixelBLL
{
    private readonly string _commonServiceUrl;
    private readonly CourierAdapter _courierAdapter;

    public FaceBookPixelBLL(IServiceProvider provider)
    {
        var configBll = provider.GetRequiredService<ConfigBLL>();
        _commonServiceUrl = configBll.GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").CommonService;
        _courierAdapter = provider.GetRequiredService<CourierAdapter>();
    }
    
    public async Task<object> Send(PixelRequest req)
    {
        var url = _commonServiceUrl + "FaceBookPixel/Send";
        var result = await _courierAdapter.PostAsync<ApiResult, PixelRequest>(url, req);

        return result;
    }

    public PixelRequest CreatePixelRequest(int custId, int currencyId, decimal? value, IEnumerable<FaceBookPixel> pixelEvent)
    {
        return new PixelRequest
        {
            CustId = custId,
            CurrencyId = currencyId,
            Value = value,
            PixelEvent = pixelEvent
        };
    }
}