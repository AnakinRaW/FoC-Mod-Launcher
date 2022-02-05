using System.Collections.Generic;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public interface IInvocationHandler
{
    bool Invoke(IEnumerable<object> items, InputSource source);
}