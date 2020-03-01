using System.IO;

namespace FocLauncherHost.Utilities
{
    internal static class StreamExtensions
    {
        internal static Stream ToStream(this string input)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(input);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
