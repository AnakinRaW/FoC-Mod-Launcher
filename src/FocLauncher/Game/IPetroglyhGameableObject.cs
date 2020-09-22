namespace FocLauncher.Game
{
    public interface IPetroglyhGameableObject
    {
        string Name { get; }

        string Description { get; }

        string? IconFile { get; }

        string Version { get; }
    }
}
