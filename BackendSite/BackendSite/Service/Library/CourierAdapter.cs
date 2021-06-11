using BackendSite.Service.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BackendSite.Service.Library
{
    public class CourierAdapter
    {
        public async Task<ApiResult<T>> MyPostAsync<T, U>(string url, U oTpInfo, int timeout = 0)
        {
            ApiResult<T> oApiResult = null;
            HttpClient client = new HttpClient();
            var fooJSON = JsonConvert.SerializeObject(oTpInfo);
            var fooContent = new StringContent(fooJSON, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (timeout != 0)
            {
                client.Timeout = TimeSpan.FromSeconds(timeout);
            }

            HttpResponseMessage response = await client.PostAsync(url, fooContent);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                oApiResult = JsonConvert.DeserializeObject<ApiResult<T>>(responseBody);
            }

            return oApiResult;
        }
    }
}
