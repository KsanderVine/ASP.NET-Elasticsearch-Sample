namespace ElasticsearchExample.Middlewares
{
    public class RequestTimeMiddleware
    {
        private readonly RequestDelegate _next;
        private System.Diagnostics.Stopwatch Stopwatch { get; set; } = new System.Diagnostics.Stopwatch();

        public RequestTimeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, ILogger<RequestTimeMiddleware> logger)
        {
            Stopwatch.Restart();
            await _next(httpContext);
            Stopwatch.Stop();

            logger.LogInformation("--> Total request time: {time} ms",
                Stopwatch.ElapsedMilliseconds);
        }
    }
}
