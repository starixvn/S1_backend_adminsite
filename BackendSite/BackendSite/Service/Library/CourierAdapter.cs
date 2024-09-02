using System.Net.Http.Headers;
using System.Text;

using BackendSite.Service.Model;

using Newtonsoft.Json;

namespace BackendSite.Service.Library
{
    public class CourierAdapter
    {
        private readonly ILogger<CourierAdapter> _logger;

        public CourierAdapter(IServiceProvider provider)
        {
            _logger = provider.GetRequiredService<ILogger<CourierAdapter>>();
        }

        public async Task<ApiResult<T>> MyPostAsync<T, U>(string url, U oTpInfo, int timeout = 0)
        {
            try
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
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        public async Task<TResponse> PostAsync<TResponse, TRequest>(string url, TRequest request, int timeout = 0)
        {
            try
            {
                TResponse result = default;
                using var client = new HttpClient();
                var jsonReq = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonReq, Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (timeout != 0)
                {
                    client.Timeout = TimeSpan.FromSeconds(timeout);
                }

                _logger.LogInformation($"url:{url}, request:{jsonReq}");
                using var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"response:{responseBody}");
                    result = JsonConvert.DeserializeObject<TResponse>(responseBody);
                }

                return result;

            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }
        
        public async Task<HttpResponseMessage> GetAsync(string url, Dictionary<string, string> headerParams, string contentType)
        {
            HttpClientHandler handler;
            
                handler = new HttpClientHandler();
            

            using var client = new HttpClient(handler);
            if (contentType != null)
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
            }

            if (headerParams != null)
            {
                foreach (var param in headerParams)
                {
                    client.DefaultRequestHeaders.Add(param.Key, param.Value);
                }
            }
            
            _logger.LogInformation($"url:{url}");
            using var data = new HttpRequestMessage(HttpMethod.Get, url);
            data.Content = new StringContent("", Encoding.UTF8, contentType);
            var response = await client.SendAsync(data);
            _logger.LogInformation($"HttpHelper.Request.Response:{response.Content.ReadAsStringAsync().Result}");
            return response;
        }
    }
}
