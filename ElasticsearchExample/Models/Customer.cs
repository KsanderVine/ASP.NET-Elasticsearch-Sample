using System.Text.Json.Serialization;

namespace ElasticsearchExample.Models
{
    public class Customer : BaseModel
    {
        public string Name { get; set; } = string.Empty;

        public string Surname { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Hobbies { get; set; } = string.Empty;

        [JsonIgnore]
        public IEnumerable<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
