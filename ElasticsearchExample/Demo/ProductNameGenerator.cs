using ElasticsearchExample.Extensions;
using System.Text;
using static ElasticsearchExample.Models.Product;

namespace ElasticsearchExample.Demo
{
    public class ProductNameGenerator : GeneratorBase
    {
        private static readonly Random Random = new(Seed);

        private List<string> Food { get; set; }
        private List<string> Electronics { get; set; }
        private List<string> Clothing { get; set; }

        public ProductNameGenerator()
        {
            string currentDirectory = Directory.GetCurrentDirectory();

            string companiesPath = Path.Combine(currentDirectory, "Resources", "Companies.list");
            var companies = ReadAllLines(companiesPath);

            string adjectivesPath = Path.Combine(currentDirectory, "Resources", "Adjectives.list");
            var adjectives = ReadAllLines(adjectivesPath);

            string foodPath = Path.Combine(currentDirectory, "Resources", "FoodTypes.list");
            var foodTypes = ReadAllLines(foodPath);

            string electronicsPath = Path.Combine(currentDirectory, "Resources", "ElectronicsTypes.list");
            var electronicsTypes = ReadAllLines(electronicsPath);

            string clothingPath = Path.Combine(currentDirectory, "Resources", "ClothingTypes.list");
            var clothingTypes = ReadAllLines(clothingPath);

            Food = CombineAll(companies, adjectives, foodTypes)
                .OrderBy(o => Random.Next())
                .ToList();

            Electronics = CombineAll(companies, adjectives, electronicsTypes)
                .OrderBy(o => Random.Next())
                .ToList();

            Clothing = CombineAll(companies, adjectives, clothingTypes)
                .OrderBy(o => Random.Next())
                .ToList();
        }

        public string GetRandomProductNameForCategory(CategoryType categoryType)
        {
            return categoryType switch
            {
                CategoryType.Food => GetRandomItem(Food),
                CategoryType.Electronics => GetRandomItem(Electronics),
                CategoryType.Clothing => GetRandomItem(Clothing),
                _ => string.Empty
            };
        }

        public List<string> GetAllProductNameForCategory(CategoryType categoryType, int maxCount = int.MaxValue)
        {
            return categoryType switch
            {
                CategoryType.Food => Food.GetRange(0, Math.Min(maxCount, Food.Count)),
                CategoryType.Electronics => Electronics.GetRange(0, Math.Min(maxCount, Electronics.Count)),
                CategoryType.Clothing => Clothing.GetRange(0, Math.Min(maxCount, Clothing.Count)),
                _ => new List<string>()
            };
        }

        static string GetRandomItem(List<string> list)
        {
            if (!list.Any())
                return string.Empty;

            return list[Random.Next(list.Count)];
        }

        private static List<string> CombineAll(
            List<string> companies,
            List<string> adjectives,
            List<string> productTypes)
        {
            List<string> products = new();

            foreach (var productType in productTypes)
            {
                foreach (var adjective in adjectives)
                {
                    foreach (var company in companies)
                    {
                        products.Add(new StringBuilder()
                                .Append('"')
                                .Append(adjective?.Trim().ToUpperFirst())
                                .Append(' ')
                                .Append(productType?.Trim().ToUpperFirst())
                                .Append("\" by ")
                                .Append(company?.Trim().ToUpperFirst())
                                .ToString());
                    }
                }
            }

            return products;
        }

        private static List<string> ReadAllLines(string path)
        {
            if (!File.Exists(path))
                return new List<string>();
            return
                File.ReadAllLines(path).ToList();
        }
    }
}
