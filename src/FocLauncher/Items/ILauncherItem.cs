using System.ComponentModel;
using FocLauncher.Game;

namespace FocLauncher.Items
{
    public interface ILauncherItem : INotifyPropertyChanged
    {
        IPetroglyhGameableObject GameObject { get; }

        string Text { get; }
    }
}