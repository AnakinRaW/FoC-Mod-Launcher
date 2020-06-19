using System;
using System.Windows;
using System.Windows.Media;
using FocLauncher.NativeMethods;

namespace FocLauncher.Utilities
{
    internal static class VisualUtilities
    {
        public static DependencyObject GetVisualOrLogicalParent(this DependencyObject sourceElement)
        {
            if (sourceElement == null)
                return null;
            return sourceElement is Visual ? VisualTreeHelper.GetParent(sourceElement) ?? LogicalTreeHelper.GetParent(sourceElement) : LogicalTreeHelper.GetParent(sourceElement);
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

        public static object FindAncestor<TElementType>(this TElementType obj, Func<TElementType, TElementType> parentEvaluator, Func<TElementType, bool> ancestorSelector)
        {
            for (var elementType = parentEvaluator(obj); elementType != null; elementType = parentEvaluator(elementType))
            {
                if (ancestorSelector(elementType))
                    return elementType;
            }
            return null;
        }

        public static void HitTestVisibleElements(Visual visual, HitTestResultCallback resultCallback,
            HitTestParameters parameters)
        {
            VisualTreeHelper.HitTest(visual, ExcludeNonVisualElements, resultCallback, parameters);
        }

        private static HitTestFilterBehavior ExcludeNonVisualElements(DependencyObject potentialHitTestTarget)
        {
            if (!(potentialHitTestTarget is Visual))
                return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
            return !(potentialHitTestTarget is UIElement uiElement) || uiElement.IsVisible && uiElement.IsEnabled
                ? HitTestFilterBehavior.Continue
                : HitTestFilterBehavior.ContinueSkipSelfAndChildren;
        }

        internal static bool ModifyStyle(IntPtr hWnd, int styleToRemove, int styleToAdd)
        {
            var windowLong = User32.GetWindowLong(hWnd, -16);
            var dwNewLong = windowLong & ~styleToRemove | styleToAdd;
            if (dwNewLong == windowLong)
                return false;
            User32.SetWindowLong(hWnd, -16, dwNewLong);
            return true;
        }

    }
}
