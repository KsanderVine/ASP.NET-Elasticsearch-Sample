using Elastic.Clients.Elasticsearch;
using ElasticsearchExample.Data;
using ElasticsearchExample.Data.Repos;
using ElasticsearchExample.Demo;
using ElasticsearchExample.Middlewares;
using ElasticsearchExample.Services;
using Microsoft.EntityFrameworkCore;

namespace ElasticsearchExample
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("InMemory"));
            }
            else
            {
                string? sqlServerConnection = builder.Configuration.GetConnectionString("SqlServer");
                builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(sqlServerConnection));
            }

            builder.Services.AddScoped<ICustomersRepository, CustomersRepository>();
            builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
            builder.Services.AddScoped<IOrderItemsRepository, OrderItemsRepository>();

            builder.Services.AddSingleton<ISearchClient<ElasticsearchClient>, ElasticClient>();
            builder.Services.AddSingleton<ICustomersSearchService, CustomersSearchService>();
            builder.Services.AddSingleton<IProductsSearchService, ProductsSearchService>();
            builder.Services.AddSingleton<IOrderItemsSearchService, OrderItemsSearchService>();

            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            string? isDemo = Environment.GetEnvironmentVariable("IS_DEMO");
            if (builder.Environment.IsDevelopment() || string.Equals(isDemo?.ToUpper(), "TRUE"))
            {
                builder.Services.AddSingleton<IDemoDataSeedingProvider, DemoSeedingProvider>();
            }

            var app = builder.Build();

            app.UseRequestTimeMiddleware();

            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseHttpsRedirection();

            app.MapControllers();

            await AppDbContextSeed.Seed(app, app.Environment);

            app.Run();
        }
    }
}