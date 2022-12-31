namespace ElasticsearchExample.Options
{
    public class DemoSeedingOptions
    {
        public const string Section = nameof(DemoSeedingOptions);

        public int CustomersCount { get; set; }
        public int ProductsCount { get; set; }

        public int OrdersCount { get; set; }
        public double OrdersPeriodInYears { get; set; }
    }
}
