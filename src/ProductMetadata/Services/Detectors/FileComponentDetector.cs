using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.ProductMetadata.Component;

namespace Sklavenwalker.ProductMetadata.Services.Detectors;

public sealed class FileComponentDetector : ComponentDetectorBase
{
    protected override ComponentType SupportedType => ComponentType.File;

    public FileComponentDetector(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override IProductComponent FindCore(IProductComponent manifestComponent, IInstalledProduct product)
    {
        if (manifestComponent is not SingleFileComponent fileComponent)
            throw new InvalidOperationException($"Component {manifestComponent.GetType().Name} is of wrong type. " +
                                                $"Expected {nameof(SingleFileComponent)}");

        if (fileComponent.OriginInfos.Count != 1)
            throw new InvalidOperationException("SingleFile component must have only one origin info");

        var variableResolver = ServiceProvider.GetService<IVariableResolver>() ?? VariableResolver.Default;

        var fileSystem = ServiceProvider.GetRequiredService<IFileSystem>();

        var originInfo = fileComponent.OriginInfos[0];
        var fileToDetect = fileSystem.Path.Combine(fileComponent.InstallPath, originInfo.FileName);

        var filePath = variableResolver.ResolveVariables(fileToDetect, product.ProductVariables.ToDictionary());

        var detectionState = DetectionState.Absent;
        if (fileSystem.File.Exists(filePath)) 
            detectionState = DetectionState.Present;

        var detectedFileComponent = new SingleFileComponent(manifestComponent, fileComponent.InstallPath, filePath)
        {
            DetectedState = detectionState
        };
        return detectedFileComponent;
    }
}