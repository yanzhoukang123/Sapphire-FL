using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.LaserScanner.ViewModel
{
    internal static class HelperMethod
    {
        // Create handy extension (from: https://stackoverflow.com/a/2576736/461998)
        public static void MoveToTop<T>(this List<T> list, int index)
        {
            T item = list[index];
            for (int i = index; i > 0; i--)
            {
                list[i] = list[i - 1];
            }
            list[0] = item;
        }

        public static void RenameKey<TKey, TValue>(this IDictionary<TKey, TValue> dic,
                                     TKey fromKey, TKey toKey)
        {
            TValue value = dic[fromKey];
            dic.Remove(fromKey);
            dic[toKey] = value;
        }
    }

}
