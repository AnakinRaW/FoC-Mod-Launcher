using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

namespace FocLauncher.Update.ViewModels;

public interface IProgressBarViewModel : IViewModel
{
    string? LeftHeaderText { get; }

    string? RightHeaderText { get; }

    string? FooterText { get; }

    double ProgressValue { get; }
}