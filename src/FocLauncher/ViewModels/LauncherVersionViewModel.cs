using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FocLauncher.ViewModels;

internal class LauncherVersionViewModel : ObservableObject, IDialogAdditionalInformationViewModel
{
    public string LauncherVersion => LauncherAssemblyInfo.InformationalVersion;
}