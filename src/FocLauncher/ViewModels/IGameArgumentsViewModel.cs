namespace FocLauncher.ViewModels;

public interface IGameArgumentsViewModel : ILauncherViewModel
{
    public object? CurrentGameObject { get; set; }
}