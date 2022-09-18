using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Helpers
{
    public static class StreamExtensions
    {
        public static async Task<byte[]> ToBytesAsync(this Stream value)
        {
            if (value is MemoryStream)
                return ((MemoryStream)value).ToArray();

            using var memoryStream = new MemoryStream();
            await value.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}