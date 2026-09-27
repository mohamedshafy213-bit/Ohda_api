using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Contracts.Responses;
using LoggerService;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

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
                _logger.LogError($"[GLOBAL_EXCEPTION_CAUGHT] Path: {httpContext.Request.Path} - Error: {ex}");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = HttpStatusCode.BadRequest;
            string userFriendlyMessage;

            if (exception is DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                if (innerMessage.Contains("IX_Users_Username") || innerMessage.Contains("Username"))
                {
                    userFriendlyMessage = "اسم المستخدم موجود مسبقاً، يرجى اختيار اسم مستخدم آخر.";
                }
                else if (innerMessage.Contains("IX_Users_Email") || innerMessage.Contains("Email"))
                {
                    userFriendlyMessage = "البريد الإلكتروني مسجل مسبقاً لمستخدم آخر.";
                }
                else if (innerMessage.Contains("PK_Users") || innerMessage.Contains("MilitaryNumber") || innerMessage.Contains("PRIMARY KEY"))
                {
                    userFriendlyMessage = "الرقم العسكري مسجل مسبقاً بالنظام.";
                }
                else if (innerMessage.Contains("FK_") || innerMessage.Contains("FOREIGN KEY"))
                {
                    userFriendlyMessage = "خطأ في ارتباط البيانات: القيمة المحددة (مثل الفرع أو مجموعة الصلاحيات) غير صحيحة أو تم حذفها.";
                }
                else
                {
                    userFriendlyMessage = $"تعذر حفظ التغييرات بقاعدة البيانات: {dbEx.InnerException?.Message ?? dbEx.Message}";
                }
            }
            else if (exception is UnauthorizedAccessException)
            {
                statusCode = HttpStatusCode.Unauthorized;
                userFriendlyMessage = "غير مصرح لك بالقيام بهذا الإجراء.";
            }
            else if (exception is InvalidOperationException invalidOpEx)
            {
                userFriendlyMessage = invalidOpEx.Message;
            }
            else if (exception is ArgumentException argEx)
            {
                userFriendlyMessage = argEx.Message;
            }
            else
            {
                statusCode = HttpStatusCode.InternalServerError;
                userFriendlyMessage = "حدث خطأ غير متوقع أثناء معالجة الطلب. تم تسجيل تفاصيل الخطأ بنجاح.";
            }

            context.Response.StatusCode = (int)statusCode;

            var response = new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = userFriendlyMessage
            };

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }
}
