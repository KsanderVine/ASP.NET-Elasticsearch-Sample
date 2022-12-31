using ElasticsearchExample.Extensions;

namespace ElasticsearchExample.Demo
{
    public class HobbiesListGenerator : GeneratorBase
    {
        private static readonly Random Random = new(Seed);

        private int ListMinLength { get; set; } = 1;
        private int ListMaxLength { get; set; } = 5;
        private List<string> Hobbies { get; set; }

        public HobbiesListGenerator()
        {
            string currentDirectory = Directory.GetCurrentDirectory();

            string hobbiesPath = Path.Combine(currentDirectory, "Resources", "Hobbies.list");
            Hobbies = ReadAllLines(hobbiesPath);
        }

        public string GetOne()
        {
            int hobbyCount = Random.Next(ListMinLength, ListMaxLength);
            var hobbyList = GetUniqueHobbiesList(hobbyCount, Hobbies);

            return string.Join(", ", hobbyList);
        }

        public List<string> GetMany(int count)
        {
            List<string> hobbyLists = new();

            for (int i = 0; i < count; i++)
            {
                hobbyLists.Add(GetOne().ToUpperFirst());
            }

            return hobbyLists.ToList();
        }

        private static List<string> GetUniqueHobbiesList(int count, List<string> hobbies)
        {
            HashSet<string> hobbyLists = new();

            while (hobbyLists.Count < count)
            {
                var hobby = GetRandomItem(hobbies);
                hobbyLists.Add(hobby);
            }

            return hobbyLists.ToList();

            static string GetRandomItem(List<string> list)
            {
                if (!list.Any())
                    return string.Empty;

                return list[Random.Next(list.Count)];
            }
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
