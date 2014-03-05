using System;
using System.Linq;
using System.Collections.Generic;

namespace LinqToGatherer
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            if (collection == null || action == null)
                return;

            foreach (T t in collection)
                action(t);
        }

        public static void AddRange<T>(this IList<T> collection, IEnumerable<T> newItems)
        {
            if (collection == null || newItems == null)
                return;

            foreach (T t in newItems)
                collection.Add(t);
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> coll)
        {
            return coll == null || !coll.Any();
        }
    }
}
