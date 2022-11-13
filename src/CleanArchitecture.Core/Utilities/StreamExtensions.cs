namespace CleanArchitecture.Core.Utilities
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