namespace AnakinRaW.AppUpaterFramework.Metadata.Component;

public interface IProductComponent : IProductComponentIdentity
{
    string? Name { get; init; }

    ComponentType Type { get; }

    DetectionState DetectedState { get; set; }
}