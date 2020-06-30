using FocLauncher.Versioning;

namespace FocLauncher.Game
{
    // TODO: Rename as file name
    public interface IPetroglyhGameableObject
    {
        string Name { get; }

        string Description { get; }

        string? IconFile { get; }

        ModVersion? Version { get; }
    }
}
