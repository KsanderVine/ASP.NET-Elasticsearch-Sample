using ElasticsearchExample.Models;

namespace ElasticsearchExample.Data.Repos
{
    public interface ICustomersRepository : IRepository<Guid, Customer>
    {

    }
}
