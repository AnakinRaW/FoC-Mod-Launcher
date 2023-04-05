using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.Imaging;

namespace AnakinRaW.ApplicationBase.ViewModels.Dialogs;

public interface IImageDialogViewModel : IDialogViewModel
{
    ImageKey Image { get; }
}