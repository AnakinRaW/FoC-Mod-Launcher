using System.Collections.Generic;

namespace AnakinRaW.AppUpaterFramework.Metadata.Component;

public interface IComponentGroup : IProductComponent
{
    IReadOnlyList<IProductComponentIdentity> Components { get; } 
}