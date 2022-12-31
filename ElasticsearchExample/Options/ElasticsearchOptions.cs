namespace ElasticsearchExample.Options
{
    public class ElasticsearchOptions
    {
        public const string Section = nameof(ElasticsearchOptions);

        public string ConnectionUsername { get; set; } = string.Empty;
        public string ConnectionPassword { get; set; } = string.Empty;
        public List<string> Nodes { get; set; } = new List<string>();
    }
}
