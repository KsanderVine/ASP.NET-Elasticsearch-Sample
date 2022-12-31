using ElasticsearchExample.Extensions;
using System.Text;

namespace ElasticsearchExample.Demo
{
    public class NameGenerator : GeneratorBase
    {
        private static readonly Random Random = new(Seed);

        private List<string> GivenNames { get; set; }
        private List<string> Surnames { get; set; }

        public NameGenerator()
        {
            string currentDirectory = Directory.GetCurrentDirectory();

            string givenNamesPath = Path.Combine(currentDirectory, "Resources", "Givennames.list");
            GivenNames = ReadAllLines(givenNamesPath);

            string surnamesPath = Path.Combine(currentDirectory, "Resources", "Surnames.list");
            Surnames = ReadAllLines(surnamesPath);
        }

        public string GetOneGivenName()
        {
            return new StringBuilder()
                .Append(GetRandomItem(GivenNames).ToUpperFirst())
                .ToString();
        }

        public List<string> GetManyGivenNames(int count)
        {
            List<string> names = new();

            while (names.Count < count)
            {
                names.Add(GetOneGivenName());
            }

            return names;
        }

        public string GetOneSurname()
        {
            return new StringBuilder()
                .Append(GetRandomItem(Surnames).ToUpperFirst())
                .ToString();
        }

        public List<string> GetManySurnames(int count)
        {
            List<string> surnames = new();

            while (surnames.Count < count)
            {
                surnames.Add(GetOneSurname());
            }

            return surnames;
        }

        public List<Name> GetManyNames(int count)
        {
            List<Name> names = new();

            while (names.Count < count)
            {
                names.Add(new Name()
                {
                    Givenname = GetOneGivenName(),
                    Surname = GetOneSurname()
                });
            }

            return names;
        }

        private static List<string> ReadAllLines(string path)
        {
            if (!File.Exists(path))
                return new List<string>();
            return
                File.ReadAllLines(path).ToList();
        }

        private static string GetRandomItem(List<string> list)
        {
            if (!list.Any())
                return string.Empty;

            return list[Random.Next(list.Count)];
        }

        public class Name
        {
            public string Givenname { get; set; } = string.Empty;
            public string Surname { get; set; } = string.Empty;
        }
    }
}
