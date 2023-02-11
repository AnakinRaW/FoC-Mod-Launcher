using System;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ProgressBar;

public interface IProgressBarViewModel : IViewModel
{
    string? LeftHeaderText { get; }

    string? RightHeaderText { get; }

    string? FooterText { get; }

    double ProgressValue { get; }
}

public class ProgressBarViewModel : ViewModelBase, IProgressBarViewModel
{
    public ProgressBarViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public string? LeftHeaderText { get; }
    public string? RightHeaderText { get; }
    public string? FooterText { get; }
    public double ProgressValue { get; }
}