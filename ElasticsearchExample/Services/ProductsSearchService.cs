using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using ElasticsearchExample.Models;
using ElasticsearchExample.Models.Buckets;

namespace ElasticsearchExample.Services
{
    public class ProductsSearchService : IProductsSearchService
    {
        private const string ProductsIndexName = "products-data-index";

        private readonly ILogger<ProductsSearchService> _logger;
        private readonly ISearchClient<ElasticsearchClient> _elasticsearch;

        public ProductsSearchService(
            ILogger<ProductsSearchService> logger,
            ISearchClient<ElasticsearchClient> elasticsearch)
        {
            _logger = logger;
            _elasticsearch = elasticsearch;
        }

        public async Task CreateIndex(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{index}] -- Create index", ProductsIndexName);
            await _elasticsearch.UsingClient(async x =>
            {
                var existsResponse = await x.Indices.ExistsAsync(ProductsIndexName, cancellationToken);

                if (!existsResponse.Exists)
                {
                    var indexResponse = await x.Indices.CreateAsync(ProductsIndexName, i => i
                        .Mappings(m => m
                            .Properties<Product>(p => p
                                .Keyword(k => k.Id)
                                .Keyword(k => k.Category)
                                .Text(k => k.Name)
                                .DoubleNumber(k => k.Price)
                                .Date(k => k.CreatedAt)))
                        .Settings(s => s
                            .NumberOfShards(1)
                            .NumberOfReplicas(1)),
                            cancellationToken);

                    if (!indexResponse.IsValidResponse || indexResponse.Acknowledged is false)
                    {
                        _logger.LogError("-- Elasticsearch server error.", indexResponse.ElasticsearchServerError);
                        return;
                    }

                    _logger.LogInformation("[{index}] -- Index created.", ProductsIndexName);
                }
                else
                {
                    _logger.LogInformation("[{index}] -- Index exists.", ProductsIndexName);
                }
            });
        }

        public async Task DeleteIndex(CancellationToken cancellationToken = default)
        {
            await _elasticsearch.UsingClient(async x =>
            {
                _logger.LogInformation("[{index}] -- Delete index", ProductsIndexName);
                var response = await x.Indices.DeleteAsync(ProductsIndexName, cancellationToken);

                if (response.IsValidResponse)
                {
                    _logger.LogInformation("[{index}] -- Delete index succeeded.", ProductsIndexName);
                }
                else
                {
                    _logger.LogWarning("[{index}] -- Delete index unsucceeded.", ProductsIndexName);
                }
            });
        }

        public async Task Index(Product document, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{index}] -- Index document", ProductsIndexName);
            await _elasticsearch.UsingClient(async x =>
            {
                var response = await x.IndexAsync(document, request => request.Index(ProductsIndexName), cancellationToken);

                if (!response.IsValidResponse)
                {
                    if (response.TryGetOriginalException(out Exception? exception))
                        throw new Exception("Elasticsearch response contains errors", exception);
                    else
                        throw new Exception("Elasticsearch response contains unknown errors");
                }
            });
        }

        public async Task IndexAll(IEnumerable<Product> documents, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{index}] -- Index all documents", ProductsIndexName);
            await _elasticsearch.UsingClient(x =>
            {
                var bulkAll = x.BulkAll(documents, r => r
                    .Index(ProductsIndexName)
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
                    _logger.LogInformation("[{index}] -- \"{action}\" completed with no failed buffers",
                        ProductsIndexName,
                        nameof(IndexAll));
                }
                else
                {
                    _logger.LogWarning("[{index}] -- \"{action}\" completed with [{number}] failed buffers",
                        ProductsIndexName,
                        nameof(IndexAll),
                        observer.TotalNumberOfFailedBuffers);
                }

                return Task.CompletedTask;
            });
        }

        public async Task<IEnumerable<Product>?> SearchByName(
            string name,
            int skip = 0,
            int size = 20,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{index}] -- Search by name", ProductsIndexName);
            return await _elasticsearch.UsingClient<IEnumerable<Product>?>(async x =>
            {
                var response = await x.SearchAsync<Product>(s => s
                    .Index(ProductsIndexName)
                    .From(skip).Size(size)
                    .Query(q => q
                        .Match(f => f
                        .Field(f => f.Name)
                        .Operator(Operator.And)
                        .Fuzziness(new Fuzziness("AUTO"))
                        .Query(name)))
                    .Sort(x => x.Field(d => d.CreatedAt, f => f.Order(SortOrder.Desc))),
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

        public async Task<IEnumerable<Product>?> SearchByCategory(
            string category,
            int skip = 0,
            int size = 20,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{index}] -- Search by category", ProductsIndexName);
            return await _elasticsearch.UsingClient<IEnumerable<Product>?>(async x =>
            {
                var response = await x.SearchAsync<Product>(s => s
                    .Index(ProductsIndexName)
                    .From(skip).Size(size)
                    .Query(q => q
                        .Match(m => m
                            .Field(x => x.Category.ToString())
                            .Operator(Operator.And)
                            .Fuzziness(new Fuzziness("AUTO"))
                            .FuzzyTranspositions()
                            .Query(category)))
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

        public async Task<IEnumerable<ProductsByCategoryBucket>> AggregateByCategories(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{index}] -- Aggregate by categories", ProductsIndexName);
            return await _elasticsearch.UsingClient<IEnumerable<ProductsByCategoryBucket>>(async x =>
            {
                SearchResponse<Product> response = await x.SearchAsync<Product>(s => s
                    .Index(ProductsIndexName)
                    .From(0).Size(0)
                    .Aggregations(a => a
                        .Terms("by-category", a => a
                            .Field(f => f.Category)
                            .Aggregations(agg => agg
                                .Cardinality("products-count", count => count.Field(f => f.Id))
                                .Avg("avg-price", x => x.Field(f => f.Price))))),
                                cancellationToken);

                if (!response.IsValidResponse)
                {
                    if (response.TryGetOriginalException(out Exception? exception))
                        throw new Exception("Elasticsearch response contains errors", exception);
                    else
                        throw new Exception("Elasticsearch response contains unknown errors");
                }

                var aggregationBuckets = response.Aggregations!
                    .GetStringTerms("by-category")!.Buckets;

                List<ProductsByCategoryBucket> buckets = new();

                foreach (var aggregationBucket in aggregationBuckets)
                {
                    string key = aggregationBucket.Key!.ToString();

                    var productsCount = Convert.ToInt32(aggregationBucket!.GetCardinality("products-count")!.Value);
                    var avgPrice = Convert.ToDecimal(aggregationBucket!.GetAvg("avg-price")!.Value);

                    buckets.Add(new ProductsByCategoryBucket(key)
                    {
                        Category = key,
                        TotalProductsCount = productsCount,
                        AvgPrice = avgPrice
                    });
                }

                return buckets;
            });
        }
    }
}