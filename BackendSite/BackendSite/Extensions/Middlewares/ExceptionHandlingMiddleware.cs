using System.Net;
using System.Text.Json;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Model.enums;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly SelLangBLL _selLangBLL;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, SelLangBLL selLangBLL)
    {
        _next = next;
        _logger = logger;
        _selLangBLL = selLangBLL;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // 執行後續 Middleware
            await _next(context);
        }
        catch (Exception ex)
        {
            // 捕捉例外並記錄日誌
            _logger.LogError(ex, "Unhandled Exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError; // 預設錯誤碼 500

        // 根據例外類型定義不同錯誤碼
        if (exception is UnauthorizedAccessException)
            code = HttpStatusCode.Unauthorized; // 401
        else if (exception is InvalidOperationException)
            code = HttpStatusCode.BadRequest; // 400

        var result = JsonSerializer.Serialize(new
        {
            errorCode = (int)ErrorCodeEnum.SystemException,
            message = _selLangBLL.GetMsg(ErrorCodeEnum.SystemException.GetDescription()),
            rawErrorCode = exception.HResult.ToString(),
            rawMessage = exception.Message,
            stackTrace = exception.StackTrace
        });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        return context.Response.WriteAsync(result);
    }
}