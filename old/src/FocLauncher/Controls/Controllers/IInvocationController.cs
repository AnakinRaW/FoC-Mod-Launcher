using System.Collections.Generic;

namespace FocLauncher.Controls.Controllers
{
    public interface IInvocationController
    {
        bool Invoke(IEnumerable<object> items);
    }
}