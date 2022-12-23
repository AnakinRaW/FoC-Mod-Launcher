using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace FocLauncher.Views;

public partial class MainPage
{
    private readonly Item _item = new("123");

    public MainPage()
    {
        InitializeComponent();
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        var r = new ObservableCollection<object>();
        TreeView.RootItemsSource = r;
        
        r.Add(new Item("000"));
        r.Add(_item); 
        _item.Items.Add(new Item("345", _item));
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        _item.Items.Add(new Item("456", _item));
    }
}

public class Item : IHasChildrenVisibility, INotifyPropertyChanged
{
    public string Text { get; }

    public ObservableCollection<Item> Items { get; }

    public bool HasItems => Items.Count > 0;

    public Item? Parent { get; }

    public Item(string text, Item? parent = null)
    {
        Text = text;
        Parent = parent;

        Items = new ObservableCollection<Item>();
        Items.CollectionChanged += OnItemsChanged;

    }

    private void OnItemsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasItems));
    }

    public override string ToString()
    {
        return Text;
    }

    public bool ShowChildrenOnDefault => true;
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}