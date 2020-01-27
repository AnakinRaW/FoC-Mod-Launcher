using System;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using FocLauncher.Threading;

namespace FocLauncherHost
{
    public partial class SplashScreen
    {
        public SplashScreen()
        {
            InitializeComponent();
        }

        public Task HideAnimationAsync()
        {
            var storyboard = FindResource("HideAnimation") as Storyboard;
            System.Threading.Tasks.TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            if (storyboard == null)
                tcs.SetException(new ArgumentNullException());
            else
            {
                EventHandler onComplete = null;
                onComplete = (s, e) => {
                    storyboard.Completed -= onComplete;
                    tcs.SetResult(true);
                };
                storyboard.Completed += onComplete;
                storyboard.Begin(this);
            }
            return tcs.Task;
        }
    }
}
