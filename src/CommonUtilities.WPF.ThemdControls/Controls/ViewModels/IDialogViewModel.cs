using System;
using System.Collections.Generic;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public interface IDialogViewModel : IWindowViewModel
{
    event EventHandler CloseDialogRequest;

    string? ResultButton { get; }

    public IList<IButtonViewModel> Buttons { get; }

    void CloseDialog();
}