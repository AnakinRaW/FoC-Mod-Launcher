using System.Collections.Generic;

namespace FocLauncher.Controls
{
    public interface IInvocationController
    {
        bool Invoke(IEnumerable<object> items);
    }
}