using Microsoft.AspNetCore.Components.Forms;

namespace ForagerSite.Utilities;

public static class PhotoUploadHelper
{
    private const long MaximumFileSize = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png"
    };

    public static async Task<(string? PreviewUrl, IBrowserFile? UploadedFile)> GeneratePreviewAsync(
        InputFileChangeEventArgs args,
        List<string> errors)
    {
        errors.Clear();
        var file = args.File;

        if (args.FileCount > 1)
        {
            errors.Add($"Error: Attempting to upload {args.FileCount} images, but only 1 is allowed.");
            return (null, null);
        }

        if (file.Size <= 0 || file.Size > MaximumFileSize)
        {
            errors.Add($"File size exceeds the maximum allowed limit of {MaximumFileSize / (1024 * 1024)} MB.");
            return (null, null);
        }

        if (!AllowedExtensions.Contains(Path.GetExtension(file.Name)))
        {
            errors.Add("Only image files (JPG, JPEG, PNG) are allowed.");
            return (null, null);
        }

        try
        {
            await using var stream = file.OpenReadStream(MaximumFileSize);
            using var buffer = new MemoryStream((int)file.Size);
            await stream.CopyToAsync(buffer);
            var previewUrl = $"data:{file.ContentType};base64,{Convert.ToBase64String(buffer.ToArray())}";
            return (previewUrl, file);
        }
        catch (Exception exception)
        {
            errors.Add($"Error generating preview: {exception.Message}");
            return (null, null);
        }
    }
}
