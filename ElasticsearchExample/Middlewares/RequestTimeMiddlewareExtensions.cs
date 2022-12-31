namespace ElasticsearchExample.Middlewares
{
    public static class RequestTimeMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestTimeMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestTimeMiddleware>();
        }
    }
}
