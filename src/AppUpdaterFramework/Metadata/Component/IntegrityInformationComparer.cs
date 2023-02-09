using System;
using System.Collections.Generic;
using System.Linq;

namespace AnakinRaW.AppUpaterFramework.Metadata.Component;

public class IntegrityInformationComparer : IEqualityComparer<ComponentIntegrityInformation>
{
    public bool Equals(ComponentIntegrityInformation x, ComponentIntegrityInformation y)
    {
        return x.HashType == y.HashType && x.Hash.SequenceEqual(y.Hash);
    }

    public int GetHashCode(ComponentIntegrityInformation obj)
    {
        return HashCode.Combine(obj.HashType, obj.Hash);
    }
}