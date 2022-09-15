using CleanArchitecture.Core;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Helpers;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Extensions.FileStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using static CleanArchitecture.Server.MediaOptions;

namespace CleanArchitecture.Server.Controllers
{
    public class MediaController : ApiController
    {
        private readonly IFileStorage _fileStorage;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppSettings _appSettings;

        public MediaController(IFileStorage fileStorage, IUnitOfWork unitOfWork, IOptions<AppSettings> appSettings)
        {
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _appSettings = appSettings.Value ?? throw new ArgumentNullException(nameof(appSettings));
        }

        [AcceptVerbs("POST", "PATCH", Route = "media/{**catchAll}")]
        public async Task<IActionResult> Process()
        {
            var uploadName = Request.Headers["Upload-Name"].ToString();
            var uploadLength = long.Parse(Request.Headers["Upload-Length"]);
            var uploadExtension = Path.GetExtension(uploadName);
            var uploadMimeType = MimeTypeMap.GetMimeType(uploadName);

            var acceptFileTypes = (Request.Headers.GetCommaSeparatedValues("Accept-File-Types") ?? Array.Empty<string>());

            var mediaRule = _appSettings.Media.Rules.FirstOrDefault(
                   _ =>
                   _.Value.FileTypes.Intersect(!acceptFileTypes.Any() ? new[] { uploadExtension } : acceptFileTypes)
                   .Contains(uploadExtension, StringComparer.InvariantCultureIgnoreCase) ||

                   _.Value.FileTypes.Intersect(!acceptFileTypes.Any() ? new[] { uploadMimeType } : acceptFileTypes)
                   .Contains(uploadMimeType, StringComparer.InvariantCultureIgnoreCase)).Value;

            if (mediaRule == null)
                return ValidationProblem(title: "The file type is not supported.");

            if (uploadLength > mediaRule.FileSize)
                return ValidationProblem(title: "The file size is too large.");

            if (HttpMethods.IsPost(Request.Method))
            {
                var uploadPath = $"/media/{mediaRule.MediaType.ToString().ToLowerInvariant()}/{Algorithm.CreateCryptographicallySecureGuid()}{uploadExtension}";
                await _fileStorage.PrepareAsync(uploadPath);
                return Ok(uploadPath);
            }
            else if (HttpMethods.IsPatch(Request.Method))
            {
                var path = Request.Path.ToString();
                var uploadOffset = long.Parse(Request.Headers["Upload-Offset"]);

                using var chunkStream = new MemoryStream(await IO.ConvertToBytesAsync(Request.Body));
                using var fileStream = await _fileStorage.WriteAsync(path, chunkStream, uploadOffset, uploadLength);

                if (fileStream != null)
                {
                    var media = new Media();
                    media.Name = uploadName;
                    media.Path = path;
                    media.Size = uploadLength;
                    media.MimeType = uploadMimeType;
                    media.Type = mediaRule.MediaType;

                    _unitOfWork.Add(media);
                    await _unitOfWork.CompleteAsync();
                }

                return Ok();
            }
            else throw new InvalidOperationException();
        }

        [HttpDelete("media/{**catchAll}")]
        public async Task<IActionResult> Revert()
        {
            var path = Request.Path.ToString();
            await _fileStorage.DeleteAsync(path);

            var media = await _unitOfWork.Query<Media>().FirstOrDefaultAsync(_ => _.Path == path);
            if (media != null)
            {
                _unitOfWork.Remove(media);
                await _unitOfWork.CompleteAsync();
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }
    }
}