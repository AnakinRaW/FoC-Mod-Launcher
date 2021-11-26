using System;
using System.Windows;
using System.Windows.Media;

namespace FocLauncher.Utilities
{
    internal static class VisualUtilities
    {
        public static DependencyObject? GetVisualOrLogicalParent(this DependencyObject? sourceElement)
        {
            if (sourceElement == null)
                return null;
            return sourceElement is Visual ? VisualTreeHelper.GetParent(sourceElement) ?? LogicalTreeHelper.GetParent(sourceElement) : LogicalTreeHelper.GetParent(sourceElement);
        }

        public static object FindAncestorOrSelf<TElementType>(this TElementType obj, Func<TElementType, TElementType> parentEvaluator, Func<TElementType, bool> ancestorSelector)
        {
            return ancestorSelector(obj) ? obj : obj.FindAncestor(parentEvaluator, ancestorSelector);
        }

        public static TAncestorType FindAncestor<TAncestorType>(this Visual obj) where TAncestorType : DependencyObject
        {
            return obj.FindAncestor<TAncestorType, DependencyObject>(GetVisualOrLogicalParent);
        }

        public static TAncestorType FindAncestor<TAncestorType, TElementType>(
          this TElementType obj,
          Func<TElementType, TElementType> parentEvaluator)
          where TAncestorType : class
        {
            return obj.FindAncestor(parentEvaluator, ancestor => ancestor is TAncestorType) as TAncestorType;
        }

        public static object? FindAncestor<TElementType>(this TElementType obj, Func<TElementType, TElementType> parentEvaluator, Func<TElementType, bool> ancestorSelector)
        {
            for (var elementType = parentEvaluator(obj); elementType != null; elementType = parentEvaluator(elementType))
            {
                if (ancestorSelector(elementType))
                    return elementType;
            }
            return null;
        }
    }
}
