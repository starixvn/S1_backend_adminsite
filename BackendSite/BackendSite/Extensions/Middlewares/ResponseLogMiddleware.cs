using System.Diagnostics;
using System.Text;

using Newtonsoft.Json.Linq;

using Serilog.Context;

namespace BackendSite.Extensions.Middlewares
{
    public class ResponseLogMiddleware
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly RequestDelegate _next;

        #endregion

        #region Constructors

        public ResponseLogMiddleware(RequestDelegate next, ILoggerFactory factory)
        {
            _next = next;
            _logger = factory.CreateLogger("ResponseLog");
        }

        #endregion

        #region Action

        public async Task Invoke(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            var path = context.Request.Path.Value;
            //if (path == "/" && (context.Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase) ||
            //                    context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase)))
            //{
            //    await _next(context);
            //}
            //else
            //{
            var bodyStream = context.Response.Body;

            var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            await _next(context);

            responseBodyStream.Seek(0, SeekOrigin.Begin);
            var recordContent = await new StreamReader(responseBodyStream, Encoding.UTF8, true, 1024, true)
                .ReadToEndAsync();
            using (LogContext.PushProperty("ElapsedMilliseconds", sw.Elapsed.TotalMilliseconds))
            {
                if (path.Contains("ResetPassword", StringComparison.OrdinalIgnoreCase) ||
                    path.Contains("ResetPwd", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var jbody = JObject.Parse(recordContent);
                        if (jbody.TryGetValue("data", StringComparison.Ordinal, out var pwd)
                              && !string.IsNullOrEmpty(pwd.ToString()))
                        {
                            jbody["data"] = "********";
                        }
                        recordContent = jbody.ToString();
                    }
                    catch
                    {
                        // ignore
                    }
                }
                _logger.LogInformation($"RESPONSE: {recordContent}");
            }

            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(bodyStream);
            //}
        }

        #endregion
    }
}
