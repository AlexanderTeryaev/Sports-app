using System.Collections.Generic;
using System.Linq;

namespace DataService.Utils
{
    public static class ListExtension
    {
        public static bool IsFirst<T>(this IList<T> items, T item)
        {
            if (items.Count == 0)
                return false;
            T first = items[0];
            return item.Equals(first);
        }

        public static bool IsLast<T>(this IList<T> items, T item)
        {
            if (items.Count == 0)
                return false;
            T last = items[items.Count - 1];
            return item.Equals(last);
        }
    }
}
