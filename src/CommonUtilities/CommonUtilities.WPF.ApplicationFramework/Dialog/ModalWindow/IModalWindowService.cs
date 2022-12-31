using System.Threading.Tasks;
using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;

public interface IModalWindowService
{
    Task ShowModal(IModalWindowViewModel viewModel);
}