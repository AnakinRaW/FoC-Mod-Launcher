namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar;

public interface ICommandBarControlViewModel
{
    CommandBarType Type { get; }

    string? Text { get; set; }
}