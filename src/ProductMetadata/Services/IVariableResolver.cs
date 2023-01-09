using System.Collections.Generic;

namespace AnakinRaW.ProductMetadata.Services;

public interface IVariableResolver
{
    string ResolveVariables(string value, IDictionary<string, string?>? variables);
}