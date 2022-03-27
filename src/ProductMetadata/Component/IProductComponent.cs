namespace Sklavenwalker.ProductMetadata.Component;

public interface IProductComponent : IProductComponentIdentity
{
    ComponentType Type { get; }

    DetectionState DetectedState { get; }
}