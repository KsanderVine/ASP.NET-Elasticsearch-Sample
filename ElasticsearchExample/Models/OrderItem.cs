using System.Text.Json.Serialization;

namespace ElasticsearchExample.Models
{
    public class OrderItem : BaseModel
    {
        public Guid CustomerId { get; set; }

        [JsonIgnore]
        public Customer? Customer { get; set; }

        public Guid ProductId { get; set; }

        [JsonIgnore]
        public Product? Product { get; set; }

        public decimal OrderPrice { get; set; }

        public int Quantity { get; set; }
    }
}
