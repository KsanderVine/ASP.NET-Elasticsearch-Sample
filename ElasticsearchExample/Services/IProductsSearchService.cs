using ElasticsearchExample.Models;
using ElasticsearchExample.Models.Buckets;

namespace ElasticsearchExample.Services
{
    public interface IProductsSearchService : ISearchService<Product>
    {
        Task<IEnumerable<Product>?> SearchByName(
            string name,
            int skip = 0,
            int size = 20,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Product>?> SearchByCategory(
            string category,
            int skip = 0,
            int size = 20,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<ProductsByCategoryBucket>> AggregateByCategories(
            CancellationToken cancellationToken = default);
    }
}