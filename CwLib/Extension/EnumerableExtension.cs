using System;
using System.Collections.Generic;
using System.Linq;

namespace CwLib.Extension
{
    public static class EnumerableExtension
    {
        /// <summary>
        /// 分頁
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> ToPaged<T>(this IEnumerable<T> list, int pageSize)
        {
            if (list == null)
            {
                yield break;
            }

            var tempList = list.ToList();
            var remainder = tempList.Count() % pageSize;
            var iTotalPages = Convert.ToInt32(Math.Ceiling((decimal)tempList.Count() / pageSize));

            for (var i = 1; i < iTotalPages; i++)
            {
                yield return tempList.GetRange(pageSize * (i - 1), pageSize);
            }

            if (remainder > 0)
            {
                yield return tempList.GetRange(tempList.Count - remainder, remainder);
            }
        }

        /// <summary>
        /// Fors the each.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values">The values.</param>
        /// <param name="action">The action.</param>
        public static void ForEach<T>(this IEnumerable<T> values, Action<T> action)
        {
            foreach (var value in values)
            {
                action(value);
            }
        }
    }
}