namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.StatusBar;

public interface IStatusBarService
{
    IStatusBarViewModel StatusBarModel { get; }

    T? GetModel<T>() where T : class, IStatusBarViewModel;
}