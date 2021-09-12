namespace Dirt
{
    public static class SimpleHash
    {
        public static int GetHash(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            unchecked
            {
                int hash = 23;
                foreach (char c in text)
                {
                    hash = hash * 31 + c;
                }
                return hash;
            }
        }
    }
}
