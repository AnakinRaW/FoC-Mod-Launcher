using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FocLauncher.Mods;
using FocLauncher.Versioning;

namespace FocLauncher.Game
{
    public abstract class PetroglyphGame : IGame
    {
        public event EventHandler<Process> GameStarted;


        public event EventHandler<GameStartingEventArgs> GameStarting;


        public event EventHandler GameClosed;

        public abstract GameType Type { get; }
        public string GameDirectory { get; protected set; }

        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual string? IconFile => string.Empty;
        public virtual ModVersion? Version => null;

        public GameProcessWatcher GameProcessWatcher { get; } = new GameProcessWatcher();

        protected abstract int DefaultXmlFileCount { get; }

        protected abstract string GameExeFileName { get; }

        protected abstract string? DebugGameExeFileName { get; }

        // TODO: Change to DirectoryInfo
        protected PetroglyphGame(string gameDirectory)
        {
            GameDirectory = gameDirectory;
            if (!ExistsGameDirectoryAndGameExecutable())
                throw new Exception($"{GetType().Name} does not exists");

            GameProcessWatcher.ProcessExited += OnGameProcessExited;
        }

        public virtual bool Exists()
        {
            return ExistsGameDirectoryAndGameExecutable();
        }

        public void PlayGame()
        {
            PlayGame(new GameRunArguments());
        }

        public bool PlayGame(GameRunArguments args)
        {
            if (!Exists())
                throw new Exception("Game was not found");
            var startingArguments = new GameStartingEventArgs(args);
            OnGameStarting(startingArguments);
            if (startingArguments.Cancel)
                return false;
            var startInfo = CreateGameProcess(args);
            var process = StartGameProcess(startInfo, args.Mod);
            if (process != null)
                OnGameStarted(process);
            return true;
        }

        public abstract bool IsPatched();

        public virtual bool IsGameAiClear()
        {
            if (Directory.Exists(Path.Combine(GameDirectory, @"Data\Scripts\")))
                return false;
            var xmlDir = Path.Combine(GameDirectory, @"Data\XML\");
            if (!Directory.Exists(xmlDir))
                return false;
            var number = Directory.EnumerateFiles(xmlDir).Count();
            if (number != DefaultXmlFileCount)
                return false;
            return !Directory.Exists(Path.Combine(xmlDir, @"AI\"));
        }

        public virtual bool HasDebugBuild()
        {
            return false;
        }

        public virtual bool IsLanguageInstalled(string language)
        {
            throw new NotImplementedException();
        }
        
        protected virtual void OnGameStarting(GameStartingEventArgs args)
        {
            GameStarting?.Invoke(this, args);
        }

        private Process StartGameProcess(ProcessStartInfo startInfo, IMod mod)
        {
            try
            {
                return GameStartHelper.StartGameProcess(startInfo, mod.IconFile);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private ProcessStartInfo CreateGameProcess(GameRunArguments args)
        {
            var exePath = Path.Combine(GameDirectory, args.UseDebug ? DebugGameExeFileName : GameExeFileName);

            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = args.ToString(),
                WorkingDirectory = GameDirectory,
                UseShellExecute = false
            };
            return startInfo;
        }

        private bool ExistsGameDirectoryAndGameExecutable()
        {
            return Directory.Exists(GameDirectory) && File.Exists(Path.Combine(GameDirectory, GameExeFileName));
        }

        private protected virtual void OnGameProcessExited(object sender, EventArgs e)
        {
            GameClosed?.Invoke(this, EventArgs.Empty);
        }

        private protected virtual void OnGameStarted(Process process)
        {
            GameProcessWatcher.SetProcess(process);
            GameStarted?.Invoke(this, process);
        }
    }

    public class GameStartingEventArgs : CancelEventArgs
    {
        public GameRunArguments GameArguments { get; }

        public GameStartingEventArgs(GameRunArguments arguments)
        {
            GameArguments = arguments;
        }
    }
}