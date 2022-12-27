using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;

namespace FocLauncher.ViewModels;

public interface IImageDialogViewModel : IDialogViewModel
{
    ImageKey Image { get; }
}