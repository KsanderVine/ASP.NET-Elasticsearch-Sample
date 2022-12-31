namespace ElasticsearchExample.Dtos
{
    public class CustomerReadDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Surname { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Hobbies { get; set; } = string.Empty;
    }
}
