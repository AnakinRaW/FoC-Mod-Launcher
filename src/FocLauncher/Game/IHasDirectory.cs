using System.IO;

namespace FocLauncher.Game
{
    public interface IHasDirectory
    {
        /// <summary>
        /// Returns a <see cref="DirectoryInfo"/> of the root directory.
        /// </summary>
        DirectoryInfo Directory { get; }
    }
}