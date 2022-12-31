using System.Text;

namespace ElasticsearchExample.Demo
{
    public class PhoneNumberGenerator : GeneratorBase
    {
        private static readonly Random Random = new(Seed);

        public string GetOne()
        {
            return new StringBuilder()
                .Append("+1")
                .Append(Random.Next(200, 999))
                .Append(Random.Next(200, 999))
                .Append(Random.Next(0, 9999)
                    .ToString()
                    .PadLeft(4, '0'))
                .ToString();
        }

        public List<string> GetMany(int count)
        {
            HashSet<string> phoneNumbers = new();

            while (phoneNumbers.Count < count)
            {
                phoneNumbers.Add(GetOne());
            }

            return phoneNumbers.ToList();
        }
    }
}
