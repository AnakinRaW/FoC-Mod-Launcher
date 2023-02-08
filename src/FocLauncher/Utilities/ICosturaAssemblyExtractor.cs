using System.Collections.Generic;
using System.Threading.Tasks;

namespace FocLauncher.Utilities;

internal interface ICosturaAssemblyExtractor
{
    Task ExtractAssembliesAsync(IEnumerable<string> assemblyNames, string fileDirectory);

    Task ExtractAssemblyAsync(string assemblyName, string fileDirectory);
}