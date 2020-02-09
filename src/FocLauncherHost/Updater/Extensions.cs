using System;
using System.Linq;

namespace FocLauncherHost.Updater
{
    internal static class Extensions
    {
        public static bool IsExceptionType<T>(this Exception error) where T : Exception
        {
            switch (error)
            {
                case T _:
                    return true;
                case AggregateException aggregateException:
                    return aggregateException.InnerExceptions.Any(p => p.IsExceptionType<T>());
                default:
                    return false;
            }
        }
    }
}
