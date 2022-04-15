using System.Collections.Generic;

namespace Sklavenwalker.ProductMetadata.Services;

public interface IVariableResolver
{
    string ResolveVariables(string value, IDictionary<string, string?>? variables);
}