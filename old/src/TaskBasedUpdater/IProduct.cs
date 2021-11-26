namespace TaskBasedUpdater
{
    public interface IProductInfo
    {
        string Name { get; }

        string Author { get; }

        string AppDataPath { get; }

        string CurrentLocation { get; }
    }
}
