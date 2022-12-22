﻿using System.Collections.Generic;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public interface IDialogViewModel : IWindowViewModel
{
    string? ResultButton { get; }

    public IList<IButtonViewModel> Buttons { get; }
}