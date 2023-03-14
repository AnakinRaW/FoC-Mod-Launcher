using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Product;
using AnakinRaW.CommonUtilities.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.Storage;

internal abstract class FileRepository : IFileRepository
{
    private readonly ConcurrentDictionary<IInstallableComponent, IFileInfo> _componentStore = new(ProductComponentIdentityComparer.Default);
    private readonly IFileSystem _fileSystem;
    private readonly IFileSystemService _fileSystemHelper;
    private readonly IProductService _productService;

    protected abstract IDirectoryInfo Root { get; }

    protected virtual string FileExtensions => "new";

    protected FileRepository(IServiceProvider serviceProvider)
    {
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _fileSystemHelper = serviceProvider.GetRequiredService<IFileSystemService>();
        _productService = serviceProvider.GetRequiredService<IProductService>();
    }


    public IFileInfo AddComponent(IInstallableComponent component)
    {
        Requires.NotNull(component, nameof(component));
        return _componentStore.GetOrAdd(component, CreateComponentFile);
    }

    public IFileInfo? GetComponent(IInstallableComponent component)
    {
        Requires.NotNull(component, nameof(component));
        _componentStore.TryGetValue(component, out var file);
        return file;
    }

    public IDictionary<IInstallableComponent, IFileInfo> GetComponents()
    {
        return new Dictionary<IInstallableComponent, IFileInfo>(_componentStore, ProductComponentIdentityComparer.Default);
    }

    public ISet<IFileInfo> GetFiles()
    {
        return new HashSet<IFileInfo>(_componentStore.Values);
    }

    public void RemoveComponent(IInstallableComponent component)
    {
        if (!_componentStore.TryRemove(component, out var file))
            return;
        _fileSystemHelper.DeleteFileWithRetry(file);
    }

    public void Clear()
    {
        foreach (var storedComponent in _componentStore.Keys) 
            RemoveComponent(storedComponent);
    }

    private IFileInfo CreateComponentFile(IInstallableComponent component)
    {
        var namePrefix = GetNamePrefix(component);

        IFileInfo file = null!;
        for (var i = 0; i < 10; i++)
        {
            file = _fileSystem.FileInfo.New(CreateRandomFilePath(namePrefix));
            if (!file.Exists)
                break;
        }

        if (file is null || file.Exists)
            throw new InvalidOperationException("Unable to create a new random file after 10 retries");

        file.Create().Dispose();
        file.Refresh();
        Assumes.True(file.Exists);
        return file;
    }

    private string? GetNamePrefix(IInstallableComponent component)
    {
        if (component is not SingleFileComponent singleFileComponent)
            return null;
        var file = singleFileComponent.GetFile(_fileSystem, _productService.GetCurrentInstance().Variables);
        return file.Name;
    }


    private string CreateRandomFilePath(string? namePrefix)
    {
        var fileName = CreateFileName(namePrefix);
        return _fileSystem.Path.Combine(Root.FullName, fileName);
    }

    private string CreateFileName(string? prefix)
    {
        var randomFilePart = _fileSystem.Path.GetFileNameWithoutExtension(_fileSystem.Path.GetRandomFileName());
        if (string.IsNullOrEmpty(prefix) || string.IsNullOrWhiteSpace(prefix))
            return $"{randomFilePart}.{FileExtensions}";
        prefix = prefix!.TrimEnd('.');
        return $"{prefix}.{randomFilePart}.{FileExtensions}";
    }
}