using System;
using System.IO;
using System.Threading.Tasks;

namespace AnakinRaW.ApplicationBase.Utilities;

public interface IResourceExtractor
{
    Task ExtractAsync(string resourceName, string fileDirectory, Func<string, Stream, bool> shouldOverwrite);

    Task ExtractAsync(string resourceName, string fileDirectory);

    Task<Stream> GetResourceAsync(string assemblyName);
}