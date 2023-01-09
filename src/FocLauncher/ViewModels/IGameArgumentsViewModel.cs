using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

namespace FocLauncher.ViewModels;

public interface IGameArgumentsViewModel : IViewModel
{
    public object? CurrentGameObject { get; set; }
}