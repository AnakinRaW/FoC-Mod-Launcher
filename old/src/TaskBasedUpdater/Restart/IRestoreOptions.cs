namespace TaskBasedUpdater.Restart
{
    public interface IRestoreOptions : IRestartOptions
    {
        bool Restore { get; set; }
    }
}