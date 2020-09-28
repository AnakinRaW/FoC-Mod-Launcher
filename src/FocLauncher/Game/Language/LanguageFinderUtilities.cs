using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using EawModinfo.Spec;

namespace FocLauncher.Game.Language
{
    public static class LanguageFinderUtilities
    {
        private static readonly IList<CultureInfo> AvailableCultures = CultureInfo.GetCultures(CultureTypes.AllCultures).ToList();

        public static IEnumerable<ILanguageInfo> GetTextFileLanguages(string directory)
        {
            var languages = GetTextFileLanguageNames(directory).ToList();
            if (!languages.Any())
                yield break;

            foreach (var language in languages)
            {
                if (!LanguageFinderUtilities.TryGetLanguageInfoFromEnglishName(language, LanguageSupportLevel.Text, out var languageInfo))
                    continue;
                yield return languageInfo!;
            }
        }
        
        public static bool TryGetLanguageInfoFromEnglishName(string englishName, LanguageSupportLevel supportLevel, out ILanguageInfo? languageInfo)
        {
            var culture = AvailableCultures.FirstOrDefault(x =>
                x.EnglishName.Equals(englishName, StringComparison.InvariantCultureIgnoreCase));
            languageInfo = default;
            if (culture == null)
                return false;
            languageInfo = new LanguageInfo(culture.TwoLetterISOLanguageName) {Support = supportLevel};
            return true;
        }

        internal static IEnumerable<string> GetTextFileLanguageNames(string directory)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                yield break;

            var languageFiles = Directory.EnumerateFiles(directory, "MasterTextFile_*.dat").ToList();

            if (languageFiles.Count() < 0)
                yield break;

            foreach (var file in languageFiles)
            {
                var langName = ExtractLanguageName(Path.GetFileNameWithoutExtension(file));
                if (string.IsNullOrEmpty(langName))
                    continue;
                yield return langName;
            }
        }

        private static string ExtractLanguageName(string file)
        {
            var start = file.IndexOf('_');
            if (start < 0 || start + 1 == file.Length)
                return string.Empty;

            return file.Substring(start + 1);
        }
    }
}