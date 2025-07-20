using DataAccess.Models;
using ForagerSite.Services;
using Microsoft.AspNetCore.Components.Forms;
using ForagerSite.Services;

namespace ForagerSite.Utilities
{
    public static class PhotoUploadHelper
    {
        private static readonly long maxFileSize = 100 * 1024 * 1024;
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

    }
}
