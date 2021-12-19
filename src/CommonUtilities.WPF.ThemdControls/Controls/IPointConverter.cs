using System.Threading.Tasks;
using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public interface IPointConverter
{
    Task<Point> PointToScreenCoordinatesAsync(Point point);
}