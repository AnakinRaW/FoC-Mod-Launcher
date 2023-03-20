using System;
using System.Windows.Threading;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework;

public interface IAppDispatcher
{
    void Invoke(Action action);

    T Invoke<T>(Func<T> func);

    void BeginInvoke(DispatcherPriority priority, Action action);
}