using System.Windows.Controls;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Models;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.CommandBar;

public abstract class CommandBarProvider<TModel, TControl> where TModel : ICommandBarItemsSource where TControl : ItemsControl
{
    public TControl Provide(TModel model)
    {
        var control = CreateControl();
        var f = new ItemsControlFactory();
        var i = f.CreateModel(model);
        control.ItemsSource = i;
        return control;
    }

    protected abstract TControl CreateControl();
}