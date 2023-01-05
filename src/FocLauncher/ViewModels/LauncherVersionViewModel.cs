using CommunityToolkit.Mvvm.ComponentModel;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;

namespace FocLauncher.ViewModels;

internal class LauncherVersionViewModel : ObservableObject, IDialogAdditionalInformationViewModel
{
    public string LauncherVersion => LauncherAssemblyInfo.InformationalVersion;
}