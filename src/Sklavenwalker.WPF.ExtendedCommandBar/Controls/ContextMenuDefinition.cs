using System.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Sklavenwalker.Wpf.CommandBar.Controls;

[DebuggerDisplay("Name = {" + nameof(Name) + "}")]
public abstract partial class CommandBarDataSource : ObservableObject
{
    /// <summary>
    /// The name of the definition
    /// </summary>
    [ObservableProperty] private string _name;
}

public abstract class CommandBarItemDataSource : CommandBarDataSource
{

}

public class CommandBarButtonDataSource : CommandBarItemDataSource
{
    public CommandBarButtonDataSource()
    {
        
    }
}



public abstract class CommandBarDefinitionBase
{

}