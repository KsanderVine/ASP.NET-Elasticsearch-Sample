namespace ElasticsearchExample.Models.Buckets
{
    public class ProductsByCategoryBucket : BaseBucket<string>
    {
        public string Category { get; set; } = string.Empty;

        public int TotalProductsCount { get; set; }
        public decimal AvgPrice { get; set; }

        public ProductsByCategoryBucket(string key) : base(key)
        {
        }
    }
}