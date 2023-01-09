﻿using System.Collections.Generic;
using System.ComponentModel;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog.Buttons;
using AnakinRaW.CommonUtilities.Wpf.Controls;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;

public interface IDialogViewModel : IModalWindowViewModel
{
    string? ResultButton { get; }

    public IList<IButtonViewModel> Buttons { get; }

    public IDialogAdditionalInformationViewModel? AdditionalInformation { get; }
}

public interface IDialogAdditionalInformationViewModel : INotifyPropertyChanged
{
}