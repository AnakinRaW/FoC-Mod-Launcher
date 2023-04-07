using System;
using System.Windows.Input;
using AnakinRaW.ApplicationBase.ViewModels.Dialogs;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;

namespace AnakinRaW.ApplicationBase.Services;

internal class WpfAppResetHandler : AppResetHandler
{
    public WpfAppResetHandler(IServiceProvider services) : base(services)
    {
    }

    protected override bool RequiresReset()
    {
        return base.RequiresReset() || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
    }

    protected override void OnResetFailed(Exception exception)
    {
        new UnhandledExceptionDialog(new UnhandledExceptionDialogViewModel(exception, Services)).ShowModal();
    }
}