using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

[ContentProperty("Child")]
public class VisualWrapper : FrameworkElement
{
    private Visual? _child;

    public Visual? Child
    {
        get => _child;
        set
        {
            if (_child != null) RemoveVisualChild(_child);

            _child = value;

            if (_child != null) AddVisualChild(_child);
        }
    }

    protected override Visual GetVisualChild(int index)
    {
        if (_child != null && index == 0)
            return _child;
        throw new ArgumentOutOfRangeException(nameof(index));
    }

    protected override int VisualChildrenCount => _child != null ? 1 : 0;
}