using Sklavenwalker.ProductMetadata.Component;

namespace Sklavenwalker.ProductMetadata.Services;

public interface IComponentConverter<in TModel, out TComponent> where TComponent : IProductComponentIdentity
{
    TComponent Convert(TModel metaModel);
}