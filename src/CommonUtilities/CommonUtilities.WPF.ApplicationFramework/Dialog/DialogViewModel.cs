using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog.Buttons;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Sklavenwalker.CommonUtilities.Wpf.Input;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;

public abstract class DialogViewModel : ModalWindowViewModel, IDialogViewModel
{
    private readonly IServiceProvider _serviceProvider;
    private IList<IButtonViewModel>? _buttons;

    public string? ResultButton { get; private set; }

    public IList<IButtonViewModel> Buttons
    {
        get
        {
            if (_buttons is null)
            {
                var buttonFactory = _serviceProvider.GetRequiredService<IDialogButtonFactory>();
                _buttons = CreateButtons(buttonFactory);
                ValidateButtons(_buttons);
            }
            return _buttons;
        }
    }

    public IDelegateCommand<IButtonViewModel> UnifiedButtonCommand { get; }

    protected DialogViewModel(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
        UnifiedButtonCommand = new DelegateCommand<IButtonViewModel>(Execute, CanExecute);
    }

    protected virtual IList<IButtonViewModel> CreateButtons(IDialogButtonFactory buttonFactory)
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