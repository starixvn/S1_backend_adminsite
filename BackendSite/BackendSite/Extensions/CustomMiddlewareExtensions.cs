using BackendSite.Extensions.Middlewares;

namespace BackendSite.Extensions
{
    public static class CustomMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogMiddleware(this IApplicationBuilder builder)
          => builder.UseMiddleware<RequestLogMiddleware>();

        public static IApplicationBuilder UseResponseLogMiddleware(this IApplicationBuilder builder)
            => builder.UseMiddleware<ResponseLogMiddleware>();
        
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
            => builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
