using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BackendSite.Service.Model.enums;

namespace BackendSite.ExceptionHandling
{
    public class ExceptionFilter : IExceptionFilter
    {
        private readonly SelLangBLL _selLangBLL;
        private readonly ILogger<ExceptionFilter> _logger;

        public ExceptionFilter(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<ExceptionFilter>>();
            _selLangBLL = serviceProvider.GetRequiredService<SelLangBLL>();
        }

        public void OnException(ExceptionContext context)
        {
            var contrllerActionPath = $"{context.RouteData.Values["controller"]}/{context.RouteData.Values["action"]}";

            var code = HttpStatusCode.InternalServerError;

            var exception = context.Exception;
            var result = JsonSerializer.Serialize(new
            {
                errorCode = (int)ErrorCodeEnum.SystemException,
                message = _selLangBLL.GetMsg(ErrorCodeEnum.SystemException.GetDescription()),
                rawErrorCode = exception.HResult.ToString(),
                rawMessage = exception.Message,
                stackTrace = exception.StackTrace
            });

            if (context.Exception.GetType() == typeof(AdminException))
            {
                code = HttpStatusCode.OK;

                var adminException = context.Exception as AdminException;
                var message = adminException.Message;

                if (string.IsNullOrEmpty(message))
                {
                    message = _selLangBLL.GetMsg(adminException.ErrorCode.GetDescription());
                }

                result = JsonSerializer.Serialize(new
                {
                    errorCode = adminException.ErrorCode,
                    message = message
                });
                _logger.LogError(exception, message);
            }
            else
            {
                _logger.LogError(context.Exception, contrllerActionPath);
            }

            var httpContext = context.HttpContext;

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)code;
            httpContext.Response.WriteAsync(result);

            context.ExceptionHandled = true;

        }
    }
}
