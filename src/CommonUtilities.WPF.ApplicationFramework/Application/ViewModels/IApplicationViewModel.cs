using System;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

public interface IApplicationViewModel : IMainWindowViewModel, IViewModel, IDisposable
{
    IViewModel? CurrentViewModel { get; }
}