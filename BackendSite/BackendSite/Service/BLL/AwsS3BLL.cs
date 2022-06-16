using System;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BackendSite.Service.BLL
{
    public class AwsS3BLL
    {
        private readonly string _awsKey;
        private readonly string _awsSecretKey;
        private readonly AmazonS3Config _s3Config;
        private readonly ILogger<AwsS3BLL> _logger;
        
        public AwsS3BLL(IServiceProvider provider)
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var proxyHost = configuration.GetValue<string>("ProxyHost");
            var proxyPort = configuration.GetValue<int>("ProxyPort");
            _awsKey = configuration.GetValue<string>("AWSKey");
            _awsSecretKey = configuration.GetValue<string>("AwsSecretKey");
            _logger = provider.GetRequiredService<ILogger<AwsS3BLL>>();
            
            _s3Config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.APSoutheast1
            };
            
            if (!string.IsNullOrEmpty(proxyHost))
            {
                _s3Config.ProxyHost = proxyHost;
                _s3Config.ProxyPort = proxyPort;
            }
        }
        
        public async Task<bool> UploadFileAsync(IFormFile file, string bucketName, string key)
        {
            try
            {
                await using var fileStream = file.OpenReadStream();
                
                IAmazonS3 client = new AmazonS3Client(_awsKey, _awsSecretKey, _s3Config);
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    InputStream = fileStream,
                    Key = key
                };

                var putResponse = await client.PutObjectAsync(putRequest);
                var success = putResponse.HttpStatusCode == HttpStatusCode.OK;
                if (!success)
                {
                    _logger.LogError($"Upload to s3 failed, key: {key}, HttpStatusCode: {(int) putResponse.HttpStatusCode}");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError("Upload to s3 Exception", ex);
                
                return false;
            }
        }
    }
}