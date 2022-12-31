using ElasticsearchExample.Models;

namespace ElasticsearchExample.Data.Repos
{
    public interface IProductsRepository : IRepository<Guid, Product>
    {

    }
}
