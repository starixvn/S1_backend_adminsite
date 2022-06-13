using BackendSite.Service.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using BackendSite.Service.Model.TpMaintain;
using TpGameType = BackendSite.Service.Model.TpMaintain.TpGameType;
using Newtonsoft.Json;
using BackendSite.Service.Library;
using Amazon.S3.Model;
using System.Net;
using Microsoft.Extensions.Options;
using BackendSite.Service.Model;
using System.Text;
using System.IO;

namespace BackendSite.Service.BLL
{
    public class SiteMaintainBLL
    {
        private readonly S3Helper _s3Helper;
        private readonly IOptions<Dictionary<string, SiteConfig>> _siteConfig;
        private readonly ServerInfoBLL _serverInfo;

        public SiteMaintainBLL(IServiceProvider provider)
        {
            _s3Helper = provider.GetRequiredService<S3Helper>();
            _siteConfig = provider.GetRequiredService<IOptions<Dictionary<string, SiteConfig>>>();
            _serverInfo = provider.GetRequiredService<ServerInfoBLL>();
        }

        public async Task<SiteMaintainObject> GetSiteMaintainObject(int siteId)
        {
            var site = _serverInfo.GetServerInfoWithSiteId(siteId);

            var putRequest = new GetObjectRequest
            {
                BucketName = "ailab-errorpage",
                Key = $"{site.SiteName.ToUpper()}/maintain.json"
            };

            var putResponse = await _s3Helper.Get(putRequest);
            using StreamReader sr = new StreamReader(putResponse.ResponseStream);
            using JsonReader reader = new JsonTextReader(sr);
            JsonSerializer serializer = new JsonSerializer();
            var response = serializer.Deserialize<SiteMaintainObject>(reader);
            return response;
        }

        public async Task<bool> UpdateSiteMaintainTime(int siteId, bool maintain, DateTimeOffset begin, DateTimeOffset end)
        {
            var site = _serverInfo.GetServerInfoWithSiteId(siteId);

            var siteMaintainObject = new SiteMaintainObject()
            {
                Maintain = maintain,
                DateTimeStart = new DateTimeOffset(begin.ToUniversalTime().DateTime, new TimeSpan(8,0,0)).ToString("yyyy-MM-dd HH:mm:ss"),
                UTCTimeStart = begin.ToUniversalTime().ToUnixTimeSeconds().ToString(),
                DateTimeEnd = new DateTimeOffset(end.ToUniversalTime().DateTime, new TimeSpan(8, 0, 0)).ToString("yyyy-MM-dd HH:mm:ss"),
                UTCTimeEnd = end.ToUniversalTime().ToUnixTimeSeconds().ToString(),
            };

            var putRequest = new PutObjectRequest
            {
                BucketName = "ailab-errorpage",
                ContentType = "application/json",
                InputStream = GetStream(JsonConvert.SerializeObject(siteMaintainObject)),
                Key = $"{site.SiteName.ToUpper()}/maintain.json"
            };

            var putResponse = await _s3Helper.Put(putRequest);

            return putResponse.HttpStatusCode == HttpStatusCode.OK;
        }

        private Stream GetStream(string content)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(content));
        }

        public class SiteMaintainObject
        {
            public bool Maintain { get; set; }
            public string DateTimeStart { get; set; }
            public string UTCTimeStart { get; set; }
            public string DateTimeEnd { get; set; }
            public string UTCTimeEnd { get; set; }
        }



    }
}
