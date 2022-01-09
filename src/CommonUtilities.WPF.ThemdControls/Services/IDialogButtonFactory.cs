using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.Services;

public interface IDialogButtonFactory
{
    IButtonViewModel CreateOk(bool isDefault);

    IButtonViewModel CreateCancel(bool isDefault);

    IButtonViewModel CreateRetry(bool isDefault);
}