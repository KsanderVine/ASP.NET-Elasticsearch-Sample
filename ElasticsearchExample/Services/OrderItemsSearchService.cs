using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using ElasticsearchExample.Models;
using ElasticsearchExample.Models.Buckets;

namespace ElasticsearchExample.Services
{
    public class OrderItemsSearchService : IOrderItemsSearchService
    {
        private const string OrderItemsIndexName = "orderitems-data-index";

        private readonly ILogger<OrderItemsSearchService> _logger;
        private readonly ISearchClient<ElasticsearchClient> _elasticsearch;

        public OrderItemsSearchService(
            ILogger<OrderItemsSearchService> logger,
            ISearchClient<ElasticsearchClient> elasticsearch)
        {
            _logger = logger;
            _elasticsearch = elasticsearch;
        }

        public async Task CreateIndex(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{index}] -- Create index", OrderItemsIndexName);
            await _elasticsearch.UsingClient(async x =>
            {
                var existsResponse = await x.Indices.ExistsAsync(OrderItemsIndexName, cancellationToken);

                if (!existsResponse.Exists)
                {
                    var indexResponse = await x.Indices.CreateAsync(OrderItemsIndexName, i => i
                        .Mappings(m => m
                            .Properties<OrderItem>(p => p
                                .Keyword(k => k.Id)
                                .Keyword(k => k.CustomerId)
                                .Keyword(k => k.ProductId)
                                .IntegerNumber(k => k.Quantity)
                                .DoubleNumber(k => k.OrderPrice)
                                .Date(k => k.CreatedAt)))
                        .Settings(s => s
                            .NumberOfShards(1)
                            .NumberOfReplicas(1)),
                            cancellationToken);

                    if (!indexResponse.IsValidResponse || indexResponse.Acknowledged is false)
                    {
                        _logger.LogError("-- Elasticsearch server error: {error}", indexResponse.ElasticsearchServerError);
                        return;
                    }

                    _logger.LogInformation("[{index}] -- Index created.", OrderItemsIndexName);
                }
                else
                {
                    _logger.LogInformation("[{index}] -- Index exists.", OrderItemsIndexName);
                }
            });
        }

        public async Task DeleteIndex(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{index}] -- Delete index", OrderItemsIndexName);
            await _elasticsearch.UsingClient(async x =>
            {
                var response = await x.Indices.DeleteAsync(OrderItemsIndexName, cancellationToken);

                if (response.IsValidResponse)
                {
                    _logger.LogInformation("[{index}] -- Delete index succeeded.", OrderItemsIndexName);
                }
                else
                {
                    _logger.LogWarning("[{index}] -- Delete index unsucceeded.", OrderItemsIndexName);
                }
            });
        }

        public async Task Index(OrderItem document, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{index}] -- Index document", OrderItemsIndexName);
            await _elasticsearch.UsingClient(async x =>
            {
                var response = await x.IndexAsync(document, request => request.Index(OrderItemsIndexName), cancellationToken);

                if (!response.IsValidResponse)
                {
                    if (response.TryGetOriginalException(out Exception? exception))
                        throw new Exception("Elasticsearch response contains errors.", exception);
                    else
                        throw new Exception("Elasticsearch response contains unknown errors");
                }
            });
        }

        public async Task IndexAll(IEnumerable<OrderItem> documents, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{index}] -- Index all documents", OrderItemsIndexName);
            await _elasticsearch.UsingClient(x =>
            {
                var bulkAll = x.BulkAll(documents, r => r
                    .Index(OrderItemsIndexName)
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
                        OrderItemsIndexName);
                }
                else
                {
                    _logger.LogWarning("[{index}] -- \"BulkAll\" completed with [{number}] failed buffers",
                        OrderItemsIndexName, observer.TotalNumberOfFailedBuffers);
                }

                return Task.CompletedTask;
            });
        }

        public async Task<IEnumerable<OrderItemsByIntervalBucket>> AggregateByInterval(
            CalendarInterval calendarInterval,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{index}] -- Aggregate by interval", OrderItemsIndexName);
            return await _elasticsearch.UsingClient<IEnumerable<OrderItemsByIntervalBucket>>(async x =>
            {
                SearchResponse<OrderItem> response = await x.SearchAsync<OrderItem>(s => s
                    .Index(OrderItemsIndexName)
                    .From(0).Size(0)
                    .Query(q => q.MatchAll())
                    .Aggregations(a => a
                        .DateHistogram("by-interval", a => a
                            .CalendarInterval(calendarInterval)
                            .Field(f => f.CreatedAt)
                            .Format("yyyy-MM-dd")
                            .Aggregations(agg => agg
                                .Cardinality("unique-customers", count => count.Field(f => f.CustomerId))
                                .Sum("quantity-sum", sum => sum.Field(fld => fld.Quantity))
                                .Sum("price-sum", sum => sum.Field(fld => (fld.OrderPrice)))))),
                                cancellationToken);

                if (!response.IsValidResponse)
                {
                    if (response.TryGetOriginalException(out Exception? exception))
                        throw new Exception("Elasticsearch response contains errors", exception);
                    else
                        throw new Exception("Elasticsearch response contains unknown errors");
                }

                var aggregationBuckets = response.Aggregations!
                    .GetDateHistogram("by-interval")!.Buckets;

                List<OrderItemsByIntervalBucket> buckets = new();

                foreach (var aggregationBucket in aggregationBuckets)
                {
                    var key = aggregationBucket.Key;
                    var quantitySum = Convert.ToDouble(aggregationBucket!.GetSum("quantity-sum")!.Value);
                    var ordersPriceSum = Convert.ToDecimal(aggregationBucket!.GetSum("price-sum")!.Value);
                    var uniqueCustomers = Convert.ToInt32(aggregationBucket!.GetCardinality("unique-customers")!.Value);

                    buckets.Add(new OrderItemsByIntervalBucket(key)
                    {
                        DateTime = DateTime.UnixEpoch.AddMilliseconds(key),
                        UniqueCustomers = uniqueCustomers,
                        QuantitySum = quantitySum,
                        OrdersPriceSum = ordersPriceSum
                    });
                }

                return buckets;
            });
        }

        public async Task<IEnumerable<OrderItemsByIntervalBucket>> AggregateForProductIdByInterval(
            Guid productId,
            CalendarInterval calendarInterval,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{index}] -- Aggregate for product id by interval", OrderItemsIndexName);
            return await _elasticsearch.UsingClient<IEnumerable<OrderItemsByIntervalBucket>>(async x =>
            {
                SearchResponse<OrderItem> response = await x.SearchAsync<OrderItem>(s => s
                    .Index(OrderItemsIndexName)
                    .From(0).Size(0)
                    .Query(q => q.Match(m => m.Field(f => f.ProductId).Query(productId.ToString())))
                    .Aggregations(a => a
                        .DateHistogram("by-interval", a => a
                            .CalendarInterval(calendarInterval)
                            .Field(f => f.CreatedAt)
                            .Format("yyyy-MM-dd")
                            .Aggregations(agg => agg
                                .Cardinality("unique-customers", count => count.Field(f => f.CustomerId))
                                .Sum("quantity-sum", sum => sum.Field(fld => fld.Quantity))
                                .Sum("price-sum", sum => sum.Field(fld => (fld.OrderPrice)))))),
                                cancellationToken);

                if (!response.IsValidResponse)
                {
                    if (response.TryGetOriginalException(out Exception? exception))
                        throw new Exception("Elasticsearch response contains errors", exception);
                    else
                        throw new Exception("Elasticsearch response contains unknown errors");
                }

                var aggregationBuckets = response.Aggregations!
                    .GetDateHistogram("by-interval")!.Buckets;

                List<OrderItemsByIntervalBucket> buckets = new();

                foreach (var aggregationBucket in aggregationBuckets)
                {
                    var key = aggregationBucket.Key;
                    var quantitySum = Convert.ToDouble(aggregationBucket!.GetSum("quantity-sum")!.Value);
                    var ordersPriceSum = Convert.ToDecimal(aggregationBucket!.GetSum("price-sum")!.Value);
                    var uniqueCustomers = Convert.ToInt32(aggregationBucket!.GetCardinality("unique-customers")!.Value);

                    buckets.Add(new OrderItemsByIntervalBucket(key)
                    {
                        DateTime = DateTime.UnixEpoch.AddMilliseconds(key),
                        UniqueCustomers = uniqueCustomers,
                        QuantitySum = quantitySum,
                        OrdersPriceSum = ordersPriceSum
                    });
                }

                return buckets;
            });
        }

        public async Task<IEnumerable<OrderItemsByIntervalBucket>> AggregateForProductIdAndCustomerIdByInterval(
            Guid productId,
            Guid customerId,
            CalendarInterval calendarInterval,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{index}] -- Aggregate for product id and customer id by interval", OrderItemsIndexName);
            return await _elasticsearch.UsingClient<IEnumerable<OrderItemsByIntervalBucket>>(async x =>
            {
                SearchResponse<OrderItem> response = await x.SearchAsync<OrderItem>(s => s
                    .Index(OrderItemsIndexName)
                    .From(0).Size(0)
                    .Query(q => q
                        .Bool(b => b.Must(m => m
                            .Match(ma => ma.Field(f => f.ProductId).Query(productId.ToString()))
                            .Match(ma => ma.Field(f => f.CustomerId).Query(customerId.ToString())))))
                    .Aggregations(a => a
                        .DateHistogram("by-interval", a => a
                            .CalendarInterval(calendarInterval)
                            .Field(f => f.CreatedAt)
                            .Format("yyyy-MM-dd")
                            .Aggregations(agg => agg
                                .Cardinality("unique-customers", count => count.Field(f => f.CustomerId))
                                .Sum("quantity-sum", sum => sum.Field(fld => fld.Quantity))
                                .Sum("price-sum", sum => sum.Field(fld => (fld.OrderPrice)))))),
                                cancellationToken);

                if (!response.IsValidResponse)
                {
                    if (response.TryGetOriginalException(out Exception? exception))
                        throw new Exception("Elasticsearch response contains errors", exception);
                    else
                        throw new Exception("Elasticsearch response contains unknown errors");
                }

                var aggregationBuckets = response.Aggregations!
                    .GetDateHistogram("by-interval")!.Buckets;

                List<OrderItemsByIntervalBucket> buckets = new();

                foreach (var aggregationBucket in aggregationBuckets)
                {
                    var key = aggregationBucket.Key;
                    var quantitySum = Convert.ToDouble(aggregationBucket!.GetSum("quantity-sum")!.Value);
                    var ordersPriceSum = Convert.ToDecimal(aggregationBucket!.GetSum("price-sum")!.Value);
                    var uniqueCustomers = Convert.ToInt32(aggregationBucket!.GetCardinality("unique-customers")!.Value);

                    buckets.Add(new OrderItemsByIntervalBucket(key)
                    {
                        DateTime = DateTime.UnixEpoch.AddMilliseconds(key),
                        UniqueCustomers = uniqueCustomers,
                        QuantitySum = quantitySum,
                        OrdersPriceSum = ordersPriceSum
                    });
                }

                return buckets;
            });
        }
    }
}