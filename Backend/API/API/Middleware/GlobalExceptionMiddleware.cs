using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using API.DTOs;
using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IMongoLogService logService)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception intercepted by GlobalExceptionMiddleware");

                // Log exception to MongoDB
                try
                {
                    await logService.LogExceptionAsync(ex.Message, ex.StackTrace, context.Request.Path);
                }
                catch
                {
                    // Prevent failure in logging from overriding response
                }

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = ApiResponse<string>.Error($"Error interno del servidor: {ex.Message}", 500);
                string json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
            }
        }
    }
}
