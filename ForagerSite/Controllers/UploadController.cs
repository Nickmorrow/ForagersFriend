using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;

namespace ForagerSite.Controllers
{

    public class DeleteFileRequest
    {
        public string FileUrl { get; set; }
        public string UserName { get; set; }
    }


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

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            foreach (var file in files)
            {
                if (file.Length > _maxFileSize)
                {
                    return BadRequest("Each file must be less than 3MB");
                }

                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Only image files (JPG, PNG, GIF) are allowed.");
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

                //string fileUrl = Path.Combine("/FindImageUploads", userName, newFileName);
                string fileUrl = $"/FindImageUploads/{userName}/{newFileName}";
                uploadedFileUrls.Add(fileUrl);
            }

            return Ok(uploadedFileUrls); // Return the list of URLs
        }

        [HttpPost("delete")]
        public IActionResult DeleteFile([FromBody] DeleteFileRequest request)
        {
            if (string.IsNullOrEmpty(request.FileUrl) || string.IsNullOrEmpty(request.UserName))
            {
                return BadRequest("Invalid file URL or user name.");
            }

            // Extract the file name from the URL
            var fileName = Path.GetFileName(request.FileUrl);
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("Invalid file URL.");
            }

            // Construct the file path
            string userDirectory = Path.Combine(_config.GetValue<string>("FileStorage"), request.UserName);
            string filePath = Path.Combine(userDirectory, fileName);

            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found.");
            }

            try
            {
                // Delete the file                

                System.IO.File.Delete(filePath);
                return Ok(new { success = true, message = "File deleted successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting file: {ex.Message}");
            }
        }

    }

}
