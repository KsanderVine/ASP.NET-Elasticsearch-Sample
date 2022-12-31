namespace ElasticsearchExample.Demo
{
    public interface IDemoDataSeedingProvider
    {
        Task SeedAsync(IServiceScope scope);
    }
}
