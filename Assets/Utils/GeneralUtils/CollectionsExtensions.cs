using System.Collections.Generic;

public static class CollectionsExtensions
{
    public static T Random<T>(this IReadOnlyList<T> items)
    {
        return items.Count <= 0 ? default(T) : items[UnityEngine.Random.Range(0, items.Count)];
    }
    
    public static T PopRandom<T>(this List<T> items)
    {
        if(items.Count <= 0) return default(T);
        var index = UnityEngine.Random.Range(0, items.Count);
        var item = items[index];
        items.RemoveAt(index);
        return item;
    }
    
    public static T Random<T>(this IEnumerable<T> items, T exception)
    {
        var itemsCopy = new List<T>(items);
        itemsCopy.RemoveAll(i => i.Equals(exception));
        return itemsCopy.Random();
    }
    
    public static T Random<T>(this IEnumerable<T> items, List<T> exceptions)
    {
        var itemsCopy = new List<T>(items);
        itemsCopy.RemoveAll(exceptions.Contains);
        return itemsCopy.Random();
    }
    
    public static List<T> Random<T>(this IEnumerable<T> items, int amount, List<T> exceptions)
    {
        var itemsCopy = new List<T>(items);
        itemsCopy.RemoveAll(exceptions.Contains);

        var randoms = new List<T>();
        for (var i = 0; i < amount; i++)
            randoms.Add(itemsCopy.PopRandom());
        return randoms;
    }
    
    public static List<T> Random<T>(this IEnumerable<T> items, int amount)
    {
        var itemsCopy = new List<T>(items);
        var randoms = new List<T>();
        for (var i = 0; i < amount; i++)
            randoms.Add(itemsCopy.PopRandom());
        return randoms;
    }
}