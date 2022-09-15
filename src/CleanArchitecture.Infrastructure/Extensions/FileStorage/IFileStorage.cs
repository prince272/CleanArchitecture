using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Extensions.FileStorage
{
    public interface IFileStorage
    {
        Task PrepareAsync(string path);

        Task<Stream?> WriteAsync(string path, Stream stream, long offset, long length);

        Task DeleteAsync(string path);
    }
}
