using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

public abstract class ViewModelBase : ObservableObject, IViewModel, IDisposable
{
    private bool _isDisposed;

    protected IServiceProvider ServiceProvider { get; }
    protected ILogger? Logger { get; }

    protected ViewModelBase(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        ServiceProvider = serviceProvider;
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());

        foreach (var property in GetType().GetProperties())
        {
            if (property.GetCustomAttribute(typeof(NotifyChangedIsLinkedToPropertyAttribute)) is NotifyChangedIsLinkedToPropertyAttribute customAttribute)
                AddPropertyDependencies(property.Name, customAttribute.LinkedProperties);
        }
    }

    ~ViewModelBase()
    {
        Dispose(false);
    }

    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void OnDispose()
    {
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;
        if (disposing)
            OnDispose();
        _isDisposed = true;
    }

    private void AddPropertyDependencies(string dependentPropertyName, ICollection<string> propertyNames)
    {
        if (string.IsNullOrEmpty(dependentPropertyName) || !propertyNames.Any())
            return;
        PropertyChanged += (_, args) =>
        {
            if (propertyNames.All(name => name != args.PropertyName))
                return;
            OnPropertyChanged(dependentPropertyName);
        };
    }
}