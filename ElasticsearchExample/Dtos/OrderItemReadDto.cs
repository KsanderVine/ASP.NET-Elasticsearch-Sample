namespace ElasticsearchExample.Dtos
{
    public class OrderItemReadDto
    {
        public Guid Id { get; set; }

        public Guid CustomerId { get; set; }

        public Guid ProductId { get; set; }

        public decimal OrderPrice { get; set; }

        public int Quantity { get; set; }

        public DateTime OrderDate { get; set; }
    }
}
