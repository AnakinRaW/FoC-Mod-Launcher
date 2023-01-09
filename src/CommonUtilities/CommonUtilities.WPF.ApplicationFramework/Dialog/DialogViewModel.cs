using System;
using System.Collections.Generic;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog.Buttons;
using AnakinRaW.CommonUtilities.Wpf.Controls;
using AnakinRaW.CommonUtilities.Wpf.Input;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;

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

    public virtual IDialogAdditionalInformationViewModel? AdditionalInformation => null;

    public IDelegateCommand<IButtonViewModel> UnifiedButtonCommand { get; }

    protected DialogViewModel(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
        UnifiedButtonCommand = new DelegateCommand<IButtonViewModel>(Execute, CanExecute);
    }

    protected virtual IList<IButtonViewModel> CreateButtons(IDialogButtonFactory buttonFactory)
    {
        var okButton = buttonFactory.CreateOk(true);
        var buttons = new List<IButtonViewModel> { okButton };
        return buttons;
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