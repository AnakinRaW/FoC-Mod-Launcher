using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FocLauncher.Versioning;

namespace FocLauncher.Game
{
    public abstract class PetroglyphGame : IGame
    {
        public event EventHandler<Process> GameStarted;


        public event EventHandler<GameStartingEventArgs> GameStarting;


        public event EventHandler GameClosed;

        public abstract GameType Type { get; }
        public string GameDirectory { get; }

        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual string? IconFile => string.Empty;
        public virtual ModVersion? Version => null;

        public GameProcessWatcher GameProcessWatcher { get; } = new GameProcessWatcher();

        protected abstract int DefaultXmlFileCount { get; }

        protected abstract string GameExeFileName { get; }


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

        public bool PlayGame(string? iconFile = null)
        {
            return PlayGame(new GameCommandArguments());
        }

        public bool PlayGame(GameCommandArguments args, string? iconFile = null)
        {
            var exeFile = new FileInfo(Path.Combine(GameDirectory, GameExeFileName));
            return StartGame(args, new GameStartInfo(exeFile, GameBuildType.Release), iconFile);
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
        
        public virtual bool IsLanguageInstalled(string language)
        {
            throw new NotImplementedException();
        }

        protected bool StartGame(GameCommandArguments args, GameStartInfo gameStartInfo, string? iconFile = null)
        {
            if (!Exists())
                throw new Exception("Game was not found");
            var startingArguments = new GameStartingEventArgs(args, gameStartInfo.BuildType);
            OnGameStarting(startingArguments);
            if (startingArguments.Cancel)
                return false;
            var processStartInfo = CreateGameProcess(args, gameStartInfo.Executable);

            Process process;
            try
            {
                process = StartGameProcess(processStartInfo, iconFile);
            }
            catch
            {
                return false;
            }

            if (process != null)
                OnGameStarted(process);
            return true;
        }
        
        protected virtual void OnGameStarting(GameStartingEventArgs args)
        {
            GameStarting?.Invoke(this, args);
        }

        private Process StartGameProcess(ProcessStartInfo startInfo, string? iconFile)
        {
            return GameStartHelper.StartGameProcess(startInfo, iconFile);
        }

        private ProcessStartInfo CreateGameProcess(GameCommandArguments options, FileInfo executable)
        {
            var startInfo = new ProcessStartInfo(executable.FullName)
            {
                Arguments = options.ToArgs(),
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

        protected class GameStartInfo
        {
            public GameBuildType BuildType { get; }

            public FileInfo Executable { get; }

            public GameStartInfo(FileInfo executable, GameBuildType buildType)
            {
                Executable = executable;
                BuildType = buildType;
            }
        }
    }
}