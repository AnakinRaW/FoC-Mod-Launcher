using System.Collections.Generic;

namespace AnakinRaW.AppUpaterFramework.Utilities;

internal interface IVariableResolver
{
    string ResolveVariables(string value, IDictionary<string, string?>? variables);
}