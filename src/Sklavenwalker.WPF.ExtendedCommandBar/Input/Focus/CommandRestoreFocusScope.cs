using System;
using System.Windows;
using System.Windows.Input;

namespace Sklavenwalker.Wpf.CommandBar.Input.Focus;

internal class CommandRestoreFocusScope : RestoreFocusScope
{
    public CommandRestoreFocusScope(IInputElement restoreFocus) : base(restoreFocus, IntPtr.Zero)
    {
    }

    public override void PerformRestoration()
    {
        base.PerformRestoration();
        if (RestoreFocus != null)
            return;
        Keyboard.ClearFocus();
    }

    protected override bool IsRestorationTargetValid()
    {
        return true;
    }
}