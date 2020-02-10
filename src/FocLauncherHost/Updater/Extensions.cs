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

        public static bool IsSuccess(this InstallResult result)
        {
            return result == InstallResult.Success || result == InstallResult.SuccessRestartRequired;
        }

        public static bool IsFailure(this InstallResult result)
        {
            return result == InstallResult.Failure || result == InstallResult.FailureException;
        }
    }
}
