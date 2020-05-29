using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace gamemaster
{
    public class DebugLoggingMiddleware
    {
        private readonly ILogger<DebugLoggingMiddleware> _logger;
        private readonly RequestDelegate _next;

        public DebugLoggingMiddleware(RequestDelegate next, ILogger<DebugLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try

            {
                var request = context.Request;
                request.EnableBuffering();
                var buffer = new byte[Convert.ToInt32(request.ContentLength)];
                await request.Body.ReadAsync(buffer, 0, buffer.Length);
                var bodyAsText = Encoding.UTF8.GetString(buffer);
                context.Items["request_body"] = bodyAsText;
                _logger.LogInformation("{Body}", bodyAsText);
                request.Body.Seek(0, SeekOrigin.Begin);
                await _next(context);
            }
            finally
            {
                _logger.LogInformation(
                    "Request {method} {url} => {statusCode}",
                    context.Request?.Method,
                    context.Request?.Path.Value,
                    context.Response?.StatusCode);
            }
        }
    }
}