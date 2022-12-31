using ElasticsearchExample.Models;

namespace ElasticsearchExample.Data.Repos
{
    public interface IOrderItemsRepository : IRepository<Guid, OrderItem>
    {

    }
}
