using System;
using System.Collections.Generic;
using System.Linq;
using FocLauncher.Game;
using FocLauncher.ModInfo;
using FocLauncher.Versioning;

namespace FocLauncher.Mods
{
    public abstract class ModBase : IMod
    {
        private ModInfoData? _modInfo;
        private string _name;
        private string _description;
        private string? _iconFile;
        private ModVersion _modVersion;

        public event EventHandler<ModCollectionChangedEventArgs> ModCollectionModified;

        public ModInfoData? ModInfo
        {
            get
            {
                if (_modInfo != null)
                    return _modInfo;
                _modInfo = ResolveModInfo();
                return _modInfo;
            }
        }

        public string Name
        {
            get
            {
                if (_name != null)
                    return _name;
                _name = InitializeName();
                return _name;
            }
        }

        public string Description
        {
            get
            {
                if (_description != null)
                    return _description;
                _description = InitializeDescription();
                return _description;
            }
        }

        public string? IconFile
        {
            get
            {
                if (_iconFile != null)
                    return _iconFile;
                _iconFile = InitializeIcon();
                return _iconFile;
            }
        }

        public ModVersion Version
        {
            get
            {
                if (_modVersion != null)
                    return _modVersion;
                _modVersion = InitializeVersion();
                return _modVersion;
            }
        }
        

        public IReadOnlyCollection<IMod> Mods => ModsInternal.ToList();

        public bool DependenciesResolved { get; private set; }

        public IReadOnlyList<IMod> Dependencies => DependenciesInternal.ToList();

        public bool WorkshopMod => Type == ModType.Workshops;

        public bool Virtual => Type == ModType.Virtual;

        public bool HasDependencies => Dependencies.Count > 0;



        public IGame Game { get; }

        public ModType Type { get; }


        protected internal HashSet<IMod> ModsInternal { get; } = new HashSet<IMod>();

        protected internal List<IMod> DependenciesInternal { get; } = new List<IMod>();


        internal ModBase(IGame game, ModType type)
        {
            Game = game ?? throw new ArgumentNullException(nameof(game));
            Type = type;
        }

        protected ModBase(string name, IGame game, ModType type) : this(game, type)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        protected ModBase(IGame game, ModType type, ModInfoData? modInfoData) : this(game, type)
        {
            if (modInfoData != null)
            {
                try
                {
                    modInfoData.Validate();
                }
                catch (Exception e)
                {
                    throw new PetroglyphModException($"When given a modInfoFile parameter it be valid: {e.Message}", e);
                }
                _modInfo = modInfoData;
            }
        }

        public virtual bool AddMod(IMod mod)
        {
            var result = ModsInternal.Add(mod);
            if (result)
                OnModCollectionModified(new ModCollectionChangedEventArgs(mod, ModCollectionChangedAction.Add));
            return result;
        }

        public virtual bool RemoveMod(IMod mod)
        {
            var result = ModsInternal.Remove(mod);
            if (result)
                OnModCollectionModified(new ModCollectionChangedEventArgs(mod, ModCollectionChangedAction.Remove));
            return result;
        }

        public bool ResolveDependencies()
        {
            var result = ResolveDependenciesCore();
            DependenciesResolved = true;
            return result;
        }

        public abstract string ToArgs(bool includeDependencies);

        protected abstract bool ResolveDependenciesCore();

        protected virtual ModInfoData? ResolveModInfo()
        {
            return null;
        }

        protected virtual string InitializeName()
        {
            var name = string.Empty;
            if (ModInfo != null)
                name = ModInfo.Name;
            return name;
        }

        protected virtual string InitializeDescription()
        {
            var name = string.Empty;
            if (ModInfo != null)
                name = ModInfo.Description;
            return name;
        }

        protected virtual ModVersion? InitializeVersion()
        {
            return ModInfo?.Version;
        }

        protected virtual string InitializeIcon()
        {
            var name = string.Empty;
            if (ModInfo != null)
                name = ModInfo.Icon;
            return name;
        }


        protected virtual void OnModCollectionModified(ModCollectionChangedEventArgs e)
        {
            ModCollectionModified?.Invoke(this, e);
        }




        //public ModInfoData? ModInfo
        //{
        //    get
        //    {
        //        if (_modInfo != null || _modInfoFileLookupFlag || ModInfoFile is null)
        //            return _modInfo;
        //        _modInfoFileLookupFlag = true;
        //        ModInfoFile.TryGetModInfo(out var modInfo);
        //        _modInfo = modInfo;
        //        return _modInfo;
        //    }
        //}

        //public ModInfoFile? ModInfoFile { get; }

        //protected ModBase(IGame game, ModType type, ModInfoFile? modInfoFile) : this(game, type)
        //{
        //    if (modInfoFile != null)
        //    {
        //        try
        //        {
        //            modInfoFile.Validate();
        //        }
        //        catch (Exception e)
        //        {
        //            throw new PetroglyphModException(e.Message, e);
        //        }
        //        ModInfoFile = modInfoFile;
        //    }
        //}
    }
}