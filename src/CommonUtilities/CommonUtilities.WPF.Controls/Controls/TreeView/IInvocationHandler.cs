using System.Collections.Generic;

namespace AnakinRaW.CommonUtilities.Wpf.Controls;

public interface IInvocationHandler
{
    bool Invoke(IEnumerable<object> items, InputSource source);
}