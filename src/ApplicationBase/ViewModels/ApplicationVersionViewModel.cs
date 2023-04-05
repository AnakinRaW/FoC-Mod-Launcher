using System;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace AnakinRaW.ApplicationBase.ViewModels;

internal class ApplicationVersionViewModel : ObservableObject, IDialogAdditionalInformationViewModel
{
    public string Version { get; }

    public ApplicationVersionViewModel(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        Version = serviceProvider.GetRequiredService<IApplicationEnvironment>().InformationalVersion.ToString();
    }
}