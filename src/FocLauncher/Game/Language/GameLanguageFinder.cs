using System.Collections.Generic;
using System.IO;
using System.Linq;
using EawModinfo.Spec;

namespace FocLauncher.Game.Language
{
    public class GameLanguageFinder : LanguageFinderBase
    {
        public GameLanguageFinder(DirectoryInfo baseDirectory) : base(baseDirectory)
        {
        }

        protected override IEnumerable<ILanguageInfo> FindTextLanguages()
        {
            var textDirectory = Path.Combine(BaseDirectory.FullName, "Data\\Text");
            if (!Directory.Exists(textDirectory))
                return Enumerable.Empty<ILanguageInfo>();

            return Enumerable.Empty<ILanguageInfo>();
        }

        protected override IEnumerable<ILanguageInfo> FindSpeechLanguages()
        {
            return Enumerable.Empty<ILanguageInfo>();
        }

        protected override IEnumerable<ILanguageInfo> FindSfxLanguages()
        {
            return Enumerable.Empty<ILanguageInfo>();
        }
    }
}