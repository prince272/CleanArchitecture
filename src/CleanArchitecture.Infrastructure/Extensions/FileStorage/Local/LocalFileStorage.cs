using Microsoft.Extensions.Options;

namespace CleanArchitecture.Infrastructure.Extensions.FileStorage.Local
{
    public class LocalFileStorage : IFileStorage
    {
        private readonly LocalFileStorageOptions _localFileStorageOptions;


        public LocalFileStorage(IOptions<LocalFileStorageOptions> localFileStorageOptions)
        {
            _localFileStorageOptions = localFileStorageOptions.Value ?? throw new ArgumentNullException(nameof(localFileStorageOptions));
        }

        public Task PrepareAsync(string path, CancellationToken cancellationToken = default)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var tempFilePath = GetTempFilePath(path);
            using var fileStream = new FileStream(tempFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            return Task.CompletedTask;
        }

        public async Task<Stream?> WriteAsync(string path, Stream stream, long offset, long length, CancellationToken cancellationToken = default)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            long tempFileLength = 0;
            var tempFilePath = GetTempFilePath(path);
            using (var tempFileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Write, FileShare.None))
            {
                tempFileStream.Seek(offset, SeekOrigin.Begin);
                await stream.CopyToAsync(tempFileStream, cancellationToken);
                tempFileLength = tempFileStream.Length;
            }

            if (tempFileLength == length)
            {
                var actualFilePath = GetActualFilePath(path);
                File.Move(tempFilePath, actualFilePath, false);
                return new FileStream(actualFilePath, FileMode.Open, FileAccess.Read, FileShare.None);
            }
            else if (tempFileLength > length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset exceed the expected length.");
            }
            else
            {
                return null;
            }
        }

        public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var tempFilePath = GetTempFilePath(path);

            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);

            var actualFilePath = GetActualFilePath(path);

            if (File.Exists(actualFilePath))
                File.Delete(actualFilePath);

            return Task.CompletedTask;
        }

        public string GetUrl(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            return $"{_localFileStorageOptions.RootUrl}{path.Replace("\\", "/")}";
        }

        string GetTempFilePath(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            return $"{GetActualFilePath(path)}.temp";
        }

        string GetActualFilePath(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            path = $"{_localFileStorageOptions.RootPath}{path.Replace("/", "\\")}";
            string sourceDirectory = Path.GetDirectoryName(path)!;

            if (!Directory.Exists(sourceDirectory))
                Directory.CreateDirectory(sourceDirectory);

            return path;
        }
    }
}