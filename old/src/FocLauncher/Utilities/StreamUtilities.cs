using System.IO;
using System.Text;

namespace FocLauncher.Utilities
{
    internal static class StreamUtilities
    {
        public static byte[] ToByteArray(this Stream stream, Encoding encoding, bool leaveOpen, bool resetPosition = false)
        {
            byte[] bytes;
            using (var br = new BinaryReader(stream, encoding, leaveOpen))
                bytes = br.ReadBytes((int)stream.Length);
            if (resetPosition && leaveOpen)
                stream.Position = 0;
            return bytes;
        }
    }
}
