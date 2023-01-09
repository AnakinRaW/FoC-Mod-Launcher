using System.Windows;
using System.Windows.Controls;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog.Buttons;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;

internal class DialogButtonTemplateSelector : DataTemplateSelector
{
    public bool Theme { get; set; }

    public DataTemplate? ButtonTemplate { get; set; }

    public DataTemplate? ThemedButtonTemplate { get; set; }
    
    public DataTemplate? DropDownButtonTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        return item switch
        {
            IDropDownButtonViewModel => DropDownButtonTemplate,
            IButtonViewModel { Themed: true } => ThemedButtonTemplate,
            IButtonViewModel { Themed: false } => ButtonTemplate,
            _ => null
        };
    }
}