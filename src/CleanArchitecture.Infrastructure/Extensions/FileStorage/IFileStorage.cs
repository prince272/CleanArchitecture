namespace CleanArchitecture.Infrastructure.Extensions.FileStorage
{
    public interface IFileStorage
    {
        Task PrepareAsync(string path, CancellationToken cancellationToken = default);

        Task<Stream?> WriteAsync(string path, Stream stream, long offset, long length, CancellationToken cancellationToken = default);

        Task DeleteAsync(string path, CancellationToken cancellationToken = default);

        string GetUrl(string path);
    }
}
