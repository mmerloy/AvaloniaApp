namespace Utils;

public static class EnumerableExtensions
{
    public static IEnumerable<T> ModifyForEach<T>(this IEnumerable<T> source, Action<T> modifier) 
        where T : class
    {
        foreach (var item in source)
            modifier?.Invoke(item);
        
        return source;
    }

    public static bool IsEmpty<T>(this IEnumerable<T> source)
        => !source.Any();
}
