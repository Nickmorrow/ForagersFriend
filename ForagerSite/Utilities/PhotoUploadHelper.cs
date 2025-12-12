using DataAccess.Models;
using ForagerSite.Services;
using ForagerSite.Services;
using Microsoft.AspNetCore.Components.Forms;
using ForagerSite.DataContainer;


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
        public static async Task<string?> UploadProfilePic(List<string> errors, string userName, IBrowserFile? uploadedFile, IConfiguration config, IUserService userService, UserViewModel userVm)
        {
            if (errors.Any())
            {
                return null; // Prevent saving if there are validation errors
            }

            var file = uploadedFile; // Assume the file was saved during HandleFileChange
            var fileExtension = Path.GetExtension(file.Name).ToLowerInvariant();

            try
            {
                string newFileName = Path.ChangeExtension(Path.GetRandomFileName(), fileExtension);
                string userDirectory = Path.Combine(config.GetValue<string>("FileStoragePf_Pics"), userName);
                string filePath = Path.Combine(userDirectory, newFileName);

                if (!Directory.Exists(userDirectory))
                {
                    Directory.CreateDirectory(userDirectory);
                }
                // Remove existing files if the directory exists
                if (Directory.Exists(userDirectory))
                {
                    var existingFiles = Directory.GetFiles(userDirectory);
                    foreach (var existingFile in existingFiles)
                    {
                        File.Delete(existingFile);
                    }
                }
                // Save the new file
                await using var fs = new FileStream(filePath, FileMode.Create);
                await file.OpenReadStream(maxFileSize).CopyToAsync(fs);

                // Generate file URL and update database
                string fileUrl = $"/UserProfileImages/{userName}/{newFileName}";
                await userService.UploadProfilePicUrl(userVm.user, fileUrl);

                return fileUrl; // Return the URL of the uploaded file

            }
            catch (Exception ex)
            {
                errors.Add($"Error uploading file: {ex.Message}");
                return null;
            }
        }
        public static async Task DeleteProfilePic(List<string> errors, string userName, IBrowserFile? uploadedFile, IConfiguration config, IUserService userService, UserViewModel userVm)
        {
            var file = uploadedFile; // Assume the file was saved during HandleFileChange

            string userDirectory = Path.Combine(config.GetValue<string>("FileStoragePf_Pics"), userName);

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
        public static async Task<List<string>> UploadFindImages(IReadOnlyList<IBrowserFile> files, string userName, IConfiguration config, List<string> errors)
        {
            var savedFileUrls = new List<string>();

            if (files == null || files.Count == 0)
                return savedFileUrls;

            if (files.Count > maxFileCountFind)
            {
                errors.Add($"You can upload a maximum of {maxFileCountFind} images.");
                return savedFileUrls;
            }

            var rootFolder = config.GetValue<string>("FileStorageFind_Images");
            if (string.IsNullOrWhiteSpace(rootFolder))
            {
                errors.Add("FileStorageFind_Images is not configured.");
                return savedFileUrls;
            }

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
                        errors.Add($"File '{file.Name}' has an invalid extension. Only .jpg, .jpeg, .png are allowed.");
                        continue;
                    }

                    if (file.Size > maxFileSize)
                    {
                        errors.Add($"File '{file.Name}' exceeds the {maxFileSize / (1024 * 1024)} MB limit.");
                        continue;
                    }

                    var newFileName = Path.ChangeExtension(Path.GetRandomFileName(), ext);
                    var filePath = Path.Combine(userDirectory, newFileName);

                    await using var readStream = file.OpenReadStream(maxFileSize);
                    await using var writeStream = new FileStream(filePath, FileMode.Create);
                    await readStream.CopyToAsync(writeStream);

                    savedFileUrls.Add($"/FindImageUploads/{userName}/{newFileName}");
                }
                catch (Exception ex)
                {
                    errors.Add($"Error uploading '{file.Name}': {ex.Message}");
                }
            }

            return savedFileUrls;
        }

    }
}
