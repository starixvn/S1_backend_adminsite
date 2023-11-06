using System.Data;
using System.Text;
using Amazon.CloudFront.Model;
using Amazon.S3.Model;
using BackendSite.Extensions;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class SiteMaintainBLL
    {
        private readonly Dictionary<string, SiteCloudFrontImageInfo> _siteConfig;
        private readonly AWSS3Helper _s3Helper;
        private readonly AWSCloudFrontHelper _cloudFrontHelper;
        private readonly string _fileName;
        private readonly string _errorPageBucketName;
        private readonly MySqlClient _mainDbClient;

        public SiteMaintainBLL(IServiceProvider provider)
        {
            _siteConfig = provider.GetRequiredService<Dictionary<string, SiteCloudFrontImageInfo>>();
            _s3Helper = provider.GetRequiredService<AWSS3Helper>();
            _cloudFrontHelper = provider.GetRequiredService<AWSCloudFrontHelper>();
            var configBll = provider.GetRequiredService<ConfigBLL>();
            _errorPageBucketName = configBll.GetConfigValue("AWSErrorPageBucketName");

            var env = provider.GetRequiredService<IWebHostEnvironment>();
            _fileName = env.IsProduction() ? "maintain.json" : "maintain_test.json";
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
        }

        public async Task<SiteMaintainResponse> GetSiteMaintain(SiteEnum site)
        {
            var putRequest = new GetObjectRequest
            {
                BucketName = _errorPageBucketName,
                Key = $"{site}/{_fileName}"
            };

            var putResponse = await _s3Helper.GetObjectAsync(putRequest);
            using StreamReader sr = new StreamReader(putResponse.ResponseStream);
            using JsonReader reader = new JsonTextReader(sr);
            JsonSerializer serializer = new JsonSerializer();
            var response = serializer.Deserialize<SiteMaintainResponse>(reader);
            return response;
        }

        public async Task<bool> UpdateSiteMaintain(SiteMaintainRequest request, string Operator)
        {
            var siteMaintainObject = new
            {
                request.Maintain,
                DateTimeStart = request.BeginTime.ToOffset(new TimeSpan(8, 0, 0)).ToString("yyyy-MM-dd HH:mm:ss"),
                UTCTimeStart = request.BeginTime.ToUniversalTime().ToUnixTimeSeconds().ToString(),
                DateTimeEnd = request.EndTime.ToOffset(new TimeSpan(8, 0, 0)).ToString("yyyy-MM-dd HH:mm:ss"),
                UTCTimeEnd = request.EndTime.ToUniversalTime().ToUnixTimeSeconds().ToString(),
            };

            //update s3
            var putRequest = new PutObjectRequest
            {
                BucketName = _errorPageBucketName,
                ContentType = "application/json",
                InputStream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(siteMaintainObject))),
                Key = $"{request.SiteId}/{_fileName}"
            };
            var putResponse = await _s3Helper.PutObjectAsync(putRequest);
            if (!putResponse.HttpStatusCode.IsSuccess())
            {
                return false;
            }

            //clear cache
            var invalidationRequest = new CreateInvalidationRequest()
            {
                DistributionId = _siteConfig[request.SiteId.GetDescription()].AwsCloudFrontErrorDomainID,
                InvalidationBatch = new InvalidationBatch()
                {
                    CallerReference = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
                    Paths = new Paths
                    {
                        Quantity = 1,
                        Items = new List<string> { $"/{_fileName}" }
                    }
                }
            };
            var createResponse = await _cloudFrontHelper.CreateInvalidationAsync(invalidationRequest);
            if (createResponse.HttpStatusCode.IsSuccess())
            {
                #region InfoLog

                var after = new
                {
                    Maintain = request.Maintain,
                    BeginTime = request.BeginTime,
                    EndTime = request.EndTime,
                };
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", (int)request.SiteId);
                logParam.Add("_CurrencyId", 0);
                logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.SiteMaintenance);
                logParam.Add("_ActionType", (int)ActionType.Update);
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", null, DbType.String);
                logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(after));
                logParam.Add("_Operator", Operator);

                #endregion

                _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam);
            }
            return createResponse.HttpStatusCode.IsSuccess();
        }

        public class SiteMaintainResponse
        {
            /// <summary>
            /// 是否在維護中
            /// </summary>
            public bool Maintain { get; set; }

            /// <summary>
            /// 維護開始時間(+8)
            /// </summary>
            public DateTimeOffset DateTimeStart
            {
                get
                {
                    return new DateTimeOffset(_dateTimeStart, new TimeSpan(8, 0, 0));
                }
                set
                {
                    _dateTimeStart = value.DateTime;
                }
            }

            private DateTime _dateTimeStart { get; set; }

            /// <summary>
            /// 維護開始時間 UnixTime Seconds (+0)
            /// </summary>
            public string UTCTimeStart { get; set; }

            /// <summary>
            /// 維護結束時間(+8)
            /// </summary>
            public DateTimeOffset DateTimeEnd
            {
                get
                {
                    return new DateTimeOffset(_dateTimeEnd, new TimeSpan(8, 0, 0));
                }
                set
                {
                    _dateTimeEnd = value.DateTime;
                }
            }

            private DateTime _dateTimeEnd { get; set; }

            /// <summary>
            /// 維護結束時間 UnixTime Seconds (+0)
            /// </summary>
            public string UTCTimeEnd { get; set; }
        }

        public class SiteMaintainRequest
        {
            public SiteEnum SiteId { get; set; }
            public bool Maintain { get; set; }
            public DateTimeOffset BeginTime { get; set; }
            public DateTimeOffset EndTime { get; set; }
        }
    }
}
