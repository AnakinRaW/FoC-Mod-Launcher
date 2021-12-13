using System.Windows;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Sklavenwalker.Wpf.CommandBar.Theming;

namespace Sklavenwalker.Wpf.CommandBar.Controls;

public class ExtendedContextMenu : ThemedContextMenu, IExposeStyleKeys
{
    private static ResourceKey? _buttonStyleKey;
    private static ResourceKey? _menuControllerStyleKey;
    private static ResourceKey? _comboBoxStyleKey;
    private static ResourceKey? _menuStyleKey;
    private static ResourceKey? _separatorStyleKey;
    
    public static ResourceKey ButtonStyleKey => _buttonStyleKey ??= new StyleKey<ExtendedContextMenu>();

    public static ResourceKey MenuControllerStyleKey => _menuControllerStyleKey ??= new StyleKey<ExtendedContextMenu>();

    public static ResourceKey ComboBoxStyleKey => _comboBoxStyleKey ??= new StyleKey<ExtendedContextMenu>();

    public static ResourceKey MenuStyleKey => _menuStyleKey ??= new StyleKey<ExtendedContextMenu>();

    public static ResourceKey SeparatorStyleKey => _separatorStyleKey ??= new StyleKey<ExtendedContextMenu>();

    ResourceKey IExposeStyleKeys.ButtonStyleKey => ButtonStyleKey;

    ResourceKey IExposeStyleKeys.MenuControllerStyleKey => MenuControllerStyleKey;

    ResourceKey IExposeStyleKeys.ComboBoxStyleKey => ComboBoxStyleKey;

    ResourceKey IExposeStyleKeys.MenuStyleKey => MenuStyleKey;

    ResourceKey IExposeStyleKeys.SeparatorStyleKey => SeparatorStyleKey;


    private bool AreUnderlinesShown => ShowOptions.HasFlag(MenuShowOptions.ShowMnemonics);

    private bool IsFirstItemSelected => ShowOptions.HasFlag(MenuShowOptions.SelectFirstItem);

    private bool IsTypeAheadSupported => ShowOptions.HasFlag(MenuShowOptions.SupportsTypeAhead);

    private bool LeftAligned => ShowOptions.HasFlag(MenuShowOptions.LeftAlign);

    private bool RightAligned => !LeftAligned && ShowOptions.HasFlag(MenuShowOptions.RightAlign);

    static ExtendedContextMenu()
    { 
       // DefaultStyleKeyProperty.OverrideMetadata(typeof(ExtendedContextMenu), new FrameworkPropertyMetadata(typeof(ExtendedContextMenu)));
    }
    

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new ExtendedMenuItem();
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        //TODO: Utility.SelectStyleForItem(element as FrameworkElement, item, (IExposeStyleKeys)this);
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        // TODO: Decide if we want build items here or before we open it.
        //if (e.Property == ItemsSourceProperty && e.NewValue != null && DataContext is IVsUIDataSource dataContext)
        //    dataContext.Invoke("UpdateItems", null, out object _);
        base.OnPropertyChanged(e);
    }

    protected override bool FilterNonVisibleItems(object item)
    {
        // TODO:
        return true;
    }
}