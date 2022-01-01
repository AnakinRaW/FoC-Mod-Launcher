using System.Windows;
using System.Windows.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

internal class DialogButtonTemplateSelector : DataTemplateSelector
{
    public DataTemplate? ButtonTemplate { get; set; }
    
    public DataTemplate? DropDownButtonTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        return item switch
        {
            IDropDownButtonViewModel => DropDownButtonTemplate,
            IButtonViewModel => ButtonTemplate,
            _ => null
        };
    }
}