using System.Threading.Tasks;
using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace FocLauncher.Services;

internal interface IQueuedDialogService
{
    Task<string?> ShowDialog(IDialogViewModel viewModel);
}