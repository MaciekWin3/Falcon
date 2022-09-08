using System.IO.Compression;
using System.Text;

namespace Falcon.Server.Utils
{
    public static class StringCompression
    {
        public static string Compress(string uncompressedString)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(uncompressedString);
            using var outputStream = new MemoryStream();
            using var gZipStream = new GZipStream(outputStream, CompressionMode.Compress);
            gZipStream.Write(inputBytes, 0, inputBytes.Length);
            var outputBytes = outputStream.ToArray();
            var outputStr = Encoding.UTF8.GetString(outputBytes);
            var outputbase64 = Convert.ToBase64String(outputBytes);
            return outputbase64;
        }

        public static string Decompress(string compressedString)
        {
            byte[] inputBytes = Convert.FromBase64String(compressedString);
            using var inputStream = new MemoryStream(inputBytes);
            using var gZipStream = new GZipStream(inputStream, CompressionMode.Decompress);
            using var streamReader = new StreamReader(gZipStream);
            var decompressedString = streamReader.ReadToEnd();
            return decompressedString;
        }
    }
}