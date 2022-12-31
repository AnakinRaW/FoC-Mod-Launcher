namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.StatusBar;

internal class StatusBarService : IStatusBarService
{
    public IStatusBarViewModel StatusBarModel { get; internal set; } = null!;

    public T? GetModel<T>() where T : class, IStatusBarViewModel
    {
        return StatusBarModel as T;
    }
}