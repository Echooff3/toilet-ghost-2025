using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using System.Text.RegularExpressions;

namespace ToiletGhost.Services;

public interface IBlobStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string userEmail, string projectName, long versionNumber);
    Task<(string originalBlobName, string thumbnailBlobName)> UploadImageWithThumbnailAsync(byte[] originalBytes, byte[] thumbnailBytes, string fileName, string contentType, string userEmail, string projectName, long versionNumber);
    Task<Uri> GetFileUrlAsync(string fileName, TimeSpan expiry);
    Task<bool> DeleteFileAsync(string fileName);
    Task<bool> FileExistsAsync(string fileName);
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _containerClient;
    private readonly string _containerName;

    public BlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureStorage") ?? 
                              configuration["AzureStorage:ConnectionString"];
        _containerName = configuration["AzureStorage:BlobContainerName"] ?? "toiletghost-files";
        
        _blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string userEmail, string projectName, long versionNumber)
    {
        // Sanitize filename: timestamp-member-project-version.extension
        var sanitizedUserEmail = SanitizeFileName(userEmail.Split('@')[0]);
        var sanitizedProjectName = SanitizeFileName(projectName);
        var extension = Path.GetExtension(fileName);
        
        var sanitizedFileName = $"{versionNumber}-{sanitizedUserEmail}-{sanitizedProjectName}-{versionNumber}{extension}";
        
        var blobClient = _containerClient.GetBlobClient(sanitizedFileName);
        
        var blobHttpHeaders = new BlobHttpHeaders
        {
            ContentType = contentType
        };

        await blobClient.UploadAsync(fileStream, new BlobUploadOptions
        {
            HttpHeaders = blobHttpHeaders
        });

        return sanitizedFileName;
    }

    public async Task<(string originalBlobName, string thumbnailBlobName)> UploadImageWithThumbnailAsync(
        byte[] originalBytes, 
        byte[] thumbnailBytes, 
        string fileName, 
        string contentType, 
        string userEmail, 
        string projectName, 
        long versionNumber)
    {
        // Sanitize filename: timestamp-member-project-version.extension
        var sanitizedUserEmail = SanitizeFileName(userEmail.Split('@')[0]);
        var sanitizedProjectName = SanitizeFileName(projectName);
        var extension = Path.GetExtension(fileName);
        
        // Create names for original and thumbnail
        var originalBlobName = $"{versionNumber}-{sanitizedUserEmail}-{sanitizedProjectName}-{versionNumber}{extension}";
        var thumbnailBlobName = $"{versionNumber}-{sanitizedUserEmail}-{sanitizedProjectName}-{versionNumber}_thumb.jpg";
        
        // Upload original image
        var originalBlobClient = _containerClient.GetBlobClient(originalBlobName);
        using (var originalStream = new MemoryStream(originalBytes))
        {
            var originalHeaders = new BlobHttpHeaders { ContentType = contentType };
            await originalBlobClient.UploadAsync(originalStream, new BlobUploadOptions
            {
                HttpHeaders = originalHeaders
            });
        }
        
        // Upload thumbnail
        var thumbnailBlobClient = _containerClient.GetBlobClient(thumbnailBlobName);
        using (var thumbnailStream = new MemoryStream(thumbnailBytes))
        {
            var thumbnailHeaders = new BlobHttpHeaders { ContentType = "image/jpeg" };
            await thumbnailBlobClient.UploadAsync(thumbnailStream, new BlobUploadOptions
            {
                HttpHeaders = thumbnailHeaders
            });
        }
        
        return (originalBlobName, thumbnailBlobName);
    }

    public Task<Uri> GetFileUrlAsync(string fileName, TimeSpan expiry)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);
        
        if (blobClient.CanGenerateSasUri)
        {
            var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(expiry));
            return Task.FromResult(sasUri);
        }

        // Fallback to the blob URL (this would require the container to be public)
        return Task.FromResult(blobClient.Uri);
    }

    public async Task<bool> DeleteFileAsync(string fileName)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);
        var response = await blobClient.DeleteIfExistsAsync();
        return response.Value;
    }

    public async Task<bool> FileExistsAsync(string fileName)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);
        var response = await blobClient.ExistsAsync();
        return response.Value;
    }

    private static string SanitizeFileName(string fileName)
    {
        // Remove unsafe characters and replace with underscore
        var sanitized = Regex.Replace(fileName, @"[^\w\-_.]", "_");
        return sanitized.Replace(" ", "_").ToLowerInvariant();
    }
}
