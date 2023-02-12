using System;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using Validation;

namespace FocLauncher.Update.ViewModels.ProductStates;

internal class UpdatingStateViewModel : ViewModelBase, IUpdatingStateViewModel
{
    public IProgressViewModel ProgressViewModel { get; }

    public UpdatingStateViewModel(IProgressViewModel progressViewModel, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Requires.NotNull(progressViewModel, nameof(progressViewModel));
        ProgressViewModel = progressViewModel;
    }
}