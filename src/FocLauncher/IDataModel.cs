using System.Collections.ObjectModel;
using System.ComponentModel;
using FocLauncher.Game;

namespace FocLauncher
{
    public interface IDataModel : INotifyPropertyChanged
    {
         IGame EaW { get; set; }

         IGame FoC { get; set; }

         GameType FocGameType { get; set; }

         ObservableCollection<IPetroglyhGameableObject> Mods { get; }

         IPetroglyhGameableObject SelectedMod { get; set; }

         bool UseDebugBuild { get; set; }

         bool IgnoreAsserts { get; set; }

         bool NoArtProcess { get; set; }
         
    }
}