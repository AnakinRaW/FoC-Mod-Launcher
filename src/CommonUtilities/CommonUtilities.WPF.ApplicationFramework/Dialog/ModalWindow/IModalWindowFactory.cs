using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;

public interface IModalWindowFactory
{
    ModalWindow Create(IModalWindowViewModel viewModel);
}