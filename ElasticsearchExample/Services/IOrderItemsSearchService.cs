using Elastic.Clients.Elasticsearch.Aggregations;
using ElasticsearchExample.Models;
using ElasticsearchExample.Models.Buckets;

namespace ElasticsearchExample.Services
{
    public interface IOrderItemsSearchService : ISearchService<OrderItem>
    {
        Task<IEnumerable<OrderItemsByIntervalBucket>> AggregateByInterval(
            CalendarInterval calendarInterval,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<OrderItemsByIntervalBucket>> AggregateForProductIdByInterval(
            Guid productId,
            CalendarInterval calendarInterval,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<OrderItemsByIntervalBucket>> AggregateForProductIdAndCustomerIdByInterval(
             Guid productId,
             Guid customerId,
             CalendarInterval calendarInterval,
             CancellationToken cancellationToken = default);
    }
}