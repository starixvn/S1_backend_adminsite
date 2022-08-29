using System.Net;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using BackendSite.Service.Model;
using Microsoft.Extensions.Options;

namespace BackendSite.Service.Library
{
    public class AWSS3Helper
    {
        private readonly IAmazonS3 _client;
        private readonly ILogger<AWSS3Helper> _logger;

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
            _logger = provider.GetRequiredService<ILogger<AWSS3Helper>>();
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
        
        public async Task<bool> UploadFileAsync(IFormFile file, string bucketName, string key)
        {
            try
            {
                await using var fileStream = file.OpenReadStream();
                
                
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    InputStream = fileStream,
                    Key = key
                };

                var putResponse = await _client.PutObjectAsync(putRequest);
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
