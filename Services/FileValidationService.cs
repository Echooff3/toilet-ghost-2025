namespace ToiletGhost.Services;

public interface IFileValidationService
{
    bool IsValidAudioFile(string fileName, long fileSizeBytes);
    bool IsValidImageFile(string fileName, long fileSizeBytes);
    string GetContentType(string fileName);
}

public class FileValidationService : IFileValidationService
{
    private readonly string[] _allowedAudioExtensions = { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".m4a", ".wma" };
    private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
    
    private const long MaxAudioSizeBytes = 75 * 1024 * 1024; // 75 MB
    private const long MaxImageSizeBytes = 15 * 1024 * 1024;  // 15 MB

    public bool IsValidAudioFile(string fileName, long fileSizeBytes)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return _allowedAudioExtensions.Contains(extension) && fileSizeBytes <= MaxAudioSizeBytes;
    }

    public bool IsValidImageFile(string fileName, long fileSizeBytes)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return _allowedImageExtensions.Contains(extension) && fileSizeBytes <= MaxImageSizeBytes;
    }

    public string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".flac" => "audio/flac",
            ".aac" => "audio/aac",
            ".ogg" => "audio/ogg",
            ".m4a" => "audio/mp4",
            ".wma" => "audio/x-ms-wma",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}
