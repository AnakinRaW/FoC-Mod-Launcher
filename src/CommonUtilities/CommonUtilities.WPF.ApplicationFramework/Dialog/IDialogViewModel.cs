using System.Collections.Generic;
using System.ComponentModel;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog.Buttons;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;

public interface IDialogViewModel : IModalWindowViewModel
{
    string? ResultButton { get; }

    public IList<IButtonViewModel> Buttons { get; }

    public IDialogAdditionalInformationViewModel? AdditionalInformation { get; }
}

public interface IDialogAdditionalInformationViewModel : INotifyPropertyChanged
{
}