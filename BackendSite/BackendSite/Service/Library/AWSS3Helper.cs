using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using BackendSite.Extensions;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Option;

namespace BackendSite.Service.Library
{
    public class AWSS3Helper
    {
        private readonly IAmazonS3 _client;
        private readonly ILogger<AWSS3Helper> _logger;

        public AWSS3Helper(IServiceProvider provider)
        {
            
            var configBll = provider.GetRequiredService<ConfigBLL>();
            var awsKeyConfig = configBll.GetConfigValue<AWSKeyConfig>("AWSS3Key");
            var proxyConfig = configBll.GetConfigValue<ProxyConfig>("ProxySetting");
            var proxyHost = proxyConfig.ProxyHost;
            var proxyPort = proxyConfig.ProxyPort;

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
                var success = putResponse.HttpStatusCode.IsSuccess();
                if (!success)
                {
                    _logger.LogError($"Upload to s3 failed, key: {key}, HttpStatusCode: {(int)putResponse.HttpStatusCode}");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError("Upload to s3 Exception", ex);

                return false;
            }
        }

        public async Task<bool> DeleteFileAsync(string bucketName, string key)
        {
            try
            {
                var putRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = key
                };

                var putResponse = await _client.DeleteObjectAsync(putRequest);
                var success = putResponse.HttpStatusCode.IsSuccess();
                if (!success)
                {
                    _logger.LogError($"Delete from s3 failed, key: {key}, HttpStatusCode: {(int)putResponse.HttpStatusCode}");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError("Delete from s3 Exception", ex);

                return false;
            }
        }
    }
}
