using BackendSite.Service.Model;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BackendSite.Service.Library
{
    public class CourierAdapter
    {
        private readonly ILogger<CourierAdapter> _logger;

        public CourierAdapter(ILogger<CourierAdapter> logger)
        {
            _logger = logger;
        }

        public async Task<ApiResult<T>> MyPostAsync<T, U>(string url, U oTpInfo, int timeout = 0)
        {
            ApiResult<T> oApiResult = null;
            using var client = new HttpClient();
            var fooJSON = JsonConvert.SerializeObject(oTpInfo);
            var fooContent = new StringContent(fooJSON, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (timeout != 0)
            {
                client.Timeout = TimeSpan.FromSeconds(timeout);
            }
            _logger.LogInformation($"url:{url}, request:{fooJSON}");
            using var response = await client.PostAsync(url, fooContent);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"response:{responseBody}");
                oApiResult = JsonConvert.DeserializeObject<ApiResult<T>>(responseBody);
            }
            return oApiResult;
        }
    }
}
