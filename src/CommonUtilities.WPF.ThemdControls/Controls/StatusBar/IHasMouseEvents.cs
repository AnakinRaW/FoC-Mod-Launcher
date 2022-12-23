using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public interface IHasMouseEvents
{
    void Click(Point point);

    void MouseMovePosition(Point point);

    void MouseLeaveControl(Point point);
}