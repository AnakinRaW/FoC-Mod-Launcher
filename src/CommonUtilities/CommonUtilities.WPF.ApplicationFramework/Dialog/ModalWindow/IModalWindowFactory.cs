using AnakinRaW.CommonUtilities.Wpf.Controls;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;

public interface IModalWindowFactory
{
    ModalWindow Create(IModalWindowViewModel viewModel);
}