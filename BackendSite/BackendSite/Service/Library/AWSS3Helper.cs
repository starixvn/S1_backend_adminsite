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
using BackendSite.Service.Model.enums;
using System.Collections.Concurrent;

namespace BackendSite.Service.Library
{
    public class AWSS3Helper
    {
        private readonly ILogger<AWSS3Helper> _logger;
        private readonly AWSKeyConfig _awsKeyConfig;
        private readonly AWSMFAConfig _awsMFAKeyConfig;
        private readonly AmazonS3Config _amazonS3Config;
        private readonly ICacheHelper _cacheHelper;
        private readonly string _redisKey = "AWSS3WithMFACredential";
        private readonly SemaphoreSlim _lock;
        private ConcurrentDictionary<string, Task<Credentials>> _credentialTasks = new ConcurrentDictionary<string, Task<Credentials>>();


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
            _cacheHelper = provider.GetRequiredService<ICacheHelper>();

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
            _lock = new SemaphoreSlim(1, 1);
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
            var key = $"BackEndSite:{_redisKey}";
            if (_awsMFAKeyConfig != null)
            {
                //var credentials = _cacheHelper.TryGetOrCreate(key, TimeSpan.FromMinutes(20),
                //() => GetCredentialsAsync().Result);

                //if (credentials.AccessKeyId is null)
                //{
                //    credentials = GetCredentialsAsync().Result;
                //    await _cacheHelper.StringSetAsync(key, JsonConvert.SerializeObject(credentials), TimeSpan.FromMinutes(20));
                //}

                //return new AmazonS3Client(credentials.AccessKeyId, credentials.SecretAccessKey, credentials.SessionToken, _amazonS3Config);
                // Try getting in-progress task first
                if (!_credentialTasks.TryGetValue(key, out var credentialTask))
                {
                    // Lock to prevent race conditions
                    await _lock.WaitAsync();
                    try
                    {
                        // Recheck cache inside the lock
                        credentialTask = _credentialTasks.GetOrAdd(key, async _ =>
                        {
                            var cachedCredentials = _cacheHelper.TryGetOrCreate(key, TimeSpan.FromMinutes(20), () => GetCredentialsAsync().Result);

                            if (cachedCredentials.AccessKeyId is null)
                            {
                                var newCredentials = await GetCredentialsAsync();
                                await _cacheHelper.StringSetAsync(key, JsonConvert.SerializeObject(newCredentials), TimeSpan.FromMinutes(20));
                                return newCredentials;
                            }

                            return cachedCredentials;
                        });
                    }
                    finally
                    {
                        _lock.Release();
                    }
                }

                var credentials = await credentialTask;
                return new AmazonS3Client(credentials.AccessKeyId, credentials.SecretAccessKey, credentials.SessionToken, _amazonS3Config);
            }
            else
            {
                return new AmazonS3Client(_awsKeyConfig.AccessKey, _awsKeyConfig.SecretKey, _amazonS3Config);
            }
        }

        private async Task<Credentials> GetCredentialsAsync()
        {
            _logger.LogInformation($"AwsMFAKeyConfig={JsonConvert.SerializeObject(_awsMFAKeyConfig)}");
            var stsClient = new AmazonSecurityTokenServiceClient(_awsMFAKeyConfig.AccessKey, _awsMFAKeyConfig.SecretAccessKey);
            var pass = false;
            var credentials = new Credentials();
            int getTimes = 5;
            while (!pass && getTimes > 0)
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
                    getTimes--;
                }
            }
            return credentials;
        }
    }
}
