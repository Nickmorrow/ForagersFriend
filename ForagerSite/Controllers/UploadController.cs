using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ForagerSite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        private const long _maxFileSize = 1024 * 1024 * 3; //3mb

        private const int _maxAllowedFiles = 4;

        private List<string> _errors = new();
        public UploadController(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _config = config;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFiles([FromForm] string userName)
        {
            var files = Request.Form.Files;

            if (files.Count > _maxAllowedFiles)
            {
                return BadRequest("Cannot upload more than 4 files");
            }

            var uploadedFileUrls = new List<string>();

            foreach (var file in files)
            {
                if (file.Length > _maxFileSize)
                {
                    return BadRequest("Each file must be less than 3MB");
                }

                string newFileName = Path.ChangeExtension(
                    Path.GetRandomFileName(),
                    Path.GetExtension(file.FileName));

                string userDirectory = Path.Combine(_config.GetValue<string>("FileStorage"), userName);
                Directory.CreateDirectory(userDirectory);

                string filePath = Path.Combine(userDirectory, newFileName);

                await using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fs);
                }

                string fileUrl = Path.Combine("/FindImageUploads", userName, newFileName);
                uploadedFileUrls.Add(fileUrl);
            }

            return Ok(uploadedFileUrls); // Return the list of URLs
        }
    }

}
