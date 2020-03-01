using System.Threading;

namespace TaskBasedUpdater
{
    internal interface IUpdaterOperation
    {
        bool Plan();

        void Run(CancellationToken token = default);
    }
}