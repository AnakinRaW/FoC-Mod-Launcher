using System.Collections.Generic;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;
using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;

public interface IDialogViewModel : IModalWindowViewModel
{
    string? ResultButton { get; }

    public IList<IButtonViewModel> Buttons { get; }
}