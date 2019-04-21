using System;
using System.IO;
using System.Linq;
using FocLauncher.Mods;

namespace FocLauncher.Game
{
    public abstract class AbstractFocGame : IGame
    {
        public string GameDirectory { get; protected set; }

        public abstract string Name { get; }

        public GameProcessData GameProcessData { get; }

        protected abstract string GameExeFileName { get; }

        protected abstract int DefaultXmlFileCount { get; }

        protected AbstractFocGame()
        {
        }

        protected AbstractFocGame(string gameDirectory)
        {
            GameDirectory = gameDirectory;
            if (!Exists())
                throw new Exception("FoC does not exists");
            GameProcessData = new GameProcessData();
        }

        public bool Exists() => File.Exists(Path.Combine(GameDirectory, GameExeFileName));

        public abstract void PlayGame();

        public abstract void PlayGame(IMod mod, DebugOptions debugOptions);

        public abstract bool IsPatched();

        public bool IsGameAiClear()
        {
            if (Directory.Exists(Path.Combine(GameDirectory, @"Data\Scripts\")))
                return false;
            var xmlDir = Path.Combine(GameDirectory, @"Data\XML\");
            if (!Directory.Exists(xmlDir))
                return false;
            var number = Directory.EnumerateFiles(xmlDir).Count();
            if (number != DefaultXmlFileCount)
                return false;
            if (Directory.Exists(Path.Combine(xmlDir, @"AI\")))
                return false;
            return true;
        }
    }
}