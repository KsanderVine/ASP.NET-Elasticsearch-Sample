namespace ElasticsearchExample.Extensions
{
    public static class StringExtensions
    {
        public static string ToUpperFirst(this string str)
        {
            return str.Length switch
            {
                0 => str,
                1 => str.ToUpper(),
                _ => $"{char.ToUpper(str[0])}{str[1..]}"
            };
        }
    }
}
