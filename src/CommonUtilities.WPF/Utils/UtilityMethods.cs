using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sklavenwalker.CommonUtilities.Wpf.Utils;

internal static class UtilityMethods
{
    public static void HitTestVisibleElements(Visual visual, HitTestResultCallback resultCallback, HitTestParameters parameters)
    {
        VisualTreeHelper.HitTest(visual, ExcludeNonVisualElements, resultCallback, parameters);
    }

    private static HitTestFilterBehavior ExcludeNonVisualElements(
        DependencyObject potentialHitTestTarget)
    {
        return potentialHitTestTarget is not Visual || potentialHitTestTarget is UIElement uiElement &&
            (!uiElement.IsVisible || !uiElement.IsEnabled)
                ? HitTestFilterBehavior.ContinueSkipSelfAndChildren
                : HitTestFilterBehavior.Continue;
    }

    public static void InvalidateRecursive(UIElement? parent) => InvalidateRecursiveToType<ItemsPresenter>(parent);

    public static void InvalidateRecursiveToType<T>(UIElement? parent)
    {
        if (parent == null)
            return;
        parent.InvalidateMeasure();
        parent.InvalidateArrange();
        if (parent is T)
            return;
        var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (var childIndex = 0; childIndex < childrenCount; ++childIndex)
            InvalidateRecursive(VisualTreeHelper.GetChild(parent, childIndex) as UIElement);
    }
}