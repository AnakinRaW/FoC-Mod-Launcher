using System;
using System.Globalization;
using System.IO;
using EawModinfo.Spec;
using FocLauncher.Game;

namespace FocLauncher.Utilities
{
    internal static class ModInfoExtension
    {
        public static string GetAbsolutePath(this IModReference modReference, IGame game)
        {
            if (modReference is null)
                throw new ArgumentNullException(nameof(modReference));
            if (string.IsNullOrEmpty(modReference.Identifier))
                throw new InvalidOperationException("location must not be null or empty");
            if (modReference.Type == ModType.Virtual || modReference.Type == ModType.Workshops)
                throw new InvalidOperationException("ModReference must be a ModType of 'Default'");
            if (Path.IsPathRooted(modReference.Identifier))
                return modReference.Identifier;
            if (game is null)
                throw new ArgumentNullException(nameof(game));
            return Path.Combine(game.Directory.FullName, modReference.Identifier);
        }

        public static string GetLanguageEnglishName(this ILanguageInfo languageInfo)
        {
            if (languageInfo is EawModinfo.Model.LanguageInfo lang)
                return lang.Culture.EnglishName;
            return new CultureInfo(languageInfo.Code).EnglishName;
        }
    }
}
