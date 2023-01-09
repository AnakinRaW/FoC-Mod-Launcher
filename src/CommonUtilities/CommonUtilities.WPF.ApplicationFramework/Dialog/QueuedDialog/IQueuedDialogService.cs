using System.Threading.Tasks;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;

public interface IQueuedDialogService
{
    Task<string?> ShowDialog(IDialogViewModel viewModel);
}