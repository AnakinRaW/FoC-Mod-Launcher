using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog.Buttons;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;

public interface IDialogButtonFactory
{
    IButtonViewModel CreateOk(bool isDefault);

    IButtonViewModel CreateCancel(bool isDefault);

    IButtonViewModel CreateRetry(bool isDefault);

    IButtonViewModel CreateCustom(string id, ICommandDefinition command, bool isDefault);
}