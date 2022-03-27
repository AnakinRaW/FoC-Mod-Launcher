using Sklavenwalker.ProductMetadata.Component;

namespace Sklavenwalker.ProductMetadata.Services.Detectors;

public interface IComponentDetector
{
    IProductComponent Find(IProductComponent manifestComponent, IInstalledProduct product);
}