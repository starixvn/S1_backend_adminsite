using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;
using System.Text.Json;

namespace BackendSite.ExceptionHandling
{
    public class ExceptionFilter : IExceptionFilter
    {
        private readonly SelLangBLL _selLangBLL;

        public ExceptionFilter()
        {
            _selLangBLL = new SelLangBLL();
        }

        public void OnException(ExceptionContext context)
        {
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
            }

            var httpContext = context.HttpContext;

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)code;
            httpContext.Response.WriteAsync(result);

            context.ExceptionHandled = true;

        }
    }
}
