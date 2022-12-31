namespace ElasticsearchExample.Services
{
    public interface ISearchClient<TClient>
    {
        Task UsingClient(Func<TClient, Task> clientRequest);
        Task<TResult> UsingClient<TResult>(Func<TClient, Task<TResult>> clientRequest);
    }
}
