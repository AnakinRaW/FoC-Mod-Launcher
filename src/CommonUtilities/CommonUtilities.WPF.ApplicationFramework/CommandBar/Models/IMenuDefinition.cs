namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Models;

public interface IMenuDefinition : ICommandBarItemDefinition, ICommandBarItemsSource
{
    bool IsEnabled { get; }
}