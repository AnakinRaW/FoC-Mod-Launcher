using System.Collections.Generic;

namespace AnakinRaW.AppUpdaterFramework.Metadata.Component;

public interface IComponentGroup : IProductComponent
{
    IReadOnlyList<IProductComponentIdentity> Components { get; } 
}