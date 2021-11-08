using BackendSite.Service.Model;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace BackendSite.Service.Library
{ 
    public class CourierAdapter
    {
        private readonly ILogger<CourierAdapter> _logger;
        public IConfiguration _configuration;

        public CourierAdapter(ILogger<CourierAdapter> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
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

        //public async Task<ApiResult<T>> PostMembersite<T, U>(string url, U oTpInfo)
        //{
        //    ApiResult<T> oApiResult = null;
        //    using var client = new HttpClient();
        //    var fooJSON = JsonConvert.SerializeObject(oTpInfo);
        //    var fooContent = new StringContent(fooJSON, Encoding.UTF8, "application/json");
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SignKey"]));
        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //    var token = new JwtSecurityToken(
        //    issuer: _configuration["JwtSettings:Issuer"],
        //    expires: DateTime.Now.AddMinutes(30),
        //    signingCredentials: creds);
        //    var Token = new JwtSecurityTokenHandler().WriteToken(token);
        //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
        //    _logger.LogInformation($"url:{url}, request:{fooJSON}");
        //    using var response = await client.PostAsync(url, fooContent);
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var responseBody = await response.Content.ReadAsStringAsync();
        //        _logger.LogInformation($"response:{responseBody}");
        //        oApiResult = JsonConvert.DeserializeObject<ApiResult<T>>(responseBody);
        //    }

        //    return oApiResult;
        //}
    }
}
