using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnakinRaW.AppUpaterFramework.Metadata.Component;
using AnakinRaW.AppUpaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpaterFramework.Updater.Tasks;

namespace AnakinRaW.AppUpaterFramework.Utilities;

internal static class Extensions
{
    internal static IEnumerable<IInstallableComponent> GetInstallableComponents(this IProductCatalog<IProductComponent> catalog)
    {
        if (!catalog.Items.Any())
            return Enumerable.Empty<IInstallableComponent>();
        var result = new List<IInstallableComponent>();
        foreach (var item in catalog.Items)
        {
            if (item is IInstallableComponent installable)
                result.Add(installable);
        }
        return result;
    }

    internal static void RaiseAsync(this EventHandler? handler, object sender, EventArgs e)
    {
        Task.Run(() => handler?.Invoke(sender, e));
    }

    internal static void RaiseAsync<T>(this EventHandler<T>? handler, object sender, T e)
    {
        Task.Run(() => handler?.Invoke(sender, e));
    }

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

    internal static bool IsOperationCanceledException(this Exception error) =>
        error.IsExceptionType<OperationCanceledException>();

    public static bool IsSuccess(this InstallResult result)
    {
        return result is InstallResult.Success or InstallResult.SuccessRestartRequired;
    }

    public static bool IsFailure(this InstallResult result)
    {
        return result is InstallResult.Failure;
    }

    public static Exception? TryGetWrappedException(this Exception exception)
    {
        var wrappedExceptions = exception.TryGetWrappedExceptions();
        return wrappedExceptions != null && wrappedExceptions.Count == 1 ? wrappedExceptions.Single() : null;
    }

    public static IReadOnlyCollection<Exception>? TryGetWrappedExceptions(this Exception exception)
    {
        return exception is AggregateException aggregateException ? aggregateException.Flatten().InnerExceptions : null;
    }
}