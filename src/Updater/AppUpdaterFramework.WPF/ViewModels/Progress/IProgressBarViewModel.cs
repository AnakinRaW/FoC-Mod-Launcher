using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

namespace AnakinRaW.AppUpdaterFramework.ViewModels.Progress;

public interface IProgressBarViewModel : IViewModel
{
    string? LeftHeaderText { get; }

    string? RightHeaderText { get; }

    string? FooterText { get; }

    double ProgressValue { get; }
}