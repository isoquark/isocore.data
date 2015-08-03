using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IQ.Core.Framework
{
    public static class Extensions
    {
        /// <summary>
        /// Forces iteration of an enumerable to enjoy consequent side-effects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items to enumerate</param>
        public static void Iterate<T>(this IEnumerable<T> items)
        {
            foreach (var item in items) ;
        }

        public static IReadOnlyCollection<TResult> Map<TSource, TResult>
            (this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            List<TResult> results = new List<TResult>();
            foreach (var item in source)
            {
                results.Add(selector(item));
            }
            return results;
        }

        /// <summary>
        /// Returns true if the predicate is satisifed by some item in the sequence
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="items">The items to search</param>
        /// <param name="f">The predicate to evaluate</param>
        /// <returns></returns>
        public static bool Exists<T>(this IEnumerable<T> items, Predicate<T> f)
        {

            foreach (var item in items)
                if (f(item))
                    return true;
            return false;
        }
    }
}
