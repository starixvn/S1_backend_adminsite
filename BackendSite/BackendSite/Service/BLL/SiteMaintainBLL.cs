using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using BackendSite.Service.Library;
using Amazon.S3.Model;
using System.Net;
using System.Text;
using System.IO;
using BackendSite.Service.Model.enums;
using Amazon.CloudFront.Model;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using BackendSite.Service.Model;
using BackendSite.Extensions;

namespace BackendSite.Service.BLL
{
    public class SiteMaintainBLL
    {
        private readonly IOptions<Dictionary<string, SiteConfig>> _siteConfig;
        private readonly AWSS3Helper _s3Helper;
        private readonly AWSCloudFrontHelper _cloudFrontHelper;
   
        public SiteMaintainBLL(IServiceProvider provider)
        {
            _siteConfig = provider.GetRequiredService<IOptions<Dictionary<string, SiteConfig>>>();
            _s3Helper = provider.GetRequiredService<AWSS3Helper>();
            _cloudFrontHelper = provider.GetRequiredService<AWSCloudFrontHelper>();
        }

        public async Task<SiteMaintainResponse> GetSiteMaintain(SiteEnum site)
        {
            var putRequest = new GetObjectRequest
            {
                BucketName = "ailab-errorpage",
                Key = $"{site}/maintain.json"
            };

            var putResponse = await _s3Helper.GetObjectAsync(putRequest);
            using StreamReader sr = new StreamReader(putResponse.ResponseStream);
            using JsonReader reader = new JsonTextReader(sr);
            JsonSerializer serializer = new JsonSerializer();
            var response = serializer.Deserialize<SiteMaintainResponse>(reader);
            return response;
        }

        public async Task<bool> UpdateSiteMaintain(SiteMaintainRequest request)
        {
            var siteMaintainObject = new SiteMaintainResponse()
            {
                Maintain = request.Maintain,
                DateTimeStart = new DateTimeOffset(request.BeginTime.ToUniversalTime().DateTime, new TimeSpan(8,0,0)).ToString("yyyy-MM-dd HH:mm:ss"),
                UTCTimeStart = request.BeginTime.ToUniversalTime().ToUnixTimeSeconds().ToString(),
                DateTimeEnd = new DateTimeOffset(request.EndTime.ToUniversalTime().DateTime, new TimeSpan(8, 0, 0)).ToString("yyyy-MM-dd HH:mm:ss"),
                UTCTimeEnd = request.EndTime.ToUniversalTime().ToUnixTimeSeconds().ToString(),
            };

            //update s3
            var putRequest = new PutObjectRequest
            {
                BucketName = "ailab-errorpage",
                ContentType = "application/json",
                InputStream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(siteMaintainObject))),
                Key = $"{request.SiteId}/maintain.json"
            };
            var putResponse = await _s3Helper.PutObjectAsync(putRequest);      
            if (!putResponse.HttpStatusCode.IsSuccess())
            {
                return false;
            }

            //clear cache
            var invalidationRequest = new CreateInvalidationRequest()
            {
                DistributionId = _siteConfig.Value[request.SiteId.GetDescription()].AwsCloudFrontErrorDomainID,
                InvalidationBatch = new InvalidationBatch()
                {
                    CallerReference = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
                    Paths = new Paths
                    {
                        Quantity = 1,
                        Items = new List<string> { "/maintain.json" }
                    }
                }
            };
            var createResponse = await _cloudFrontHelper.CreateInvalidationAsync(invalidationRequest);
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
            public string DateTimeStart { get; set; }
            /// <summary>
            /// 維護開始時間 UnixTime Seconds (+0)
            /// </summary>
            public string UTCTimeStart { get; set; }
            /// <summary>
            /// 維護結束時間(+8)
            /// </summary>
            public string DateTimeEnd { get; set; }
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
