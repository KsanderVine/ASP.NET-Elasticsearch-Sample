namespace ElasticsearchExample.Dtos
{
    public class ProductReadDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public decimal Price { get; set; }
    }
}
