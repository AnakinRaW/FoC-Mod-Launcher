using System.Windows;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.StatusBar;

public interface IHasMouseEvents
{
    void Click(Point point);

    void MouseMovePosition(Point point);

    void MouseLeaveControl(Point point);
}