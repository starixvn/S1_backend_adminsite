using BackendSite.Extensions.Middlewares;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Extensions
{
    public static class CustomMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogMiddleware(this IApplicationBuilder builder)
          => builder.UseMiddleware<RequestLogMiddleware>();

        public static IApplicationBuilder UseResponseLogMiddleware(this IApplicationBuilder builder)
            => builder.UseMiddleware<ResponseLogMiddleware>();
    }
}
