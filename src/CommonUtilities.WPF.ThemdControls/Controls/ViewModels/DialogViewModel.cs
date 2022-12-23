using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public abstract class DialogViewModel : ModalWindowViewModel, IDialogViewModel
{
    private IList<IButtonViewModel>? _buttons;

    public string? ResultButton { get; private set; }

    public IList<IButtonViewModel> Buttons
    {
        get
        {
            if (_buttons is null)
            {
                _buttons = CreateButtons();
                ValidateButtons(_buttons);
            }
            return _buttons;
        }
    }

    public IRelayCommand<IButtonViewModel> UnifiedButtonCommand { get; }

    protected DialogViewModel()
    {
        UnifiedButtonCommand = new RelayCommand<IButtonViewModel>(Execute, CanExecute);
    }

    protected virtual IList<IButtonViewModel> CreateButtons()
    {
        return new List<IButtonViewModel>();
    }

    private void Execute(IButtonViewModel? button)
    {
        if (button is null)
            return;
        ResultButton = button.Id;
        button.CommandDefinition.Command?.Execute(this);
    }

    private bool CanExecute(IButtonViewModel? button)
    {
        return button?.CommandDefinition.Command is null || button.CommandDefinition.Command.CanExecute(this);
    }

    private static void ValidateButtons(IEnumerable<IButtonViewModel> buttons)
    {
        var flag1 = false;
        var flag2 = false;
        foreach (var button in buttons)
        {
            if (button.IsDefault)
                flag2 = !flag2 ? true : throw new InvalidOperationException("Dialog cannot have multiple default buttons");
            if (button.IsCancel)
                flag1 = !flag1 ? true : throw new InvalidOperationException("Dialog cannot have multiple cancel buttons");
        }
        if (!flag1 && !flag2)
            throw new InvalidOperationException("Dialog requires a default or cancel button");
    }
}