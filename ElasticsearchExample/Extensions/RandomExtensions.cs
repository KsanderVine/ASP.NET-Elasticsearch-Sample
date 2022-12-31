namespace ElasticsearchExample.Extensions
{
    public static class RandomExtensionMethods
    {
        public static long NextLong(this Random random)
        {
            return random.NextLong(long.MinValue, long.MaxValue);
        }

        public static long NextLong(this Random random, long max)
        {
            return random.NextLong(0, max);
        }

        public static long NextLong(this Random random, long min, long max)
        {
            if (max <= min)
                throw new ArgumentException($"{nameof(max)} must be more then {nameof(min)}");

            ulong uRange = (ulong)(max - min);
            ulong ulongRand;

            do
            {
                byte[] buf = new byte[8];
                random.NextBytes(buf);
                ulongRand = (ulong)BitConverter.ToInt64(buf, 0);
            }
            while (ulongRand > ulong.MaxValue - ((ulong.MaxValue % uRange) + 1) % uRange);

            return (long)(ulongRand % uRange) + min;
        }
    }
}
