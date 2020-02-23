using System.Threading;

namespace FocLauncherHost.Updater
{
    internal interface IUpdaterOperation
    {
        bool Plan();

        void Run(CancellationToken token = default);
    }
}