using System.Collections.Generic;

namespace AnakinRaW.AppUpdaterFramework.Utilities;

internal interface IVariableResolver
{
    string ResolveVariables(string value, IDictionary<string, string?>? variables);
}