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
using Newtonsoft.Json;

namespace BackendSite.Service.Library
{
    public class AWSS3Helper
    {
        private readonly ILogger<AWSS3Helper> _logger;
        private readonly AWSKeyConfig _awsKeyConfig;
        private readonly AWSMFAConfig _awsMFAKeyConfig;
        private readonly AmazonS3Config _amazonS3Config;

        public AWSS3Helper(IServiceProvider provider)
        {
            //取得s3 config
            var configBll = provider.GetRequiredService<ConfigBLL>();
            _awsKeyConfig = configBll.GetConfigValue<AWSKeyConfig>("AWSS3Key");
            _awsMFAKeyConfig = configBll.GetConfigValue<AWSMFAConfig>("AWSS3WithMFA");

            //設定region & config
            var region = configBll.GetConfigValue("AWSRegion");
            var proxyConfig = configBll.GetConfigValue<ProxyConfig>("ProxySetting");
            var proxyHost = proxyConfig.ProxyHost;
            var proxyPort = proxyConfig.ProxyPort;

            _amazonS3Config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(region)
            };

            if (!string.IsNullOrEmpty(proxyHost))
            {
                _amazonS3Config.ProxyHost = proxyHost;
                _amazonS3Config.ProxyPort = proxyPort;
            }

            _logger = provider.GetRequiredService<ILogger<AWSS3Helper>>();
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

        private async Task<AmazonS3Client> GetClientAsync()
        {

            if (_awsMFAKeyConfig != null)
            {
                _logger.LogInformation($"AwsMFAKeyConfig={JsonConvert.SerializeObject(_awsMFAKeyConfig)}");
                var stsClient = new AmazonSecurityTokenServiceClient(_awsMFAKeyConfig.AccessKey, _awsMFAKeyConfig.SecretAccessKey);
                var pass = false;
                var credentials = new Credentials();
                //int getTimes = 5;
                //while (!pass && getTimes > 0)
                while (!pass)
                {
                    try
                    {
                        var tokenCode = new Totp(Base32Encoding.ToBytes(_awsMFAKeyConfig.SecretKey), mode: OtpHashMode.Sha1).ComputeTotp();
                        //_logger.LogInformation($"TokenCode={tokenCode}");
                        var request = new GetSessionTokenRequest
                        {
                            DurationSeconds = _awsMFAKeyConfig.DurationSeconds,
                            SerialNumber = _awsMFAKeyConfig.SerialNumber,
                            TokenCode = tokenCode
                        };
                        //_logger.LogInformation($"GetSessionTokenRequest={JsonConvert.SerializeObject(request)}");
                        var response = await stsClient.GetSessionTokenAsync(request);
                        //_logger.LogInformation($"GetSessionTokenResponse={JsonConvert.SerializeObject(response)}");

                        credentials = response.Credentials;
                        pass = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message, ex);
                        //getTimes --;
                    }
                }
                return new AmazonS3Client(credentials.AccessKeyId, credentials.SecretAccessKey, credentials.SessionToken, _amazonS3Config);
            }
            else
            {
                return new AmazonS3Client(_awsKeyConfig.AccessKey, _awsKeyConfig.SecretKey, _amazonS3Config);
            }
        }
    }
}
