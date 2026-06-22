using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Movie_StructureCode.Application.Abstractions.Services.Storage
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string pathPrefix, string contentType, CancellationToken cancellationToken);
        string? GetAbsoluteUrl(string? relativePath, string? pathPrefix);
    }
}
