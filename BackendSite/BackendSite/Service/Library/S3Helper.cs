using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BackendSite.Service.Library
{
    public class S3Helper
    {
        private readonly IAmazonS3 _client;
        public S3Helper(IServiceProvider provider)
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var awsKey = configuration.GetValue<string>("AWSKey");
            var awsSecretKey = configuration.GetValue<string>("AwsSecretKey");
            var proxyHost = configuration.GetValue<string>("ProxyHost");
            var proxyPort = configuration.GetValue<int>("ProxyPort");

            var config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.APSoutheast1
            };

            if (!string.IsNullOrEmpty(proxyHost))
            {
                config.ProxyHost = proxyHost;
                config.ProxyPort = proxyPort;
            }

            _client = new AmazonS3Client(awsKey, awsSecretKey, config);
        }

        public async Task<PutObjectResponse> Put(PutObjectRequest request)
        {
            return await _client.PutObjectAsync(request);
        }

        public async Task<DeleteObjectResponse> Delete(DeleteObjectRequest request)
        {
            var a = await _client.DeleteObjectAsync(request);
            return a;
        }

        public async Task<GetObjectResponse> Get(GetObjectRequest request)
        {
            return await _client.GetObjectAsync(request);
        }
    }
}
