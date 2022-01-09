using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;

namespace FocLauncher.ViewModels;

public interface IImageDialogViewModel : IDialogViewModel
{
    ImageMoniker Image { get; }
}