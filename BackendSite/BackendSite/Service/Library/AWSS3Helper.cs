using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using BackendSite.Service.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendSite.Service.Library
{
    public class AWSS3Helper
    {
        private readonly IAmazonS3 _client;

        public AWSS3Helper(IServiceProvider provider)
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var awsKeyConfig = provider.GetRequiredService<IOptions<Dictionary<string, AWSKeyConfig>>>().Value["ailab-s3"];
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

            _client = new AmazonS3Client(awsKeyConfig.AccessKey, awsKeyConfig.SecretKey, config);
        }

        public async Task<PutObjectResponse> PutObjectAsync(PutObjectRequest request)
        {
            return await _client.PutObjectAsync(request);
        }

        public async Task<DeleteObjectResponse> DeleteObjectAsync(DeleteObjectRequest request)
        {
            return await _client.DeleteObjectAsync(request);
        }

        public async Task<GetObjectResponse> GetObjectAsync(GetObjectRequest request)
        {
            return await _client.GetObjectAsync(request);
        }

        public async Task<CopyObjectResponse> CopyObjectAsync(CopyObjectRequest request)
        {
            return await _client.CopyObjectAsync(request);
        }
    }
}
