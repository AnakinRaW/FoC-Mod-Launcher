using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EawModinfo.Spec;
using Microsoft;

namespace FocLauncher.Game
{
    public class LanguageFinder
    {
        private readonly IDictionary<string, LanguageInfo> _store = new Dictionary<string, LanguageInfo>();
        private bool _searched;
        private readonly object _syncObject = new object();


        public DirectoryInfo BaseDirectory { get; }

        public LanguageFinder(DirectoryInfo baseDirectory)
        {
            Requires.NotNull(baseDirectory, nameof(baseDirectory));
            if (!baseDirectory.Exists)
                throw new ArgumentException(nameof(baseDirectory), new DirectoryNotFoundException());
            BaseDirectory = baseDirectory;
        }

        public ICollection<ILanguageInfo> Find()
        {
            lock (_syncObject)
            {
                if (!_searched)
                {
                    FindLanguagesCore();
                    _searched = true;
                }
            }
            return _store.Values.OfType<ILanguageInfo>().ToList();
        }

        protected virtual void FindLanguagesCore()
        {
            var textLangs = FindTextLanguages();
            AddToStore(textLangs);
            var speechLangs = FindSpeechLanguages();
            AddToStore(speechLangs);
            var sfxLangs = FindSfxLanguages();
            AddToStore(sfxLangs);
        }

        private IEnumerable<ILanguageInfo> FindTextLanguages()
        {
            var textDirectory = Path.Combine(BaseDirectory.FullName, "Data\\Text");
            if (!Directory.Exists(textDirectory))
                return Enumerable.Empty<ILanguageInfo>();

            return Enumerable.Empty<ILanguageInfo>();
        }

        private IEnumerable<ILanguageInfo> FindSpeechLanguages()
        {
            return Enumerable.Empty<ILanguageInfo>();
        }

        private IEnumerable<ILanguageInfo> FindSfxLanguages()
        {
            return Enumerable.Empty<ILanguageInfo>();
        }

        protected void AddToStore(IEnumerable<ILanguageInfo> newItems)
        {
            foreach (var languageInfo in newItems)
            {
                if (_store.ContainsKey(languageInfo.Code))
                    _store[languageInfo.Code].Support |= languageInfo.Support;
                else
                    _store.Add(languageInfo.Code, new LanguageInfo(languageInfo.Code) {Support = languageInfo.Support});
            }
        }
    }

    internal class LanguageInfo : ILanguageInfo
    {
        public string Code { get; }

        public LanguageSupportLevel Support { get; set; }

        public LanguageInfo(string code)
        {
            Code = code;
        }

        public bool Equals(ILanguageInfo other)
        {
            return Code == other.Code;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ILanguageInfo) obj);
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }
    }
}
