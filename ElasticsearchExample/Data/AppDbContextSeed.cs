using ElasticsearchExample.Demo;
using Microsoft.EntityFrameworkCore;

namespace ElasticsearchExample.Data
{
    public static class AppDbContextSeed
    {
        public static async Task Seed(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using var scope = app.ApplicationServices.CreateScope();
            await SeedDataAsync(scope, env);
        }

        private static async Task SeedDataAsync(IServiceScope scope, IWebHostEnvironment env)
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("AppDbContextSeed");

            if (env.IsProduction())
            {
                logger.LogInformation("Attempting to apply migrations...");
                try
                {
                    await context.Database.MigrateAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Could not apply migrations");
                }
            }

            var demoSeedingProvider = scope.ServiceProvider.GetRequiredService<IDemoDataSeedingProvider>();
            if (demoSeedingProvider is not null)
            {
                await demoSeedingProvider.SeedAsync(scope);
            }
        }
    }
}
