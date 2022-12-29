using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog.Buttons;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;

public interface IDialogButtonFactory
{
    IButtonViewModel CreateOk(bool isDefault);

    IButtonViewModel CreateCancel(bool isDefault);

    IButtonViewModel CreateRetry(bool isDefault);
}