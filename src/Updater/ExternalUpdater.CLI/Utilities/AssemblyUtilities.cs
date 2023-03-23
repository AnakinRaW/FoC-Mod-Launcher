using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace AnakinRaW.ExternalUpdater.CLI.Utilities;

internal static class AssemblyUtilities
{
    public static string? GetAttributeCtorString(this IEnumerable<CustomAttribute> attributes, Type type, int ctorParamIndex = 0)
    {
        var attribute = attributes.FirstOrDefault(x => x.AttributeType.FullName.Equals(type.FullName));
        if (attribute is null || !attribute.HasConstructorArguments)
            return null;

        var argument = attribute.ConstructorArguments[ctorParamIndex];
        if (argument.Type.MetadataType != MetadataType.String)
            return null;

        return argument.Value as string;
    }
}