using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
                ErrorCode = (int)ErrorCodeEnum.SystemException,
                Message = _selLangBLL.GetMsg(ErrorCodeEnum.SystemException.GetDescription()),
                RawErrorCode = exception.HResult.ToString(),
                RawMessage = exception.Message,
                StackTrace = exception.StackTrace
            });

            if (context.Exception.GetType() == typeof(AdminException))
            {
                code = HttpStatusCode.OK;

                AdminException adminException = context.Exception as AdminException;
                string message = adminException.Message;

                if (String.IsNullOrEmpty(message))
                {
                    message = _selLangBLL.GetMsg(ErrorCodeEnum.SystemException.GetDescription());
                }

                result = JsonSerializer.Serialize(new
                {
                    ErrorCode = adminException.ErrorCode,
                    Message = message
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
