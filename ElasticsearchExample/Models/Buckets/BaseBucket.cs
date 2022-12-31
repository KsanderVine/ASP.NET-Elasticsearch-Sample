namespace ElasticsearchExample.Models.Buckets
{
    public abstract class BaseBucket<TKey>
    {
        public TKey Key { get; set; }

        protected BaseBucket(TKey key)
        {
            Key = key;
        }
    }
}