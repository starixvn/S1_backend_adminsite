using System.Text;

using BackendSite.Service.BLL;
using BackendSite.Service.Library;

using Microsoft.AspNetCore.Http.Extensions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Serilog.Context;

namespace BackendSite.Extensions.Middlewares
{
    public class RequestLogMiddleware
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly RequestDelegate _next;
        private readonly NewAuthorizeBLL _authorizeBLL;
        private readonly Tools _tools;

        #endregion

        #region Constructors

        public RequestLogMiddleware(RequestDelegate next,
                                    ILoggerFactory factory,
                                    NewAuthorizeBLL authorizeBLL,
                                    Tools tools)
        {
            _next = next;
            _logger = factory.CreateLogger("RequestLog");
            _authorizeBLL = authorizeBLL;
            _tools = tools;
        }

        #endregion

        #region Action

        public async Task Invoke(HttpContext context, IWebHostEnvironment env)
        {
            context.Request.EnableBuffering();
            var url = context.Request.GetDisplayUrl();
            var path = context.Request.Path.Value;

            var requestBodyStream = new MemoryStream();
            var originalRequestBody = context.Request.Body;
            var content = string.Empty;
            if (context.Request.ContentType?.StartsWith("application/x-www-form-urlencoded") ?? false)
            {
                var formData = await context.Request.ReadFormAsync();
                var dic = formData.ToDictionary(i => i.Key, i => i.Value);

                if (path.Contains("UpdatePassword", StringComparison.OrdinalIgnoreCase) &&
                    dic.TryGetValue("Password", out var pwd1) &&
                    !string.IsNullOrEmpty(pwd1.ToString()))
                {
                    dic["Password"] = "********";
                }

                content = JsonConvert.SerializeObject(dic);
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
                LogContext.PushProperty("ClientIp", _tools.GetClientIP(context));
                LogContext.PushProperty("UserId", authData?.UserId);
                LogContext.PushProperty("UserName", authData?.UserName);
                {
                    // log需將登入密碼隱碼
                    if (path.Contains("Signin", StringComparison.OrdinalIgnoreCase) ||
                        path.Contains("CreateUser", StringComparison.OrdinalIgnoreCase) ||
                        path.Contains("AddUser", StringComparison.OrdinalIgnoreCase) ||
                        path.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
                        path.Contains("ChangePwd", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var jbody = JObject.Parse(content);
                            var passWordCheckList = new List<string>
                            {
                                "Password", "password", "PassWord",
                                "userPwd", "txtPW", "OldPwd", "NewPwd",
                            };

                            foreach (var word in passWordCheckList)
                            {

                                if (jbody.TryGetValue(word, StringComparison.Ordinal, out var pwd)
                                && !string.IsNullOrEmpty(pwd.ToString()))
                                {
                                    jbody[word] = "********";
                                }
                            }
                            content = jbody.ToString();
                        }
                        catch
                        {
                            // ignore
                        }
                    }

                    if (path.Contains("SetAdminUserPassword", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var jbody = JObject.Parse(content);
                            var password = jbody["after"]["password"].ToString();
                            if (!string.IsNullOrEmpty(password))
                            {
                                jbody["after"]["password"] = "********";
                            }
                            content = jbody.ToString();
                        }
                        catch
                        {
                            // ignore
                        }
                    }

                    var log = new StringBuilder();
                    log.AppendLine($"REQUEST URL: {url}, Method: {context.Request.Method}, CodePage: {codePage},");
                    log.AppendLine($"REQUEST BODY: {content}");
                    _logger.LogInformation(log.ToString());
                }
            }

            requestBodyStream.Seek(0, SeekOrigin.Begin);
            context.Request.Body = requestBodyStream;

            await _next(context);
            context.Request.Body = originalRequestBody;
        }

        #endregion
    }
}
