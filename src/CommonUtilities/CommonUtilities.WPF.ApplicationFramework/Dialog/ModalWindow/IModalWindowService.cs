using System.Threading.Tasks;
using AnakinRaW.CommonUtilities.Wpf.Controls;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;

public interface IModalWindowService
{
    Task ShowModal(IModalWindowViewModel viewModel);
}