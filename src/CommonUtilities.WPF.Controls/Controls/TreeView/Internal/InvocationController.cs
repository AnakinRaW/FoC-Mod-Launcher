using System;
using System.Collections.Generic;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

internal class InvocationController : ItemsProviderCollector<IInvokable, IInvocationHandler>
{
    public static bool Invoke(IInvokable item, InputSource inputSource = InputSource.None)
    {
        return Invoke(new[]
        {
            item
        }, inputSource);
    }

    public static bool Invoke(IEnumerable<IInvokable> items, InputSource inputSource = InputSource.None)
    {
        return Invoke(items, inputSource, item => item.InvocationHandler);
    }

    private static bool Invoke(IEnumerable<IInvokable> items, InputSource inputSource, Func<IInvokable, IInvocationHandler> getController)
    {
        var dictionary = CollectProviders(items, getController);
        var success = false;
        foreach (var keyValuePair in dictionary)
            success = keyValuePair.Key.Invoke(keyValuePair.Value, inputSource) | success;
        return success;
    }
}