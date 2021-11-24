using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog.Context;

namespace BackendSite.Extensions.Middlewares
{
    public class RequestLogMiddleware
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly RequestDelegate _next;
        private readonly AuthorizeBLL _authorizeBLL;
        private readonly ServerInfoBLL _serverInfoBLL;
        private readonly Tools _tools;

        #endregion

        #region Constructors

        public RequestLogMiddleware(RequestDelegate next,
                                    ILoggerFactory factory,
                                    AuthorizeBLL authorizeBLL,
                                    ServerInfoBLL serverInfoBLL,
                                    Tools tools)
        {
            _next = next;
            _logger = factory.CreateLogger("RequestLog");
            _authorizeBLL = authorizeBLL;
            _serverInfoBLL = serverInfoBLL;
            _tools = tools;
        }

        #endregion

        #region Action

        public async Task Invoke(HttpContext context,IWebHostEnvironment env)
        {
            var url = context.Request.GetDisplayUrl();
            var path = context.Request.Path.Value;
            
            if (env.IsProduction())
            {
                if (path.IndexOf("apiV2", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    throw new AdminException(ErrorCodeEnum.Fail);
                }
            }
            
            var requestBodyStream = new MemoryStream();
            var originalRequestBody = context.Request.Body;
            var content = string.Empty;
            if (context.Request.ContentType?.StartsWith("application/x-www-form-urlencoded") ?? false)
            {
                var formData = await context.Request.ReadFormAsync();
                content = JsonConvert.SerializeObject(formData);
            }

            await context.Request.Body.CopyToAsync(requestBodyStream);
            requestBodyStream.Seek(0, SeekOrigin.Begin);

            using (var requestBody = new StreamReader(requestBodyStream, Encoding.UTF8, true, 1024, true))
            {
                var codePage = requestBody.CurrentEncoding.CodePage;

                content = !context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase)
                          && string.IsNullOrEmpty(content)
                    ? await requestBody.ReadToEndAsync()
                    : content;

                var authData = _authorizeBLL.GetAuthData();
                LogContext.PushProperty("UserId", authData?.UserId);
                LogContext.PushProperty("UserName", authData?.UserName);
                {
                    _logger.LogInformation(
                        $"REQUEST METHOD: {context.Request.Method}, CodePage: {codePage}, REQUEST BODY: {content}, REQUEST URL: {url}");
                }
            }

            requestBodyStream.Seek(0, SeekOrigin.Begin);
            context.Request.Body = requestBodyStream;

            await _next(context);
            context.Request.Body = originalRequestBody;
            //}
        }

        #endregion
    }
}
