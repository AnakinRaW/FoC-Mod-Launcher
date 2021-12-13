using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Sklavenwalker.CommonUtilities.Wpf.Utils;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public abstract class WorkerThreadElementContainer : VisualWrapper
{
    private UIElement? _containedElement;
    private readonly object _syncObject = new();
    private Dispatcher? _backgroundDispatcher;
    private Dictionary<DependencyProperty, object>? _propertyCache;
    private VisualTargetPresentationSource? _backgroundPresentationSource;
    private bool _initializedOnce;
    private Size _measuredSize = Size.Empty;
    private Size _arrangedSize = Size.Empty;

    protected virtual string? DispatcherGroup => null;

    protected virtual int StackSize => 0;

    protected WorkerThreadElementContainer()
    {
        Child = new HostVisual();
        PresentationSource.AddSourceChangedHandler(this, OnPresentationSourceChanged);
    }

    protected abstract UIElement CreateRootUiElement();

    protected override Size MeasureOverride(Size availableSize)
    {
        lock (_syncObject)
        {
            if (_containedElement == null)
            {
                _measuredSize = availableSize;
                return Size.Empty;
            }
        }
        _containedElement.Dispatcher.BeginInvoke(() => InnerMeasure(availableSize), DispatcherPriority.Render);
        return _containedElement.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        lock (_syncObject)
        {
            if (_containedElement == null)
            {
                _arrangedSize = finalSize;
                return finalSize;
            }
        }
        _containedElement.Dispatcher.BeginInvoke(() => InnerArrange(finalSize), DispatcherPriority.Render);
        return finalSize;
    }

    protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
    {
        return new PointHitTestResult(Child, hitTestParameters.HitPoint);
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (!ShouldForwardPropertyChange(e))
            return;
        ForwardPropertyChange(e.Property, e.NewValue);
    }

    private void ForwardPropertyChange(DependencyProperty dp, object newValue)
    {
        lock (_syncObject)
        {
            if (_containedElement == null)
            {
                _propertyCache ??= new Dictionary<DependencyProperty, object>();
                if (newValue == DependencyProperty.UnsetValue)
                    _propertyCache.Remove(dp);
                else
                    _propertyCache[dp] = newValue;
            }
            else
                _containedElement.Dispatcher.BeginInvoke(() => _containedElement.SetValue(dp, newValue), DispatcherPriority.DataBind);
        }
    }

    private void ForwardCachedProperties(Visual visual)
    {
        if (_propertyCache == null)
            return;
        foreach (var keyValuePair in _propertyCache)
            visual.SetValue(keyValuePair.Key, keyValuePair.Value);
        _propertyCache.Clear();
        _propertyCache = null;
    }


    protected virtual bool ShouldForwardPropertyChange(DependencyPropertyChangedEventArgs e)
    {
        return !e.Property.ReadOnly && e.Property.GetMetadata(typeof(FrameworkElement)) is FrameworkPropertyMetadata { Inherits: true };
    }

    private void OnPresentationSourceChanged(object sender, SourceChangedEventArgs e)
    {
        if (e.OldSource != null && e.NewSource != null)
            return;
        if (e.OldSource != null)
        {
            if (_backgroundDispatcher == null)
                return;
            _backgroundDispatcher.BeginInvoke(new Action(DisconnectHostedVisualFromSourceWorker));
        }
        else
        {
            if (e.NewSource == null)
                return;
            if (!_initializedOnce)
            {
                _initializedOnce = true;
                if (_backgroundDispatcher == null)
                {
                    var dispatcherName = DispatcherGroup;
                    if (string.IsNullOrEmpty(dispatcherName))
                        dispatcherName = nameof(WorkerThreadElementContainer) + Guid.NewGuid();
                    _backgroundDispatcher = BackgroundDispatcher.GetBackgroundDispatcher(dispatcherName, StackSize);
                }
                _backgroundDispatcher.BeginInvoke(new Action(CreateHostedVisualWorker));
            }
            else
                _backgroundDispatcher!.BeginInvoke(new Action(ConnectHostedVisualToSourceWorker));
        }
    }

    private void CreateHostedVisualWorker()
    {
        _backgroundDispatcher!.VerifyAccess();
        var rootUiElement = CreateRootUiElement();
        lock (_syncObject)
        {
            _containedElement = rootUiElement;
            if (!_measuredSize.IsEmpty)
                InnerMeasure(_measuredSize);
            if (!_arrangedSize.IsEmpty)
                InnerArrange(_arrangedSize);
            ForwardCachedProperties(_containedElement);
        }
        ConnectHostedPresentationSource();
    }

    private void ConnectHostedVisualToSourceWorker()
    {
        _backgroundDispatcher!.VerifyAccess();
        ConnectHostedPresentationSource();
    }

    private void ConnectHostedPresentationSource()
    {
        var presentationSource = new VisualTargetPresentationSource((HostVisual)Child!);
        presentationSource.RootVisual = _containedElement!;
        _backgroundPresentationSource = presentationSource;
    }

    private void DisconnectHostedVisualFromSourceWorker()
    {
        _backgroundDispatcher!.VerifyAccess();
        using (_backgroundPresentationSource)
            _backgroundPresentationSource = null;
    }

    private void InnerMeasure(Size availableSize)
    {
        var desiredSize = _containedElement!.DesiredSize;
        _containedElement.Measure(availableSize);
        if (!(desiredSize != _containedElement.DesiredSize))
            return;
        Dispatcher.BeginInvoke(InvalidateMeasure, DispatcherPriority.Render);
    }

    private void InnerArrange(Size finalSize)
    {
        var renderSize = _containedElement!.RenderSize;
        _containedElement.Arrange(new Rect(finalSize));
        if (!(renderSize != _containedElement.RenderSize))
            return;
        Dispatcher.BeginInvoke(InvalidateVisual, DispatcherPriority.Render);
    }
}