using System;
using System.Diagnostics;
using System.IO;
using FocLauncher.Mods;

namespace FocLauncher.Game
{
    public abstract class PetroglyphGame : IGame
    {
        public event EventHandler<Process> GameStarted;


        public event EventHandler GameStarting;


        public event EventHandler GameClosed;

        public abstract GameType Type { get; }
        public string GameDirectory { get; protected set; }

        public abstract string Name { get; }

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
            PlayGame(null, default);
        }

        public void PlayGame(IMod mod, GameRunArguments args)
        {
            if (!Exists())
                throw new Exception("Game was not found");
            OnGameStarting(mod, ref args);
            var startInfo = CreateGameProcess(args);
            var process = StartGameProcess(startInfo, mod);
            if (process != null)
                OnGameStarted(process);
        }

        public abstract bool IsPatched();

        public abstract bool IsGameAiClear();

        public virtual bool HasDebugBuild()
        {
            return false;
        }

        public virtual bool IsLanguageInstalled(string language)
        {
            throw new NotImplementedException();
        }

        protected virtual void OnGameStarting(IMod mod, ref GameRunArguments args)
        {
            GameStarting?.Invoke(this, EventArgs.Empty);
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
}