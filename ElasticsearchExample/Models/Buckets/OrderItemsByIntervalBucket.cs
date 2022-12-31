namespace ElasticsearchExample.Models.Buckets
{
    public class OrderItemsByIntervalBucket : BaseBucket<long>
    {
        public DateTime DateTime { get; set; }
        public double QuantitySum { get; set; }
        public int UniqueCustomers { get; set; }
        public decimal OrdersPriceSum { get; set; }

        public OrderItemsByIntervalBucket(long key) : base(key)
        {
        }
    }
}