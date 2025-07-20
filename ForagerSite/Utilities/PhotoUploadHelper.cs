using DataAccess.Models;
using ForagerSite.Services;
using Microsoft.AspNetCore.Components.Forms;
using ForagerSite.Services;

namespace ForagerSite.Utilities
{
    public static class PhotoUploadHelper
    {
        private static readonly long maxFileSize = 100 * 1024 * 1024;

        private static readonly int maxFileCountFind = 4;

        private static readonly List<string> allowedExtensions = new()
        {
            ".jpg", ".jpeg", ".png"
        };
        public static async Task<string?> UploadProfilePic(List<string> errors, string userName, IBrowserFile? uploadedFile, IConfiguration config, UserService userService, UserViewModel userVm)
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

        public static async Task DeleteProfilePic(List<string> errors, string userName, IBrowserFile? uploadedFile, IConfiguration config, UserService userService, UserViewModel userVm)
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

        public static async Task<List<string>> UploadFindImages(List<string> base64Images, string userName, IConfiguration config, List<string> errors)
        {
            var savedFileUrls = new List<string>();
            var userDirectory = Path.Combine(config.GetValue<string>("FileStorageFind_Images"), userName);

            if (base64Images.Count > maxFileCountFind)
            {
                errors.Add($"You can upload a maximum of {maxFileCountFind} images.");
                return savedFileUrls;
            }

            if (!Directory.Exists(userDirectory))
            {
                Directory.CreateDirectory(userDirectory);
            }

            foreach (var base64Image in base64Images)
            {
                try
                {
                    var header = base64Image.Split(',')[0];
                    var imageBytes = Convert.FromBase64String(base64Image.Split(',')[1]);

                    if (imageBytes.Length > maxFileSize)
                    {
                        errors.Add("One or more images exceed the 5MB size limit.");
                        continue;
                    }

                    var extension = GetImageExtensionFromBase64(base64Image);
                    if (!allowedExtensions.Contains(extension))
                    {
                        errors.Add("Only .jpg, .jpeg, and .png formats are allowed.");
                        continue;
                    }

                    var fileName = Path.ChangeExtension(Path.GetRandomFileName(), extension);
                    var filePath = Path.Combine(userDirectory, fileName);

                    await File.WriteAllBytesAsync(filePath, imageBytes);
                    savedFileUrls.Add($"/UserFindImages/{userName}/{fileName}");
                }
                catch (Exception ex)
                {
                    errors.Add($"Error uploading image: {ex.Message}");
                }
            }

            return savedFileUrls;
        }

        private static string GetImageExtensionFromBase64(string base64)
        {
            if (base64.StartsWith("data:image/jpeg")) return ".jpg";
            if (base64.StartsWith("data:image/png")) return ".png";
            if (base64.StartsWith("data:image/jpg")) return ".jpg";
            return ".unknown";
        }
    }
}
