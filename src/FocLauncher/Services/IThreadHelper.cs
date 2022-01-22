using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FocLauncher.Services;

public interface IThreadHelper
{
    void OnUIThread(Action action, DispatcherPriority priority = DispatcherPriority.Normal);

    Task OnUIThread(Func<Task> action);
}