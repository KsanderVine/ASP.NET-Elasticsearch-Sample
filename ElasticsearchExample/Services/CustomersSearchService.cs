using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using ElasticsearchExample.Models;

namespace ElasticsearchExample.Services
{
    public sealed class CustomersSearchService : ICustomersSearchService
    {
        private const string CustomersIndexName = "customers-data-index";

        private readonly ILogger<CustomersSearchService> _logger;
        private readonly ISearchClient<ElasticsearchClient> _elasticsearch;

        public CustomersSearchService(
            ILogger<CustomersSearchService> logger,
            ISearchClient<ElasticsearchClient> elasticsearch)
        {
            _logger = logger;
            _elasticsearch = elasticsearch;
        }

        public async Task CreateIndex(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{index}] -- Create index", CustomersIndexName);
            await _elasticsearch.UsingClient(async x =>
            {
                var existsResponse = await x.Indices.ExistsAsync(CustomersIndexName);

                if (!existsResponse.Exists)
                {
                    var indexResponse = await x.Indices.CreateAsync(CustomersIndexName, i => i
                        .Mappings(m => m
                            .Properties<Customer>(p => p
                                .Keyword(k => k.Id)
                                .Keyword(k => k.Name)
                                .Keyword(k => k.Surname)
                                .Keyword(k => k.PhoneNumber)
                                .Text(k => k.Hobbies)
                                .Date(n => n.CreatedAt)))
                        .Settings(s => s
                            .NumberOfShards(1)
                            .NumberOfReplicas(1)),
                            cancellationToken);

                    if (!indexResponse.IsValidResponse || indexResponse.Acknowledged is false)
                    {
                        _logger.LogError("-- Elasticsearch server error.", indexResponse.ElasticsearchServerError);
                        return;
                    }

                    _logger.LogInformation("[{index}] -- Index created.", CustomersIndexName);
                }
                else
                {
                    _logger.LogInformation("[{index}] -- Index exists.", CustomersIndexName);
                }
            });
        }

        public async Task DeleteIndex(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{index}] -- Delete index", CustomersIndexName);
            await _elasticsearch.UsingClient(async x =>
            {
                var response = await x.Indices.DeleteAsync(CustomersIndexName, cancellationToken);

                if (response.IsValidResponse)
                {
                    _logger.LogInformation("[{index}] -- Delete index succeeded.", CustomersIndexName);
                }
                else
                {
                    _logger.LogWarning("[{index}] -- Delete index unsucceeded.", CustomersIndexName);
                }
            });
        }

        public async Task Index(Customer document, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{index}] -- Index document", CustomersIndexName);
            await _elasticsearch.UsingClient(async x =>
            {
                var response = await x.IndexAsync(document, request => request.Index(CustomersIndexName), cancellationToken);

                if (!response.IsValidResponse)
                {
                    if (response.TryGetOriginalException(out Exception? exception))
                        throw new Exception("Elasticsearch response contains errors.", exception);
                    else
                        throw new Exception("Elasticsearch response contains unknown errors");
                }
            });
        }

        public async Task IndexAll(IEnumerable<Customer> documents, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{index}] -- Index all documents", CustomersIndexName);
            await _elasticsearch.UsingClient(x =>
            {
                var bulkAll = x.BulkAll(documents, r => r
                    .Index(CustomersIndexName)
                    .BackOffRetries(2)
                    .BackOffTime("30s")
                    .ContinueAfterDroppedDocuments()
                    .DroppedDocumentCallback((r, d) =>
                    {
                        _logger.LogError("-- Elasticsearch error while BulkAll.", r.Error?.Reason);
                    })
                    .MaxDegreeOfParallelism(4)
                    .Size(1000),
                    cancellationToken);

                var observer = bulkAll.Wait(TimeSpan.FromMinutes(10), r => { });

                if (observer.TotalNumberOfFailedBuffers == 0)
                {
                    _logger.LogInformation("[{index}] -- \"BulkAll\" completed with no failed buffers",
                        CustomersIndexName);
                }
                else
                {
                    _logger.LogWarning("[{index}] -- \"BulkAll\" completed with [{number}] failed buffers",
                        CustomersIndexName, observer.TotalNumberOfFailedBuffers);
                }

                return Task.CompletedTask;
            });
        }

        public async Task<IEnumerable<Customer>?> SearchByName(
            string name,
            int skip = 0,
            int size = 20,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{index}] -- Search by name", CustomersIndexName);
            return await _elasticsearch.UsingClient<IEnumerable<Customer>?>(async x =>
            {
                var response = await x.SearchAsync<Customer>(s => s
                    .Index(CustomersIndexName)
                    .From(skip).Size(size)
                    .Query(q => q
                        .Prefix(f => f
                            .Field(f => f.Name)
                            .Value(name)))
                    .Sort(x => x.Field(s => s.Name, f => f.Order(SortOrder.Asc))),
                    cancellationToken);

                if (!response.IsValidResponse)
                {
                    if (response.TryGetOriginalException(out Exception? exception))
                        throw new Exception("Elasticsearch response contains errors.", exception);
                    else
                        throw new Exception("Elasticsearch response contains unknown errors");
                }

                return response.Documents;
            });
        }

        public async Task<IEnumerable<Customer>?> SearchBySurname(
            string name,
            int skip = 0,
            int size = 20,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{index}] -- Search by surname", CustomersIndexName);
            return await _elasticsearch.UsingClient<IEnumerable<Customer>?>(async x =>
            {
                var response = await x.SearchAsync<Customer>(s => s
                    .Index(CustomersIndexName)
                    .From(skip).Size(size)
                    .Query(q => q
                        .Prefix(f => f
                            .Field(f => f.Surname)
                            .Value(name)))
                    .Sort(x => x.Field(s => s.Surname, f => f.Order(SortOrder.Asc))),
                    cancellationToken);

                if (!response.IsValidResponse)
                {
                    if (response.TryGetOriginalException(out Exception? exception))
                        throw new Exception("Elasticsearch response contains errors", exception);
                    else
                        throw new Exception("Elasticsearch response contains unknown errors");
                }

                return response.Documents;
            });
        }

        public async Task<IEnumerable<Customer>?> SearchByPhoneNumber(
            string phoneNumber,
            int skip = 0,
            int size = 20,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{index}] -- Search by phone number", CustomersIndexName);
            return await _elasticsearch.UsingClient<IEnumerable<Customer>?>(async x =>
            {
                var response = await x.SearchAsync<Customer>(s => s
                    .Index(CustomersIndexName)
                    .From(skip).Size(size)
                    .Query(q => q
                        .Fuzzy(f => f
                            .Field(f => f.PhoneNumber)
                            .Fuzziness(new Fuzziness("AUTO"))
                            .Value(phoneNumber)))
                    .Sort(x => x.Field(s => s.PhoneNumber)),
                    cancellationToken);

                if (!response.IsValidResponse)
                {
                    if (response.TryGetOriginalException(out Exception? exception))
                        throw new Exception("Elasticsearch response contains errors", exception);
                    else
                        throw new Exception("Elasticsearch response contains unknown errors");
                }

                return response.Documents;
            });
        }

        public async Task<IEnumerable<Customer>?> SearchByHobby(
            string hobby,
            int skip = 0,
            int size = 20,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{index}] -- Search by hobby", CustomersIndexName);
            return await _elasticsearch.UsingClient<IEnumerable<Customer>?>(async x =>
            {
                var response = await x.SearchAsync<Customer>(s => s
                    .Index(CustomersIndexName)
                    .From(skip).Size(size)
                    .Query(q => q
                        .Match(m => m
                            .Field(x => x.Hobbies)
                            .Query(hobby)
                            .Operator(Operator.And)))
                    .Sort(x => x.Field(s => s.CreatedAt)),
                    cancellationToken);

                if (!response.IsValidResponse)
                {
                    if (response.TryGetOriginalException(out Exception? exception))
                        throw new Exception("Elasticsearch response contains errors", exception);
                    else
                        throw new Exception("Elasticsearch response contains unknown errors");
                }

                return response.Documents;
            });
        }

        public async Task<int> CountCustomersByHobby(
            string hobby,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{index}] -- Count customers by hobby", CustomersIndexName);
            return await _elasticsearch.UsingClient<int>(async x =>
            {
                var response = await x.SearchAsync<Customer>(s => s
                    .Index(CustomersIndexName)
                    .From(0)
                    .Query(q => q
                        .Match(m => m
                            .Field(x => x.Hobbies)
                            .Query(hobby)
                            .Operator(Operator.And)))
                    .Aggregations(a => a.ValueCount("count", s => s.Field(f => f.Id))));

                if (!response.IsValidResponse)
                {
                    if (response.TryGetOriginalException(out Exception? exception))
                        throw new Exception("Elasticsearch response contains errors", exception);
                    else
                        throw new Exception("Elasticsearch response contains unknown errors");
                }

                return Convert.ToInt32(response.Aggregations!.GetValueCount("count")!.Value);
            });
        }
    }
}