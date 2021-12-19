using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Sklavenwalker.CommonUtilities.Wpf.Utilities;

public static class ExtensionMethods
{
    public static bool IsConnectedToPresentationSource(this DependencyObject obj) => PresentationSource.FromDependencyObject(obj) != null;

    public static void AddPropertyChangeHandler<T>(this T instance, DependencyProperty property, EventHandler handler) 
        where T : DependencyObject
    {
        instance.AddPropertyChangeHandler(property, handler, typeof(T));
    }

    public static void AddPropertyChangeHandler<T>(this T instance, DependencyProperty property, EventHandler handler, Type targetType)
        where T : DependencyObject
    {
        DependencyPropertyDescriptor.FromProperty(property, targetType).AddValueChanged(instance, handler);
    }

    public static void RemovePropertyChangeHandler<T>(this T instance, DependencyProperty property, EventHandler handler)
        where T : DependencyObject
    {
        instance.RemovePropertyChangeHandler(property, handler, typeof(T));
    }

    public static void RemovePropertyChangeHandler<T>(this T instance, DependencyProperty property, EventHandler handler, Type targetType)
        where T : DependencyObject
    {
        DependencyPropertyDescriptor.FromProperty(property, targetType).RemoveValueChanged(instance, handler);
    }

    public static TAncestorType? FindAncestor<TAncestorType>(this Visual obj) where TAncestorType : DependencyObject
    {
        return obj.FindAncestor<TAncestorType, DependencyObject>(GetVisualOrLogicalParent);
    }

    public static TAncestorType? FindAncestor<TAncestorType, TElementType>(this TElementType obj, Func<TElementType, TElementType?> parentEvaluator)
        where TAncestorType : class
    {
        return obj.FindAncestor(parentEvaluator, ancestor => ancestor is TAncestorType) as TAncestorType;
    }

    public static object? FindAncestor<TElementType>(this TElementType obj,
        Func<TElementType, TElementType?> parentEvaluator, Func<TElementType, bool> ancestorSelector)
    {
        for (var ancestor = parentEvaluator(obj); ancestor != null; ancestor = parentEvaluator(ancestor))
        {
            if (ancestorSelector(ancestor))
                return ancestor;
        }

        return null;
    }

    public static DependencyObject? GetVisualOrLogicalParent(this DependencyObject? sourceElement)
    {
        if (sourceElement == null)
            return null;
        return sourceElement is Visual ? VisualTreeHelper.GetParent(sourceElement) ?? LogicalTreeHelper.GetParent(sourceElement) : LogicalTreeHelper.GetParent(sourceElement);
    }

    public static TAncestorType? FindAncestorOrSelf<TAncestorType>(this Visual obj) where TAncestorType : DependencyObject
    {
        return obj.FindAncestorOrSelf<TAncestorType, DependencyObject>(GetVisualOrLogicalParent);
    }

    public static TAncestorType? FindAncestorOrSelf<TAncestorType, TElementType>(this TElementType obj, Func<TElementType, TElementType?> parentEvaluator)
        where TAncestorType : DependencyObject
    {
        return obj is TAncestorType ancestorType ? ancestorType : obj.FindAncestor<TAncestorType, TElementType>(parentEvaluator);
    }

    public static object? FindAncestorOrSelf<TElementType>(this TElementType obj, 
        Func<TElementType, TElementType?> parentEvaluator, Func<TElementType, bool> ancestorSelector)
    {
        return ancestorSelector(obj) ? obj : obj.FindAncestor(parentEvaluator, ancestorSelector);
    }

    public static IEnumerable<T> FindDescendants<T>(this DependencyObject? obj) where T : class
    {
        if (obj == null)
            return Enumerable.Empty<T>();
        var descendants = new List<T>();
        obj.TraverseVisualTree<T>(child => descendants.Add(child));
        return descendants;
    }

    public static void TraverseVisualTree<T>(this DependencyObject? obj, Action<T> action) where T : class
    {
        if (obj == null)
            return;
        for (var childIndex = 0; childIndex < VisualTreeHelper.GetChildrenCount(obj); ++childIndex)
        {
            var child = VisualTreeHelper.GetChild(obj, childIndex);
            if (child != null)
            {
                var obj1 = child as T;
                child.TraverseVisualTreeReverse(action);
                if (obj1 != null)
                    action(obj1);
            }
        }
    }

    public static void TraverseVisualTreeReverse<T>(this DependencyObject? obj, Action<T> action) where T : class
    {
        if (obj == null)
            return;
        for (var childIndex = VisualTreeHelper.GetChildrenCount(obj) - 1; childIndex >= 0; --childIndex)
        {
            var child = VisualTreeHelper.GetChild(obj, childIndex);
            if (child != null)
            {
                var obj1 = child as T;
                child.TraverseVisualTreeReverse(action);
                if (obj1 != null)
                    action(obj1);
            }
        }
    }
}