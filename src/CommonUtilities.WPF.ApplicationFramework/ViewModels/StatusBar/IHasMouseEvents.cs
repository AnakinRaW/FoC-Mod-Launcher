using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

public interface IHasMouseEvents
{
    void Click(Point point);

    void MouseMovePosition(Point point);

    void MouseLeaveControl(Point point);
}