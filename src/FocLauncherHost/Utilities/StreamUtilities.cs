using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FocLauncherHost.Utilities
{
    internal static class StreamUtilities
    {
        public static async Task CopyFileToStreamAsync(string filePath, Stream stream, CancellationToken cancellation = default)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath));
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            await fileStream.CopyToAsync(stream, 81920, cancellation);
        }
    }
}
