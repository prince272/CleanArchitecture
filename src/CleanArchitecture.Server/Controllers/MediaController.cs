using AutoMapper;
using CleanArchitecture.Core;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Utilities;
using CleanArchitecture.Infrastructure.Extensions.EmailSender;
using CleanArchitecture.Infrastructure.Extensions.FileStorage;
using CleanArchitecture.Infrastructure.Extensions.SmsSender;
using CleanArchitecture.Infrastructure.Extensions.ViewRenderer;
using CleanArchitecture.Server.Extensions.Authentication;
using CleanArchitecture.Server.Extensions.Hosting;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Server.Controllers
{
    public class MediaController : ApiController
    {
        private readonly UserManager<User> _userManager;
        private readonly AppSettings _appSettings;
        private readonly IFileStorage _fileStorage;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MediaController(
            UserManager<User> userManager,
            IOptions<AppSettings> appSettings,
            IFileStorage fileStorage,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _appSettings = appSettings.Value ?? throw new ArgumentNullException(nameof(appSettings));
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [Authorize]
        [AcceptVerbs("POST", "PATCH", Route = "media/{**catchAll}")]
        public async Task<IActionResult> ProcessMedia()
        {
            var uploadName = Request.Headers["Upload-Name"].ToString();
            var uploadLength = Request.Headers["Upload-Length"].To<long>();
            var uploadExtension = Path.GetExtension(uploadName).ToLowerInvariant();
            var uploadMimeType = MimeTypeMap.GetMimeType(uploadName);

            var fileExtensions = _appSettings.MediaSettings.Rules.SelectMany(_ => _.Value.FileExtensions).ToArray();
            var acceptedFileExtensions = (Request.Headers.GetCommaSeparatedValues("Accept-File-Extensions") ?? Array.Empty<string>());
            acceptedFileExtensions = acceptedFileExtensions.Any() ? acceptedFileExtensions.Intersect(fileExtensions).ToArray() : fileExtensions;

            var mediaRule = _appSettings.MediaSettings.Rules.FirstOrDefault(_ => acceptedFileExtensions.Intersect(_.Value.FileExtensions)
            .Contains(uploadExtension, StringComparer.InvariantCultureIgnoreCase)).Value;

            if (mediaRule == null)
                return ValidationProblem(title: "File Not Allowed", detail: $"The file you were trying to upload is not allowed.");

            if (uploadLength > mediaRule.FileSize)
                return ValidationProblem(title: "File Too Large", detail: $"The file you were trying to upload is larger then {mediaRule.FileSize.Bytes().Humanize()}.");

            if (HttpMethods.IsPost(Request.Method))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null) throw new InvalidOperationException($"Value cannot be null.");

                var uploadPath = $"/media/{mediaRule.MediaType.ToString().ToLowerInvariant()}/{currentUser.Id}/{Algorithm.CreateCryptographicallySecureGuid()}{uploadExtension}";
                await _fileStorage.PrepareAsync(uploadPath);
                return Ok(uploadPath);
            }
            else if (HttpMethods.IsPatch(Request.Method))
            {
                var path = Request.Path.ToString();
                var uploadOffset = Request.Headers["Upload-Offset"].To<long>();

                using var chunkStream = new MemoryStream(await Request.Body.ToBytesAsync());
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

        [Authorize]
        [HttpDelete("media/{**catchAll}")]
        public async Task<IActionResult> RevertMedia()
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
