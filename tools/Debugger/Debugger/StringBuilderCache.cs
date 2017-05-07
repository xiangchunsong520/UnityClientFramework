using System;
using System.Text;

internal static class StringBuilderCache
{
    [ThreadStatic]
    private static StringBuilder CachedInstance;
    private const int MAX_BUILDER_SIZE = 512;

    public static StringBuilder Acquire(int capacity = 16)
    {
        if (capacity <= MAX_BUILDER_SIZE)
        {
            StringBuilder sb = CachedInstance;
            if ((sb != null) && (capacity <= sb.Capacity))
            {
                CachedInstance = null;
                sb.Length = 0;
                return sb;
            }
        }
        return new StringBuilder(capacity);
    }

    public static string GetStringAndRelease(StringBuilder sb)
    {
        Release(sb);
        return sb.ToString();
    }

    public static void Release(StringBuilder sb)
    {
        if (sb.Capacity <= MAX_BUILDER_SIZE)
        {
            CachedInstance = sb;
        }
    }
}