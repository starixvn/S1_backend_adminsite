using Amazon;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Option;

namespace BackendSite.Service.Library
{
    public class AWSCloudFrontHelper
    {
        private readonly IAmazonCloudFront _client;

        public AWSCloudFrontHelper(IServiceProvider provider)
        {
            var configBll = provider.GetRequiredService<ConfigBLL>();
            var awsKeyConfig = configBll.GetConfigValue<AWSKeyConfig>("AWSCloudFrontKey");
            var proxyConfig = configBll.GetConfigValue<ProxyConfig>("ProxySetting");
            var proxyHost = proxyConfig.ProxyHost;
            var proxyPort = proxyConfig.ProxyPort;

            var config = new AmazonCloudFrontConfig
            {
                RegionEndpoint = RegionEndpoint.APSoutheast1
            };

            if (!string.IsNullOrEmpty(proxyHost))
            {
                config.ProxyHost = proxyHost;
                config.ProxyPort = proxyPort;
            }

            _client = new AmazonCloudFrontClient(awsKeyConfig.AccessKey, awsKeyConfig.SecretKey, config);
        }

        public async Task<CreateInvalidationResponse> CreateInvalidationAsync(CreateInvalidationRequest request)
        {
            return await _client.CreateInvalidationAsync(request);
        }
    }
}
