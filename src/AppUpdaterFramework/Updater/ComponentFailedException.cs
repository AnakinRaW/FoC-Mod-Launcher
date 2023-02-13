using System;
using System.Collections.Generic;
using System.Text;
using AnakinRaW.AppUpaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpaterFramework.Updater;

[Serializable]
public class ComponentFailedException : UpdaterException
{
    private readonly IEnumerable<IProductComponent>? _failedComponents;
    private string? _error;

    public override string Message => Error;

    private string Error
    {
        get
        {
            if (_error != null)
                return _error;
            var stringBuilder = new StringBuilder();
            if (_failedComponents != null)
            {
                // TODO: Not sure if IUpdateItem or IProductComponent
                foreach (var component in _failedComponents)
                    stringBuilder.Append("Package '" + component.Id + "' failed to " + component + ";");
            }
            return stringBuilder.ToString().TrimEnd(';');
        }
    }

    public ComponentFailedException(IEnumerable<IProductComponent> failedComponents) : base()
    {
        _failedComponents = failedComponents;
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