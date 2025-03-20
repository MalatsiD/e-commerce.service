using Ecommerce.API.Errors;
using System.Net;
using System.Text.Json;

namespace Ecommerce.API.Middleware
{
    public class ExceptionMiddleware(IHostEnvironment env, RequestDelegate next, ILoggerFactory _loggerFactory)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var _logger = _loggerFactory.CreateLogger<ExceptionMiddleware>();

            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                string? stackTrace = env.IsDevelopment() ? ex.StackTrace : "Internal server error";
                _logger.LogError("Error: {@ErrorMessage}, {@StackTrace}", ex.Message, stackTrace);

                await HandleExceptionAsync(context, ex, env);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex, IHostEnvironment env)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = env.IsDevelopment() ?
                new ApiErrorResponse(context.Response.StatusCode, ex.Message, ex.StackTrace) :
                new ApiErrorResponse(context.Response.StatusCode, ex.Message, "Internal server error");

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            var json = JsonSerializer.Serialize(response, options);

            return context.Response.WriteAsync(json);
        }
    }
}
