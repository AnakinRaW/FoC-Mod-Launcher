using System.Text;
using FocLauncher.Mods;

namespace FocLauncher.Game
{
    public class GameRunArguments
    {
        public bool UseDebug { get; set; }

        public bool IgnoreAsserts { get; set; }

        public bool NoArtProcess { get; set; }

        public bool Windowed { get; set; }

        public IMod? Mod { get; }
        
        public GameRunArguments()
        {
        }

        public GameRunArguments(IMod mod)
        {
            Mod = mod;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder();

            if (Mod != null && !string.IsNullOrEmpty(Mod.FolderName))
            {
                var modArg = Mod.WorkshopMod ? $"STEAMMOD={Mod.FolderName}" : $"MODPATH=Mods/{Mod.FolderName}";
                sb.Append(modArg);
                sb.Append(" ");
            }
            
            if (IgnoreAsserts)
                sb.Append("IGNOREASSERTS ");
            if (NoArtProcess)
                sb.Append("NOARTPROCESS ");

            return sb.ToString().TrimEnd(' ');
        }
    }
}