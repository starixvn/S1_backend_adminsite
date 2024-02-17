using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.Model.Option;

namespace BackendSite.Service.Library;

public class HttpHelper
{
    private readonly HttpClient _clientWithProxy;
    private readonly HttpClient _client;
    public HttpHelper(IHttpClientFactory clientFactory, ConfigBLL configBll)
    {
        var proxyConfig = configBll.GetConfigValue<ProxyConfig>("ProxySetting");
        
        _clientWithProxy = !string.IsNullOrEmpty(proxyConfig.ProxyHost)
            ? clientFactory.CreateClient("with_proxy")
            : clientFactory.CreateClient();
        
        _client = clientFactory.CreateClient();
    }

    public HttpClient GetClient() => _client;
    public HttpClient GetWithProxyClient() => _clientWithProxy;

    public HttpClient GetClientWithHandler(HttpClientHandler handler) => new HttpClient(handler);
}