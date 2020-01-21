using System.Collections.Generic;
using System.ComponentModel;
using FocLauncher.Game;
using FocLauncher.Mods;

namespace FocLauncher
{
    public interface IDataModel : INotifyPropertyChanged
    {
         IGame EaW { get; set; }

         IGame FoC { get; set; }

         GameType FocGameType { get; set; }

         IEnumerable<IMod> Mods { get; set; }

         IMod SelectedMod { get; set; }

         bool UseDebugBuild { get; set; }

         bool IgnoreAsserts { get; set; }

         bool NoArtProcess { get; set; }
         
    }
}