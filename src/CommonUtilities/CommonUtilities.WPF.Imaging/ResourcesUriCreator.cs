using System;
using System.Reflection;

namespace AnakinRaW.CommonUtilities.Wpf.Imaging;

public static class ResourcesUriCreator
{
    /// <summary>
    /// Create a Uri to an embedded file. The target assembly is taken from the assembly that calls this method.
    /// The embedded file location must be equivalent to: \Resources\Icons\ImageTypeAsString\TheFileName.ImageType
    /// </summary>
    /// <param name="fileName">The name of the file WITHOUT file extension</param>
    /// <param name="type">The file type</param>
    /// <param name="assembly">The assembly of the embedded file</param>
    /// <returns>The URI of the file</returns>
    public static Uri Create(string fileName, ImageFileKind type, Assembly assembly)
    {
        var prefix = string.Empty;
        if (type == ImageFileKind.Png)
            prefix = "pack://application:,,,";
        return new Uri(
            $"{prefix}/{assembly.GetName().Name};component/Resources/Icons/{type}/{fileName}.{type.ToString().ToLowerInvariant()}",
            UriKind.RelativeOrAbsolute);
    }

    /// <summary>
    /// Create a Uri to an embedded file. The target assembly is taken from the assembly that calls this method.
    /// The embedded file location must be equivalent to: \Resources\Icons\ImageTypeAsString\TheFileName.ImageType
    /// </summary>
    /// <param name="fileName">The name of the file WITHOUT file extension</param>
    /// <param name="type">The file type</param>
    /// <returns>The URI of the file</returns>
    public static Uri Create(string fileName, ImageFileKind type)
    {
        return Create(fileName, type, Assembly.GetCallingAssembly());
    }
}