using System;
using System.Collections.Generic;

namespace AnakinRaW.CommonUtilities.Wpf.Controls;

internal class ItemsProviderCollector<TPattern, TController>
{
    protected static Dictionary<TController, IList<TPattern>> CollectProviders(
        IEnumerable<TPattern> items, Func<TPattern, TController> selectController)
    {
        var dictionary = new Dictionary<TController, IList<TPattern>>();
        foreach (var pattern in items)
        {
            var key = selectController(pattern);
            if (key != null)
            {
                if (!dictionary.TryGetValue(key, out var patternList))
                {
                    patternList = new List<TPattern>();
                    dictionary[key] = patternList;
                }
                patternList.Add(pattern);
            }
        }
        return dictionary;
    }
}