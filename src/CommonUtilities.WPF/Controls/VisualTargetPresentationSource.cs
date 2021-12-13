using System;
using System.Security;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Media;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

internal class VisualTargetPresentationSource : PresentationSource, IDisposable
{
    private VisualTarget? _visualTarget;
    
    public override Visual RootVisual
    {
        get => _visualTarget!.RootVisual;
        [SecurityCritical, UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows), 
         UIPermission(SecurityAction.InheritanceDemand, Window = UIPermissionWindow.AllWindows)]
        set
        {
            var rootVisual = _visualTarget!.RootVisual;
            if (rootVisual == value)
                return;
            _visualTarget.RootVisual = value;
            RootChanged(rootVisual, value);
        }
    }

    public override bool IsDisposed => _visualTarget == null;

    public VisualTargetPresentationSource(HostVisual hostVisual)
    {
        _visualTarget = hostVisual != null
            ? new VisualTarget(hostVisual)
            : throw new ArgumentNullException(nameof(hostVisual));
    }

    ~VisualTargetPresentationSource() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected override CompositionTarget GetCompositionTargetCore()
    {
        return _visualTarget!;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
            return;
        using (_visualTarget)
            _visualTarget = null;
    }
}