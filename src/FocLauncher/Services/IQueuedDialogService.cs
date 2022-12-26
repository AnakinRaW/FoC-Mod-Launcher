using System.Threading.Tasks;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;

namespace FocLauncher.Services;

internal interface IQueuedDialogService
{
    Task<string?> ShowDialog(IDialogViewModel viewModel);
}