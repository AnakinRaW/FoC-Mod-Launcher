using System;
using System.Collections.Generic;
using System.Text;
using AnakinRaW.AppUpdaterFramework.Updater.Tasks;

namespace AnakinRaW.AppUpdaterFramework.Updater;

[Serializable]
public class ComponentFailedException : UpdaterException
{
    private readonly IEnumerable<IProgressTask>? _failedComponentTasks;
    private string? _error;

    public override string Message => Error;

    private string Error
    {
        get
        {
            if (_error != null)
                return _error;
            var stringBuilder = new StringBuilder();
            if (_failedComponentTasks != null)
            {
                // TODO: Not sure if IUpdateItem or IProductComponent
                foreach (var task in _failedComponentTasks)
                    stringBuilder.Append("Package '" + task.Component.Id + "' failed to " + task.Type.ToString("G") + ";");
            }
            return stringBuilder.ToString().TrimEnd(';');
        }
    }

    internal ComponentFailedException(IEnumerable<IProgressTask> failedComponentTasks)
    {
        _failedComponentTasks = failedComponentTasks;
        HResult = 1603;
    }

    internal ComponentFailedException(string error, int errorCode = 1603)
    {
        if (string.IsNullOrEmpty(error))
            throw new ArgumentNullException(nameof(error));
        _error = error;
        HResult = errorCode;
    }
}