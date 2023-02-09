namespace AnakinRaW.AppUpaterFramework.Metadata.Component;

public interface IProductComponent : IProductComponentIdentity
{
    ComponentType Type { get; }

    DetectionState DetectedState { get; set; }
}