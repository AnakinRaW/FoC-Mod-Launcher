using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FocLauncher.Threading
{
    public static class Extensions
    {
        public static Task ForeachAsync<T>(this IEnumerable<T> list, Func<T, Task> function)
        {
            if (list == null)
                throw new NullReferenceException(nameof(list));
            return Task.WhenAll(list.Select(function));
        }

        public static async Task<IEnumerable<TOut>> ForeachAsync<TIn, TOut>(this IEnumerable<TIn> list, Func<TIn, Task<TOut>> function)
        {
            var loopResult = await Task.WhenAll(list.Select(function));
            return loopResult.ToList().AsEnumerable();
        }
    }
}
