using System;
using AnakinRaW.ProductMetadata.Component;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace AnakinRaW.ProductMetadata.Services.Detectors;

internal abstract class ComponentDetectorBase<T> : IComponentDetector where T : IInstallableComponent
{
    protected IServiceProvider ServiceProvider { get; }

    protected ILogger? Logger { get; }

    protected ComponentDetectorBase(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        ServiceProvider = serviceProvider;
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }
        
    public bool GetCurrentInstalledState(IInstallableComponent installableComponent, VariableCollection productVariables)
    {
        Requires.NotNull(installableComponent, nameof(installableComponent));
        Requires.NotNull(productVariables, nameof(productVariables));
        var component = ValidateSupported(installableComponent);
        return FindCore(component, productVariables);
    }

    protected abstract bool FindCore(T component, VariableCollection product);

    protected T ValidateSupported(IInstallableComponent component)
    {
        if (component is not T obj)
            throw new InvalidOperationException($"Component {component.GetType().Name} is of wrong type. " +
                                                $"Expected {nameof(SingleFileComponent)}");
        return obj;
    }

    public override string ToString()
    {
        return $"Detector: {GetType().Name}";
    }
}