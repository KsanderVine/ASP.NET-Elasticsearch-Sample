using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using ElasticsearchExample.Options;
using System.Diagnostics;

namespace ElasticsearchExample.Services
{
    public class ElasticClient : ISearchClient<ElasticsearchClient>
    {
        private readonly ILogger<ElasticClient> _logger;
        private readonly ElasticsearchClient _client;
        private readonly ElasticsearchOptions _options;

        private Stopwatch Stopwatch { get; set; } = new Stopwatch();

        public ElasticClient(
            ILogger<ElasticClient> logger,
            IConfiguration configuration)
        {
            _logger = logger;

            _options = configuration
                .GetSection(ElasticsearchOptions.Section)
                .Get<ElasticsearchOptions>();

            var authentication = new BasicAuthentication(
                _options.ConnectionUsername,
                _options.ConnectionPassword);

            var nodes = _options.Nodes.Select(x => new Uri(x));
            var pool = new StaticNodePool(nodes);

            ElasticsearchClientSettings? clientSettings = new ElasticsearchClientSettings(pool)
                .Authentication(authentication)
                .EnableDebugMode()
                .PrettyJson()
                .RequestTimeout(TimeSpan.FromMinutes(2));

            _client = new ElasticsearchClient(clientSettings);
        }

        public async Task UsingClient(Func<ElasticsearchClient, Task> clientRequest)
        {
            Stopwatch.Restart();
            await clientRequest(_client);
            Stopwatch.Stop();

            _logger.LogInformation("--> Elasticsearch request. Stopwatch time: {time} ms",
                Stopwatch.ElapsedMilliseconds);
        }

        public async Task<TResult> UsingClient<TResult>(Func<ElasticsearchClient, Task<TResult>> clientRequest)
        {
            Stopwatch.Restart();
            var result = await clientRequest(_client);
            Stopwatch.Stop();

            _logger.LogInformation("--> Elasticsearch request. Stopwatch time: {time} ms",
                Stopwatch.ElapsedMilliseconds);

            return result;
        }
    }
}
