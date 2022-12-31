namespace ElasticsearchExample.Services
{
    public interface ISearchService
    {
        Task CreateIndex(CancellationToken cancellationToken = default);
        Task DeleteIndex(CancellationToken cancellationToken = default);
    }

    public interface ISearchService<TDocumentType> : ISearchService
        where TDocumentType : class
    {
        Task Index(TDocumentType document, CancellationToken cancellationToken = default);
        Task IndexAll(IEnumerable<TDocumentType> documents, CancellationToken cancellationToken = default);
    }
}