using DataAccess.Models;
using ForagerSite.Services;
using ForagerSite.Services;
using Microsoft.AspNetCore.Components.Forms;
using ForagerSite.DataContainer;
using ForagerSite.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;


namespace ForagerSite.Utilities
{
    public static class PhotoUploadHelper
    {
        private static readonly long maxFileSize = 10 * 1024 * 1024;

        private static readonly int maxFileCountFind = 4;

        private static readonly List<string> allowedExtensions = new()
        {
            ".jpg", ".jpeg", ".png"
        };

        public static async Task<(string? PreviewUrl, IBrowserFile? UploadedFile)> GeneratePreviewAsync(InputFileChangeEventArgs e, List<string> errors)
        {
            errors.Clear();

            var file = e.File;

            if (e.FileCount > 1)
            {
                errors.Add($"Error: Attempting to upload {e.FileCount} images, but only 1 is allowed.");
                return (null, null);
            }

            if (file.Size > maxFileSize)
            {
                errors.Add($"File size exceeds the maximum allowed limit of {maxFileSize / (1024 * 1024)} MB.");
                return (null, null);
            }

            var fileExtension = Path.GetExtension(file.Name).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                errors.Add("Only image files (JPG, JPEG, PNG) are allowed.");
                return (null, null);
            }

            try
            {
                using var stream = file.OpenReadStream(maxFileSize);
                var buffer = new byte[file.Size];
                await stream.ReadAsync(buffer, 0, (int)file.Size);
                var previewUrl = $"data:{file.ContentType};base64,{Convert.ToBase64String(buffer)}";
                return (previewUrl, file);
            }
            catch (Exception ex)
            {
                errors.Add($"Error generating preview: {ex.Message}");
                return (null, null);
            }
        }
        public static async Task<string?> UploadProfilePic(
        List<string> errors,
        string userName,
        IBrowserFile? uploadedFile,
        IConfiguration config,
        IWebHostEnvironment env,
        IUserService userService,
        UserDataContainer userVm)
        { 
            if (errors.Any())
                return null;

            if (uploadedFile is null)
            {
                errors.Add("No file selected.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(userName))
            {
                errors.Add("User name is missing.");
                return null;
            }

            var relativeFolder = config.GetValue<string>("FileStoragePf_Pics");
            if (string.IsNullOrWhiteSpace(relativeFolder))
            {
                errors.Add("FileStoragePf_Pics is not configured.");
                return null;
            }

            var fileExtension = Path.GetExtension(uploadedFile.Name).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                errors.Add("Only image files (JPG, JPEG, PNG) are allowed.");
                return null;
            }

            if (uploadedFile.Size > maxFileSize)
            {
                errors.Add($"File size exceeds the maximum allowed limit of {maxFileSize / (1024 * 1024)} MB.");
                return null;
            }

            try
            {
                string newFileName = Path.ChangeExtension(Path.GetRandomFileName(), fileExtension);

                // Physical path (disk)
                string rootFolder = Path.Combine(env.WebRootPath, relativeFolder);
                string userDirectory = Path.Combine(rootFolder, userName);
                Directory.CreateDirectory(userDirectory);

                // Remove existing files
                foreach (var existingFile in Directory.GetFiles(userDirectory))
                    File.Delete(existingFile);

                // Save the new file (resized/compressed)
                newFileName = $"{Path.GetRandomFileName()}.jpg"; // recommended: always save as jpg
                string filePath = Path.Combine(userDirectory, newFileName);

                await using var readStream = uploadedFile.OpenReadStream(maxFileSize);
                await ResizeAndSaveImageAsync(
                    readStream,
                    filePath,
                    maxWidth: 256,   // profile pics should be small
                    quality: 70
                );


                // Public URL (what you store in DB)
                string fileUrl = $"/{relativeFolder}/{userName}/{newFileName}";
                await userService.UploadProfilePicUrl(userVm.user, fileUrl);

                return fileUrl;
            }
            catch (Exception ex)
            {
                errors.Add($"Error uploading file: {ex.Message}");
                return null;
            }
        }

        public static async Task DeleteProfilePic(List<string> errors, string userName, IBrowserFile? uploadedFile, IConfiguration config, IWebHostEnvironment env, IUserService userService, UserDataContainer userVm)
        {
            var file = uploadedFile; // Assume the file was saved during HandleFileChange

            //string userDirectory = Path.Combine(config.GetValue<string>("FileStoragePf_Pics"), userName);
            var relativeFolder = config.GetValue<string>("FileStoragePf_Pics");

            if (string.IsNullOrWhiteSpace(relativeFolder) || string.IsNullOrWhiteSpace(userName))
            {
                errors.Add("Profile picture storage is not configured correctly.");
                return;
            }

            var rootFolder = Path.Combine(env.WebRootPath, relativeFolder);
            var userDirectory = Path.Combine(rootFolder, userName);

            // Remove existing files if the directory exists
            if (Directory.Exists(userDirectory))
            {
                var existingFiles = Directory.GetFiles(userDirectory);
                foreach (var existingFile in existingFiles)
                {
                    File.Delete(existingFile);
                }
            }
            await userService.DeleteProfilePicUrl(userVm.user);
        }
        public static async Task<List<string>> UploadFindImages(IReadOnlyList<IBrowserFile> files, string userName, IConfiguration config, IWebHostEnvironment env, List<string> errors)
        {
            var savedFileUrls = new List<string>();

            if (files == null || files.Count == 0)
                return savedFileUrls;

            if (files.Count > maxFileCountFind)
            {
                errors.Add($"You can upload a maximum of {maxFileCountFind} images.");
                return savedFileUrls;
            }

            //var rootFolder = config.GetValue<string>("FileStorageFind_Images");
            //if (string.IsNullOrWhiteSpace(rootFolder))
            //{
            //    errors.Add("FileStorageFind_Images is not configured.");
            //    return savedFileUrls;
            //}
            var relativeFolder = config.GetValue<string>("FileStorageFind_Images");
            if (string.IsNullOrWhiteSpace(relativeFolder))
            {
                errors.Add("FileStorageFind_Images is not configured.");
                return savedFileUrls;
            }

            var rootFolder = Path.Combine(env.WebRootPath, relativeFolder);
            if (string.IsNullOrWhiteSpace(userName))
            {
                errors.Add("User name is missing.");
                return savedFileUrls;
            }

            var userDirectory = Path.Combine(rootFolder, userName);
            if (!Directory.Exists(userDirectory))
            {
                Directory.CreateDirectory(userDirectory);
            }

            foreach (var file in files)
            {
                try
                {
                    var ext = Path.GetExtension(file.Name).ToLowerInvariant();
                    if (!allowedExtensions.Contains(ext))
                    {
                        errors.Add($"File '{file.Name}' has an invalid extension.");
                        continue;
                    }

                    if (file.Size > maxFileSize)
                    {
                        errors.Add($"File '{file.Name}' exceeds size limit.");
                        continue;
                    }

                    var newFileName = $"{Path.GetRandomFileName()}.jpg";
                    var filePath = Path.Combine(userDirectory, newFileName);

                    await using var readStream = file.OpenReadStream(maxFileSize);
                    await ResizeAndSaveImageAsync(readStream, filePath);

                    savedFileUrls.Add($"/{relativeFolder}/{userName}/{newFileName}");
                }
                catch (Exception ex)
                {
                    errors.Add($"Error uploading '{file.Name}': {ex.Message}");
                }
            }


            return savedFileUrls;
        }
        public static async Task ResizeAndSaveImageAsync(
        Stream input,
        string outputPath,
        int maxWidth = 1280,
        int quality = 75)
        {
            using var image = await Image.LoadAsync(input);

            if (image.Width > 8000 || image.Height > 8000)
                throw new Exception("Image dimensions too large.");

            if (image.Width > maxWidth)
            {
                var ratio = (double)maxWidth / image.Width;
                var newHeight = (int)(image.Height * ratio);

                image.Mutate(x => x.Resize(maxWidth, newHeight));
            }

            var encoder = new JpegEncoder
            {
                Quality = quality
            };

            await image.SaveAsync(outputPath, encoder);
        }

    }
}
