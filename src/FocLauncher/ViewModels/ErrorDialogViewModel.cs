using System.Collections.Generic;
using System.Threading.Tasks;
using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace FocLauncher.ViewModels;

public class ErrorDialogViewModel : DialogViewModel, IErrorDialogViewModel
{
    public ErrorDialogViewModel(IList<IButtonViewModel> buttons) : base(buttons)
    {
        IsResizable = false;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}