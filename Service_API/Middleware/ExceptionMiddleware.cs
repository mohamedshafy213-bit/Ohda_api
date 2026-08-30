using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Contracts.Responses;
using LoggerService;
using Microsoft.AspNetCore.Http;

namespace Service_API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerManager _logger;

        public ExceptionMiddleware(RequestDelegate next, ILoggerManager logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unhandled exception occurred: {ex}");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var message = "حدث خطأ داخلي في الخادم. يرجى المحاولة مرة أخرى لاحقاً.";
            
            // Localized database error message if it's a database exception
            if (exception.ToString().Contains("Npgsql") || exception.ToString().Contains("Microsoft.EntityFrameworkCore"))
            {
                message = "خطأ في معالجة البيانات بقاعدة البيانات. يرجى التأكد من صحة الحقول والمدخلات.";
            }

            var response = new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = message
            };

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }
}
