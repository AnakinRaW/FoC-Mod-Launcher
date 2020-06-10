using System.Threading.Tasks;

namespace FocLauncher.Threading
{
    public static class TaskExtensions
    {
        public static async void ForgetButThrow(this Task task)
        {
            await task;
        }
    }
}