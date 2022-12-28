using System.Threading.Tasks;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;

public interface IQueuedDialogService
{
    Task<string?> ShowDialog(IDialogViewModel viewModel);
}