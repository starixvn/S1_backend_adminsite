using Amazon;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using BackendSite.Service.Model;
using Microsoft.Extensions.Options;

namespace BackendSite.Service.Library
{
    public class AWSCloudFrontHelper
    {
        private readonly IAmazonCloudFront _client;

        public AWSCloudFrontHelper(IServiceProvider provider)
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var awsKeyConfig = provider.GetRequiredService<IOptions<Dictionary<string, AWSKeyConfig>>>().Value["ailab-cloudfront"];
            var proxyHost = configuration.GetValue<string>("ProxyHost");
            var proxyPort = configuration.GetValue<int>("ProxyPort");

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
