using System.IO;

namespace FocLauncherHost.Updater
{
    internal static class StreamExtensions
    {
        internal static Stream ToStream(this string @this)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(@this);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
