namespace ElasticsearchExample.Models.QueryParameters
{
    public class QueryParameters
    {
        protected virtual int MaxSize { get; } = 30;

        private int _page = 1;
        public int PageNumber
        {
            get => _page;
            set => _page = Math.Max(1, value);
        }

        private int _size = 10;
        public int QuerySize
        {
            get => _size;
            set => _size = Math.Min(MaxSize, value);
        }
    }
}
