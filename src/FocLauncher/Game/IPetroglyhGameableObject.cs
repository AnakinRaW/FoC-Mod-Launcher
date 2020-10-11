using System.Collections.Generic;
using EawModinfo.Spec;

namespace FocLauncher.Game
{
    public interface IPetroglyhGameableObject
    {
        string Name { get; }

        string Description { get; }

        string? IconFile { get; }

        string Version { get; }

        ICollection<ILanguageInfo> InstalledLanguages { get; }
    }
}
