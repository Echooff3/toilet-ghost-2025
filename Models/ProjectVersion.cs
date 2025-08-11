using Azure;
using Azure.Data.Tables;

namespace ToiletGhost.Models;

public class ProjectVersion : ITableEntity
{
    public string PartitionKey { get; set; } = "versions";
    public string RowKey { get; set; } = string.Empty; // ProjectId-Timestamp
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string ProjectId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long VersionNumber { get; set; } // Unix timestamp
    public long FileSizeBytes { get; set; }
    public string FileType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ProjectVersion() { }

    public ProjectVersion(string projectId, string fileName, long fileSizeBytes, string fileType)
    {
        ProjectId = projectId;
        FileName = fileName;
        FileSizeBytes = fileSizeBytes;
        FileType = fileType;
        VersionNumber = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        RowKey = $"{projectId}-{VersionNumber}";
    }
}
