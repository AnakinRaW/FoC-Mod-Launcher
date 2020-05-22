﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FocLauncher.ModInfo
{
    public sealed class ModInfoFile
    {
        private DateTime? _lastWriteTime;
        private ModInfoData? _data;

        public bool IsValid
        {
            get
            {
                try
                {
                    Validate();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public FileInfo File { get; }

        public ModInfoFile(FileInfo modInfoFile)
        {
            if (modInfoFile is null)
                throw new ArgumentNullException(nameof(modInfoFile));
            ModFileDataUtilities.CheckModInfoFile(modInfoFile);
            File = modInfoFile;
            _lastWriteTime = modInfoFile.LastWriteTime;
        }

        public static bool Find(DirectoryInfo directory, out ModInfoFile? modInfoFile)
        {
            if (directory is null)
                throw new ArgumentNullException(nameof(directory));
            modInfoFile = default;

            var file = directory.GetFiles().FirstOrDefault(x => x.Name.ToLower().Equals("modinfo.json"));
            if (file is null)
                return false;
            modInfoFile = new ModInfoFile(file);
            return true;
        }

        public void Validate()
        {
            ModFileDataUtilities.CheckModInfoFile(File);
        }

        public void Invalidate()
        {
            ModFileDataUtilities.CheckModInfoFile(File);
            _lastWriteTime = null;
        }

        public async Task<ModInfoData> GetModInfoAsync()
        {
            ModFileDataUtilities.CheckModInfoFile(File);
            if (_data != null && _lastWriteTime.HasValue && _lastWriteTime.Value.Equals(File.LastWriteTime))
                return _data;
            _data = await ModFileDataUtilities.ParseAsync(File);
            return _data;
        }

        public ModInfoData GetModInfo()
        {
            ModFileDataUtilities.CheckModInfoFile(File);
            if (_data != null && _lastWriteTime.HasValue && _lastWriteTime.Value.Equals(File.LastWriteTime))
                return _data;
            _data = ModFileDataUtilities.Parse(File);
            return _data;
        }

        public bool TryGetModInfo(out ModInfoData modInfo)
        {
            modInfo = null;
            try
            {
                modInfo = GetModInfo();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
