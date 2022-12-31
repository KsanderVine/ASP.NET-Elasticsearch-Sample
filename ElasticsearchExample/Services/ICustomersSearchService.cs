using ElasticsearchExample.Models;

namespace ElasticsearchExample.Services
{
    public interface ICustomersSearchService : ISearchService<Customer>
    {
        Task<IEnumerable<Customer>?> SearchByName(
            string name,
            int skip = 0,
            int size = 20,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Customer>?> SearchBySurname(
            string name,
            int skip = 0,
            int size = 20,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Customer>?> SearchByPhoneNumber(
            string phoneNumber,
            int skip = 0,
            int size = 20,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Customer>?> SearchByHobby(
            string hobby,
            int skip = 0,
            int size = 20,
            CancellationToken cancellationToken = default);

        Task<int> CountCustomersByHobby(
            string hobby,
            CancellationToken cancellationToken = default);
    }
}