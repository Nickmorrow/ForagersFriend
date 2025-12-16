using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

public static class BlobImageHelper
{
    public static async Task<string> UploadAsync(Stream imageStream, string blobName, string containerName, IConfiguration config)
    {
        var connStr = config["BlobStorage:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connStr))
            throw new InvalidOperationException("BlobStorage:ConnectionString is missing.");

        if (imageStream.CanSeek) imageStream.Position = 0;

        var container = new BlobContainerClient(connStr, containerName);
        await container.CreateIfNotExistsAsync();

        var blob = container.GetBlobClient(blobName);

        await blob.DeleteIfExistsAsync();
        await blob.UploadAsync(
            imageStream,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = "image/jpeg" }
            });

        return blob.Uri.ToString();
    }

}
public static class BlobDeleteHelper
{
    public static async Task DeleteByUrlIfExistsAsync(string blobUrl, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(blobUrl)) return;
        if (!Uri.TryCreate(blobUrl, UriKind.Absolute, out var uri)) return;

        var parts = uri.AbsolutePath.TrimStart('/').Split('/', 2);
        if (parts.Length < 2) return;

        var containerName = parts[0];
        var blobName = parts[1];

        var container = new BlobContainerClient(connectionString, containerName);
        // optional but safe:
        // await container.CreateIfNotExistsAsync();

        await container.GetBlobClient(blobName).DeleteIfExistsAsync();
    }

}
