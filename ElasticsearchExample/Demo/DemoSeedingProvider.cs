using AutoMapper;
using ElasticsearchExample.Data;
using ElasticsearchExample.Extensions;
using ElasticsearchExample.Models;
using ElasticsearchExample.Options;
using ElasticsearchExample.Services;

namespace ElasticsearchExample.Demo
{
    public class DemoSeedingProvider : GeneratorBase, IDemoDataSeedingProvider
    {
        private readonly ILogger<DemoSeedingProvider> _logger;
        private readonly IMapper _mapper;
        private readonly DemoSeedingOptions _options;

        private readonly NameGenerator _nameGenerator;
        private readonly ProductNameGenerator _productNameGenerator;
        private readonly PhoneNumberGenerator _phoneNumberGenerator;
        private readonly HobbiesListGenerator _hobbiesListGenerator;

        private readonly ICustomersSearchService _customersSearchDocument;
        private readonly IProductsSearchService _productsSearchDocument;
        private readonly IOrderItemsSearchService _orderItemsSearchDocument;

        private static readonly Random Random = new(Seed);

        public DemoSeedingProvider(
            ILogger<DemoSeedingProvider> logger,
            IConfiguration configuration,
            IMapper mapper,
            ICustomersSearchService customersSearchIndex,
            IProductsSearchService productsSearchDocument,
            IOrderItemsSearchService orderItemsSearchDocument)
        {
            _logger = logger;
            _mapper = mapper;
            _options = configuration
                .GetSection(DemoSeedingOptions.Section)
                .Get<DemoSeedingOptions>();

            _nameGenerator = new NameGenerator();
            _productNameGenerator = new ProductNameGenerator();
            _phoneNumberGenerator = new PhoneNumberGenerator();
            _hobbiesListGenerator = new HobbiesListGenerator();

            _customersSearchDocument = customersSearchIndex;
            _productsSearchDocument = productsSearchDocument;
            _orderItemsSearchDocument = orderItemsSearchDocument;
        }

        public async Task SeedAsync(IServiceScope scope)
        {
            using var context = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            if (context.Customers.Any())
            {
                _logger.LogInformation("Seeding already done...");
                return;
            }

            _logger.LogInformation("Seeding development data...");

            List<Customer> customers = await SeedAndCreateIndicesForCustomers(context);
            List<Product> products = await SeedAndCreateIndicesForProducts(context);
            List<OrderItem> orders = await SeedAndCreateIndicesForOrderItems(context, customers, products);

            _logger.LogInformation("--> Total seeded customers count -- {Count}", customers.Count);
            _logger.LogInformation("--> Total seeded products count  -- {Count}", products.Count);
            _logger.LogInformation("--> Total seeded orders count    -- {Count}", orders.Count);

            _logger.LogInformation("Data seeded...");
        }

        private async Task<List<Customer>> SeedAndCreateIndicesForCustomers(AppDbContext context)
        {
            var customers = SeedCustomers();
            await context.Customers.AddRangeAsync(customers);
            await context.SaveChangesAsync();

            await _customersSearchDocument.DeleteIndex();
            await _customersSearchDocument.CreateIndex();
            await _customersSearchDocument.IndexAll(customers);
            return customers;
        }

        private async Task<List<Product>> SeedAndCreateIndicesForProducts(AppDbContext context)
        {
            var products = SeedProducts();
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            await _productsSearchDocument.DeleteIndex();
            await _productsSearchDocument.CreateIndex();
            await _productsSearchDocument.IndexAll(products);
            return products;
        }

        private async Task<List<OrderItem>> SeedAndCreateIndicesForOrderItems(AppDbContext context, List<Customer> customers, List<Product> products)
        {
            var orders = SeedOrders(customers, products);
            await context.Orders.AddRangeAsync(orders);
            await context.SaveChangesAsync();

            await _orderItemsSearchDocument.DeleteIndex();
            await _orderItemsSearchDocument.CreateIndex();
            await _orderItemsSearchDocument.IndexAll(orders);
            return orders;
        }

        private List<Customer> SeedCustomers()
        {
            _logger.LogInformation("Seeding customers data...");
            var customers = new List<Customer>();

            var customerNames = _nameGenerator.GetManyNames(_options.CustomersCount);
            var phoneNumbers = _phoneNumberGenerator.GetMany(_options.CustomersCount);
            var hobbiesLists = _hobbiesListGenerator.GetMany(_options.CustomersCount);

            for (int i = 0; i < _options.CustomersCount; i++)
            {
                string customerName = customerNames[i].Givenname;
                string customerSurname = customerNames[i].Surname;

                string phoneNumber = phoneNumbers[i];
                string hobbiesList = hobbiesLists[i];

                customers.Add(new Customer()
                {
                    Name = customerName,
                    Surname = customerSurname,
                    PhoneNumber = phoneNumber,
                    Hobbies = hobbiesList,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return customers;
        }

        private List<Product> SeedProducts()
        {
            _logger.LogInformation("Seeding products data...");
            var products = new List<Product>();

            var categories = (Product.CategoryType[])Enum.GetValues(typeof(Product.CategoryType));

            if (categories != null)
            {
                foreach (var categoryType in categories)
                {
                    var productNames = _productNameGenerator
                        .GetAllProductNameForCategory(categoryType, _options.ProductsCount / categories.Length);

                    foreach (var productName in productNames)
                    {
                        products.Add(new Product()
                        {
                            Name = productName,
                            Category = categoryType,
                            Price = Convert.ToDecimal(Random.NextSingle() * 100),
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            return products;
        }

        private List<OrderItem> SeedOrders(
            List<Customer> customers,
            List<Product> products)
        {
            _logger.LogInformation("Seeding orders data...");
            var orders = new List<OrderItem>();

            var timePeriodTicks = (long)(DateTime.MinValue.AddYears(1).Ticks * _options.OrdersPeriodInYears);
            var periodEnd = DateTime.UtcNow;
            var periodStart = periodEnd.AddTicks(-timePeriodTicks);

            Console.WriteLine($"--> Orders seeding period start -- {periodStart}");
            Console.WriteLine($"--> Orders seeding period end   -- {periodEnd}");

            for (int i = 0; i < _options.OrdersCount; i++)
            {
                Customer customer = GetRandomItem(customers);
                Product product = GetRandomItem(products);
                var quantity = 1 + Random.Next(10);

                orders.Add(new OrderItem()
                {
                    CustomerId = customer.Id,
                    ProductId = product.Id,
                    Quantity = quantity,
                    OrderPrice = product.Price * quantity,
                    CreatedAt = GetRandomDateWithinPeriod(periodStart, periodEnd)
                });
            }

            return orders;

            static DateTime GetRandomDateWithinPeriod(DateTime periodStart, DateTime periodEnd)
            {
                if (periodStart == periodEnd)
                    return periodEnd;

                return DateTime.MinValue.AddTicks(Random.NextLong(periodStart.Ticks, periodEnd.Ticks));
            }

            static T GetRandomItem<T>(List<T> list)
            {
                if (list == null || !list.Any())
                    throw new ArgumentException($"Argument {nameof(list)} is null or empty");

                return list[Random.Next(list.Count)];
            }
        }
    }
}
