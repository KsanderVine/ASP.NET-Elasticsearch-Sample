using System.Text.Json.Serialization;

namespace ElasticsearchExample.Models
{
    public class Product : BaseModel
    {
        public enum CategoryType
        {
            Food,
            Electronics,
            Clothing
        }

        public string Name { get; set; } = string.Empty;

        public CategoryType Category { get; set; }

        public decimal Price { get; set; }

        [JsonIgnore]
        public IEnumerable<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
