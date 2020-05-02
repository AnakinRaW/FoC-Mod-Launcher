using FocLauncher.Controls;
using FocLauncher.Game;

namespace FocLauncher
{
    public class LauncherListItemModel : IHasInvocationController
    {
        public IPetroglyhGameableObject GameObject { get; }

        public IInvocationController InvocationController { get; }

        public LauncherListItemModel(IPetroglyhGameableObject gameObject, IInvocationController controller)
        {
            GameObject = gameObject;
            InvocationController = controller;
        }
    }
}