using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FocLauncher.Annotations;
using FocLauncher.Game;
using FocLauncher.Utilities;
using FocLauncher.Versioning;

namespace FocLauncher.Mods
{
    public class Mod : IMod
    {
        private string _name;
        public string FolderName { get; }
        public string ModDirectory { get; }

        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public ModVersion Version
        {
            get
            {
                try
                {
                    var node = XmlTools.GetNodeValue(ModDirectory + @"\Data\XML\Gameobjectfiles.xml",
                        "/Game_Object_Files/Version");
                    return string.IsNullOrEmpty(node) ? null : ModVersion.Parse(node);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        public bool WorkshopMod { get; }
        public string IconFile { get; }

        public Mod(string modDirectory, bool workshopMod = false)
        {
            ModDirectory = modDirectory;
            WorkshopMod = workshopMod;
            FolderName = new DirectoryInfo(ModDirectory).Name;

            Task.Run(() => GetName(FolderName, workshopMod)).ContinueWith(t => Name = t.Result);

            //Name = GetName(FolderName, workshopMod).Result;
            var icon = Directory.EnumerateFiles(ModDirectory, "*.ico");
            IconFile = icon.FirstOrDefault();
        }

        private async Task<string> GetName(string folderName, bool workshop)
        {
            if (!workshop)
                return folderName.Replace('_', ' ');

            if (KnownModIds.TryFind(folderName, out var modName))
                return modName;
            
            var doc = await HtmlDownloader.GetSteamModPageDocument(FolderName);
            return new WorkshopNameResolver().GetName(doc, FolderName);            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
