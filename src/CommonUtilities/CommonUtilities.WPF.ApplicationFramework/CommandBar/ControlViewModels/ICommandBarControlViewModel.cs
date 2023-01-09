namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.CommandBar;

internal interface ICommandBarControlViewModel
{
    CommandBarType Type { get; }

    string? Text { get; set; }
}