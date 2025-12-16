using Azure.Storage.Blobs;
using DataAccess.Models;
using ForagerSite.DataContainer;
using ForagerSite.Services;
using ForagerSite.Services;
using ForagerSite.Services.Interfaces;
using Microsoft.AspNetCore.Components.Forms;
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
            if (errors.Any()) return null;

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
                // always store as jpg
                var newFileName = $"{Path.GetRandomFileName()}.jpg";

                var storageMode = config["ImageStorage"] ?? "Local";
                if (storageMode.Equals("Blob", StringComparison.OrdinalIgnoreCase))
                {
                    // --- BLOB MODE ---
                    var containerName = config["BlobStorage:ProfileContainer"] ?? "profile-images";
                    var blobName = $"profile-pics/{userName}/{newFileName}";

                    await using var readStream = uploadedFile.OpenReadStream(maxFileSize);
                    await using var resized = await ResizeToJpegStreamAsync(readStream, maxWidth: 256, quality: 70);

                    var blobUrl = await BlobImageHelper.UploadAsync(
                        resized,
                        blobName,
                        containerName,
                        config);

                    await userService.UploadProfilePicUrl(userVm.user, blobUrl);
                    return blobUrl;
                }
                else
                {
                    // --- LOCAL DISK MODE ---
                    var relativeFolder = config.GetValue<string>("FileStoragePf_Pics");
                    if (string.IsNullOrWhiteSpace(relativeFolder))
                    {
                        errors.Add("FileStoragePf_Pics is not configured.");
                        return null;
                    }

                    var rootFolder = Path.Combine(env.WebRootPath, relativeFolder);
                    var userDirectory = Path.Combine(rootFolder, userName);
                    Directory.CreateDirectory(userDirectory);

                    // Remove existing profile pics
                    foreach (var existingFile in Directory.GetFiles(userDirectory))
                        File.Delete(existingFile);

                    var filePath = Path.Combine(userDirectory, newFileName);

                    await using var readStream = uploadedFile.OpenReadStream(maxFileSize);
                    await ResizeAndSaveImageAsync(readStream, filePath, maxWidth: 256, quality: 70);

                    var fileUrl = $"/{relativeFolder}/{userName}/{newFileName}";
                    await userService.UploadProfilePicUrl(userVm.user, fileUrl);
                    return fileUrl;
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error uploading file: {ex.Message}");
                return null;
            }
        }


    public static async Task DeleteProfilePic(
    List<string> errors,
    string userName,
    IConfiguration config,
    IWebHostEnvironment env,
    IUserService userService,
    UserDataContainer userVm)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            errors.Add("User name is missing.");
            return;
        }

        try
        {
            var mode = config["ImageStorage"] ?? "Local";

            if (mode.Equals("Blob", StringComparison.OrdinalIgnoreCase))
            {
                // --- BLOB MODE ---
                var connStr = config["BlobStorage:ConnectionString"];
                var containerName = config["BlobStorage:ProfileContainer"] ?? "profile-images";

                if (string.IsNullOrWhiteSpace(connStr))
                {
                    errors.Add("Blob storage connection string missing.");
                    return;
                }

                var container = new BlobContainerClient(connStr, containerName);
                await container.CreateIfNotExistsAsync();

                // Must match how you named blobs on upload
                var prefix = $"profile-pics/{userName}/";

                await foreach (var blob in container.GetBlobsAsync(prefix: prefix))
                {
                    await container.GetBlobClient(blob.Name).DeleteIfExistsAsync();
                }
            }
            else
            {
                // --- LOCAL MODE ---
                var relativeFolder = config.GetValue<string>("FileStoragePf_Pics");
                if (string.IsNullOrWhiteSpace(relativeFolder))
                {
                    errors.Add("FileStoragePf_Pics is not configured.");
                    return;
                }

                var userDirectory = Path.Combine(env.WebRootPath, relativeFolder, userName);

                if (Directory.Exists(userDirectory))
                {
                    foreach (var file in Directory.GetFiles(userDirectory))
                        File.Delete(file);
                }
            }

            // Clear DB ref in both modes
            await userService.DeleteProfilePicUrl(userVm.user);
        }
        catch (Exception ex)
        {
            errors.Add($"Error deleting profile picture: {ex.Message}");
        }
    }


    public static async Task<List<string>> UploadFindImages(
        IReadOnlyList<IBrowserFile> files,
        string userName,
        IConfiguration config,
        IWebHostEnvironment env,
        List<string> errors)
        {
            var savedFileUrls = new List<string>();

            if (files == null || files.Count == 0)
                return savedFileUrls;

            if (files.Count > maxFileCountFind)
            {
                errors.Add($"You can upload a maximum of {maxFileCountFind} images.");
                return savedFileUrls;
            }

            if (string.IsNullOrWhiteSpace(userName))
            {
                errors.Add("User name is missing.");
                return savedFileUrls;
            }

            var storageMode = config["ImageStorage"] ?? "Local";

            // Local disk config (only used in Local mode)
            var relativeFolder = config.GetValue<string>("FileStorageFind_Images");

            // Blob config (only used in Blob mode)
            var findContainer = config["BlobStorage:FindContainer"] ?? "find-images";

            if (storageMode.Equals("Local", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(relativeFolder))
            {
                errors.Add("FileStorageFind_Images is not configured.");
                return savedFileUrls;
            }

            // Create local directory up front (Local mode)
            string? userDirectory = null;
            if (storageMode.Equals("Local", StringComparison.OrdinalIgnoreCase))
            {
                var rootFolder = Path.Combine(env.WebRootPath, relativeFolder!);
                userDirectory = Path.Combine(rootFolder, userName);
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

                    // Always store as jpg
                    var newFileName = $"{Path.GetRandomFileName()}.jpg";

                    await using var readStream = file.OpenReadStream(maxFileSize);

                    if (storageMode.Equals("Blob", StringComparison.OrdinalIgnoreCase))
                    {
                        // --- BLOB MODE ---
                        var blobName = $"{userName}/{newFileName}";

                        await using var resized = await ResizeToJpegStreamAsync(readStream, maxWidth: 1280, quality: 75);

                        var blobUrl = await BlobImageHelper.UploadAsync(
                            resized,
                            blobName,
                            findContainer,
                            config);

                        savedFileUrls.Add(blobUrl);
                    }
                    else
                    {
                        // --- LOCAL DISK MODE ---
                        var filePath = Path.Combine(userDirectory!, newFileName);

                        await ResizeAndSaveImageAsync(readStream, filePath, maxWidth: 1280, quality: 75);

                        var fileUrl = $"/{relativeFolder}/{userName}/{newFileName}";
                        savedFileUrls.Add(fileUrl);
                    }
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
        public static async Task<MemoryStream> ResizeToJpegStreamAsync(
        Stream input,
        int maxWidth = 1280,
        int quality = 75)
            {
            var output = new MemoryStream();

            using var image = await Image.LoadAsync(input);

            if (image.Width > 8000 || image.Height > 8000)
                throw new Exception("Image dimensions too large.");

            if (image.Width > maxWidth)
            {
                var ratio = (double)maxWidth / image.Width;
                var newHeight = (int)(image.Height * ratio);
                image.Mutate(x => x.Resize(maxWidth, newHeight));
            }

            var encoder = new JpegEncoder { Quality = quality };

            await image.SaveAsJpegAsync(output, encoder);
            output.Position = 0; // IMPORTANT before upload
            return output;
        }

    }
}
