using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using FocLauncher.Annotations;
using FocLauncher.Game;
using FocLauncher.Mods;
using FocLauncher.Properties;

namespace FocLauncher
{
    public class LauncherDataModel : IDataModel
    {
        public static string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"FoC Launcher\");
        public static string IconPath = Path.Combine(AppDataPath, "foc.ico");

        private IGame _eaW;
        private IGame _foC;
        private GameType _focGameType;
        private IEnumerable<IMod> _mods;
        private IMod _selectedMod;
        private bool _useDebugBuild;
        private bool _ignoreAsserts = true;
        private bool _noArtProcess = true;
        public event PropertyChangedEventHandler PropertyChanged;

        public LauncherDataModel()
        {
            InitAppDataDirectory();
            if (!InitGames(out _))
            {
                MessageBox.Show("Could not find Forces of Corruption", "FoC Launcher", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            SearchMods();
        }

        public IGame EaW
        {
            get => _eaW;
            set
            {
                if (Equals(value, _eaW))
                    return;
                _eaW = value;
                OnPropertyChanged();
            }
        }

        public IGame FoC
        {
            get => _foC;
            set
            {
                if (Equals(value, _foC))
                    return;
                _foC = value;
                OnPropertyChanged();
            }
        }

        public GameType FocGameType
        {
            get => _focGameType;
            set
            {
                if (value == _focGameType)
                    return;
                _focGameType = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<IMod> Mods
        {
            get => _mods;
            set
            {
                if (Equals(value, _mods)) return;
                _mods = value;
                OnPropertyChanged();
            }
        }

        public IMod SelectedMod
        {
            get => _selectedMod;
            set
            {
                if (Equals(value, _selectedMod)) return;
                _selectedMod = value;
                OnPropertyChanged();
            }
        }

        public bool UseDebugBuild
        {
            get => _useDebugBuild;
            set
            {
                if (value == _useDebugBuild) return;
                _useDebugBuild = value;
                OnPropertyChanged();
            }
        }

        public bool IgnoreAsserts
        {
            get => _ignoreAsserts;
            set
            {
                if (value == _ignoreAsserts)
                    return;
                _ignoreAsserts = value;
                OnPropertyChanged();
            }
        }

        public bool NoArtProcess
        {
            get => _noArtProcess;
            set
            {
                if (value == _noArtProcess) return;
                _noArtProcess = value;
                OnPropertyChanged();
            }
        }

        private bool InitGames(out GameDetectionResult result)
        {
            result = GameHelper.GetGameInstallations();

            if (result.IsError)
                return false;

            try
            {
                EaW = new Eaw(result.EawPath);
            }
            catch
            {
                return false;
            }

            switch (result.FocType)
            {
                case GameType.Disk:
                    FoC = new Foc(result.FocPath);
                    break;
                case GameType.SteamGold:
                    FoC = new SteamGame(result.FocPath);
                    break;
                case GameType.GoG:
                    FoC = new Foc(result.FocPath);
                    break;
            }

            FocGameType = result.FocType;
            return true;
        }

        private void SearchMods()
        {
            Mods = ModHelper.FindMods(FoC);
            SelectedMod = Mods.FirstOrDefault();
        }

        private void InitAppDataDirectory()
        {
            if (!Directory.Exists(AppDataPath))
                Directory.CreateDirectory(AppDataPath);

            using (var fs = new FileStream(IconPath, FileMode.Create, FileAccess.Write))
            {
                Resources.foc.Save(fs);
            }
        }


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}