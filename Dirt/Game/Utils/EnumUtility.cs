namespace Dirt.Game.Utils
{
    public static class EnumUtility
    {
        public static int GetHighestValue<T>() where T : System.Enum
        {
            var values = typeof(T).GetEnumValues();
            int max = 0;
            for (int i = 0; i < values.Length; ++i)
            {
                object genValue = values.GetValue(i);
                int enumV = System.Convert.ToInt32(genValue);

                if (enumV > max)
                    max = enumV;
            }
            return max;
        }
        public static T[] CreateArray<T, C>() where C : System.Enum
        {
            int size = GetHighestValue<C>();
            return new T[size + 1];
        }
    }
}
