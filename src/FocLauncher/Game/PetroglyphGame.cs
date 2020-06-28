using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FocLauncher.ModInfo;
using FocLauncher.Mods;
using FocLauncher.Versioning;

namespace FocLauncher.Game
{
    public abstract class PetroglyphGame : IGame
    {
        public event EventHandler<Process> GameStarted;
        public event EventHandler<GameStartingEventArgs> GameStarting;
        public event EventHandler GameClosed;
        public event EventHandler<ModCollectionChangedEventArgs> ModCollectionModified;



        public DirectoryInfo Directory { get; }

        public GameProcessWatcher GameProcessWatcher { get; } = new GameProcessWatcher();



        public abstract GameType Type { get; }

        public abstract string Name { get; }

        public abstract string Description { get; }

        public virtual string? IconFile => string.Empty;
        public virtual ModVersion? Version => null;

      

        protected abstract int DefaultXmlFileCount { get; }

        protected abstract string GameExeFileName { get; }

        public IReadOnlyCollection<IMod> Mods => ModsInternal.ToList();

        protected internal HashSet<IMod> ModsInternal { get; } = new HashSet<IMod>(ModEqualityComparer.NameAndIdentifier);


        protected PetroglyphGame(DirectoryInfo gameDirectory)
        {
            Directory = gameDirectory;
            if (!ExistsGameDirectoryAndGameExecutable())
                throw new Exception($"{GetType().Name} does not exists");

            GameProcessWatcher.ProcessExited += OnGameProcessExited;
        }

        public virtual bool Exists()
        {
            return ExistsGameDirectoryAndGameExecutable();
        }

        public bool PlayGame(GameCommandArguments? args, string? iconFile = null)
        {
            var exeFile = new FileInfo(Path.Combine(Directory.FullName, GameExeFileName));
            args ??= new GameCommandArguments();
            return StartGame(args, new GameStartInfo(exeFile, GameBuildType.Release), iconFile);
        }

        public abstract bool IsPatched();

        public virtual bool IsGameAiClear()
        {
            if (System.IO.Directory.Exists(Path.Combine(Directory.FullName, @"Data\Scripts\")))
                return false;
            var xmlDir = Path.Combine(Directory.FullName, @"Data\XML\");
            if (!System.IO.Directory.Exists(xmlDir))
                return false;
            var number = Directory.EnumerateFiles(xmlDir).Count();
            if (number != DefaultXmlFileCount)
                return false;
            return !System.IO.Directory.Exists(Path.Combine(xmlDir, @"AI\"));
        }
        
        public virtual bool IsLanguageInstalled(string language)
        {
            throw new NotImplementedException();
        }

        public ICollection<IMod> GetPhysicalMods(bool add)
        {
            var mods = GetPhysicalModsCore();
                mods = mods.Distinct(ModEqualityComparer.NameAndIdentifier).ToList();
            if (add)
            {
                foreach (var mod in mods)
                    AddMod(mod);
            }
            return mods.ToList();
        }

        public IMod? SearchMod(IModReference modReference, ModSearchOptions modSearchOptions, bool add)
        {
            IMod? mod = null;
            try
            {
                if (modSearchOptions.HasFlag(ModSearchOptions.Registered))
                {
                    mod = ModsInternal.FirstOrDefault(x => x.Equals(modReference));
                    if (mod != null)
                        return mod;

                }
                if (modSearchOptions.HasFlag(ModSearchOptions.FileSystem))
                    throw new NotImplementedException();
                return mod;
            }
            finally
            {
                if (mod != null && add)
                    AddMod(mod);
            }
        }

        protected virtual IEnumerable<IMod> GetPhysicalModsCore()
        {
            return SearchDiskMods();
        }

        public virtual IMod CreateMod(ModCreationDelegate modCreation, bool shallAdd)
        {
            throw new NotImplementedException();
        }

        public virtual bool TryCreateMod(ModCreationDelegate modCreation, bool shallAdd, out IMod mod)
        {
            throw new NotImplementedException();
        }

        public virtual void Setup(GameSetupOptions setupMode)
        {
            if (setupMode == GameSetupOptions.NoSetup)
                return;
            GetPhysicalMods(true);
            if (setupMode != GameSetupOptions.ResolveModDependencies)
                return;
            foreach (var mod in Mods)
            {
                if (mod.ExpectedDependencies == 0 || mod.DependenciesResolved)
                    continue;
                mod.ResolveDependencies(ModDependencyResolveStrategy.FromExistingModsRecursive);
            }
        }

        public virtual bool AddMod(IMod mod)
        {
            bool result;
            lock (ModsInternal) 
                result = ModsInternal.Add(mod);

            if (result)
                OnModCollectionModified(new ModCollectionChangedEventArgs(mod, ModCollectionChangedAction.Add));
            return result;
        }

        public virtual bool RemoveMod(IMod mod)
        {
            bool result;
            lock (ModsInternal)
                result = ModsInternal.Remove(mod);
            if (result)
                OnModCollectionModified(new ModCollectionChangedEventArgs(mod, ModCollectionChangedAction.Remove));
            return result;
        }

        protected bool StartGame(GameCommandArguments gameArgs, GameStartInfo gameStartInfo, string? iconFile = null)
        {
            if (!Exists())
                throw new Exception("Game was not found");
            var startingArguments = new GameStartingEventArgs(gameArgs, gameStartInfo.BuildType);
            OnGameStarting(startingArguments);
            if (startingArguments.Cancel)
                return false;

            gameArgs.Validate();
            var processStartInfo = CreateGameProcess(gameArgs.ToArgs(), gameStartInfo.Executable);

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

        protected virtual void OnModCollectionModified(ModCollectionChangedEventArgs e)
        {
            ModCollectionModified?.Invoke(this, e);
        }

        private Process StartGameProcess(ProcessStartInfo startInfo, string? iconFile)
        {
            return GameStartHelper.StartGameProcess(startInfo, iconFile);
        }

        private ProcessStartInfo CreateGameProcess(string arguments, FileSystemInfo executable)
        {
            var startInfo = new ProcessStartInfo(executable.FullName)
            {
                Arguments = arguments,
                WorkingDirectory = Directory.FullName,
                UseShellExecute = false
            };
            return startInfo;
        }

        private bool ExistsGameDirectoryAndGameExecutable()
        {
            return Directory.Exists && File.Exists(Path.Combine(Directory.FullName, GameExeFileName));
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

        protected IEnumerable<IMod> SearchDiskMods()
        {
            var modsDir = Directory.GetDirectories("Mods").FirstOrDefault();
            if (modsDir is null || !modsDir.Exists)
                return Enumerable.Empty<IMod>();
            var modFolders = modsDir.EnumerateDirectories().ToList();
            return modFolders.SelectMany(folder => ModFactory.CreateModAndVariants(this, ModType.Default, folder, true));
        }
    }
}