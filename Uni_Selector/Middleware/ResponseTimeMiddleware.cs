using System.Diagnostics;
using Uni_Selector.Helpers;

namespace Uni_Selector.Middleware
{
    public class ResponseTimeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ResponseTimeMiddleware> _logger;

        public ResponseTimeMiddleware(RequestDelegate next, ILogger<ResponseTimeMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Start timing
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                // Stop timing
                stopwatch.Stop();
                var responseTime = stopwatch.Elapsed.TotalMilliseconds;

                // Record the response time
                ResponseTimeTracker.RecordResponseTime(
                    path: context.Request.Path,
                    method: context.Request.Method,
                    statusCode: context.Response.StatusCode,
                    responseTimeMs: responseTime
                );

                // Add response time header (optional, for debugging)
                if (!context.Response.HasStarted)
                {
                    context.Response.Headers["X-Response-Time-ms"] = responseTime.ToString("F2");
                }

                // Log slow requests (over 1 second)
                if (responseTime > 1000)
                {
                    _logger.LogWarning(
                        "Slow request detected: {Method} {Path} took {ResponseTime}ms (Status: {StatusCode})",
                        context.Request.Method,
                        context.Request.Path,
                        responseTime.ToString("F2"),
                        context.Response.StatusCode
                    );
                }
            }
        }
    }
}
