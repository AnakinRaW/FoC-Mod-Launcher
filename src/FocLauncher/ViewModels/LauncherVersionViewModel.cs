using CommunityToolkit.Mvvm.ComponentModel;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;

namespace FocLauncher.ViewModels;

internal partial class LauncherVersionViewModel : ObservableObject, IDialogAdditionalInformationViewModel
{
    public string LauncherVersion => LauncherAssemblyInfo.InformationalVersion + " " + LauncherAssemblyInfo.FileVersion + " " + LauncherAssemblyInfo.AssemblyVersion;
}