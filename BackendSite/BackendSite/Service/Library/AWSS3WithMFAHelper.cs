using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecurityToken.Model;
using Amazon.SecurityToken;
using BackendSite.Extensions;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Option;
using OtpNet;

namespace BackendSite.Service.Library
{
    public class AWSS3WithMFAHelper
    {
        private readonly ILogger<AWSS3WithMFAHelper> _logger;

        private readonly AWSKeyConfig _awsKeyConfig;
        private readonly AWSMFAConfig _awsTokenConfig;
        private readonly AmazonS3Config _amazonS3Config;

        public AWSS3WithMFAHelper(IServiceProvider provider)
        {

            var configBll = provider.GetRequiredService<ConfigBLL>();
            _awsKeyConfig = configBll.GetConfigValue<AWSKeyConfig>("AWSS3WithMFA_Key");
            _awsTokenConfig = configBll.GetConfigValue<AWSMFAConfig>("AWSS3WithMFA_Token");
            var proxyConfig = configBll.GetConfigValue<ProxyConfig>("ProxySetting");
            var proxyHost = proxyConfig.ProxyHost;
            var proxyPort = proxyConfig.ProxyPort;

            var region = configBll.GetConfigValue("AWSRegion");

            _amazonS3Config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(region)
            };

            if (!string.IsNullOrEmpty(proxyHost))
            {
                _amazonS3Config.ProxyHost = proxyHost;
                _amazonS3Config.ProxyPort = proxyPort;
            }

            _logger = provider.GetRequiredService<ILogger<AWSS3WithMFAHelper>>();
        }

        public async Task<PutObjectResponse> PutObjectAsync(PutObjectRequest request)
        {
            var client = await GetClientAsync();
            return await client.PutObjectAsync(request);
        }

        public async Task<DeleteObjectResponse> DeleteObjectAsync(DeleteObjectRequest request)
        {
            var client = await GetClientAsync();
            return await client.DeleteObjectAsync(request);
        }

        public async Task<GetObjectResponse> GetObjectAsync(GetObjectRequest request)
        {
            var client = await GetClientAsync();
            return await client.GetObjectAsync(request);
        }

        public async Task<CopyObjectResponse> CopyObjectAsync(CopyObjectRequest request)
        {
            var client = await GetClientAsync();
            return await client.CopyObjectAsync(request);
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
                var client = await GetClientAsync();
                var putResponse = await client.PutObjectAsync(putRequest);
                var success = putResponse.HttpStatusCode.IsSuccess();
                if (!success)
                {
                    _logger.LogError($"Upload to s3 failed, key: {key}, HttpStatusCode: {(int)putResponse.HttpStatusCode}");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload to s3 Exception");

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
                var client = await GetClientAsync();
                var putResponse = await client.DeleteObjectAsync(putRequest);
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

        #region Private Methods

        private async Task<AmazonS3Client> GetClientAsync()
        {
            var credentials = await GetCredentialsAsync();
            return new AmazonS3Client(credentials.AccessKeyId, credentials.SecretAccessKey, credentials.SessionToken, _amazonS3Config);
        }

        private async Task<Credentials> GetCredentialsAsync()
        {
            var tokenCode = new Totp(Base32Encoding.ToBytes(_awsTokenConfig.SecretKey)).ComputeTotp();
            var stsClient = new AmazonSecurityTokenServiceClient(_awsKeyConfig.AccessKey, _awsKeyConfig.SecretKey);
            var response = await stsClient.GetSessionTokenAsync(new GetSessionTokenRequest
            {
                DurationSeconds = _awsTokenConfig.DurationSeconds,
                SerialNumber = _awsTokenConfig.SerialNumber,
                TokenCode = tokenCode
            });

            return response.Credentials;
        }

        #endregion
    }
}
